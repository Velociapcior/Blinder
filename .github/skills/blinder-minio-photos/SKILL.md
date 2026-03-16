---
name: blinder-minio-photos
description: |
  Handles all photo storage logic for the Blinder app using self-hosted MinIO on Hetzner VPS.
  Use this skill when: uploading profile photos, generating presigned URLs, implementing photo
  reveal access control, managing photo lifecycle (pending/approved/revealed), integrating
  CSAM scanning before persistence, or revoking photo access after unmatching.
  Triggers: "photo upload", "presigned URL", "MinIO", "photo reveal", "photo access", "CSAM scan".
---

# Blinder MinIO Photo Storage

## Overview

Blinder uses self-hosted **MinIO** (Docker Compose on Hetzner VPS) for object storage.
Photos are never served directly — all access is via **short-lived presigned URLs** generated
by the backend. Access control is enforced at URL-generation time based on reveal state.

## Bucket Structure

```
blinder-photos/
├── pending/        # Uploaded, awaiting CSAM scan — never served to clients
├── approved/       # Scanned and clean — access controlled by reveal state
└── deleted/        # Soft-deleted, retained briefly for audit then purged
```

## Critical Rule: No Persistence Before CSAM Scan

Photos MUST follow this exact sequence:
1. Client requests a presigned **upload** URL from the API
2. Client uploads directly to MinIO `pending/` bucket
3. API triggers CSAM scan (PhotoDNA or equivalent) asynchronously
4. On clean result → API moves object to `approved/` bucket
5. On flagged result → API deletes object and notifies moderation

**Never** generate a presigned download URL for anything in `pending/`.

## MinIO Client Setup (.NET)

```csharp
// Registration in Program.cs
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var config = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
    return new MinioClient()
        .WithEndpoint(config.Endpoint)
        .WithCredentials(config.AccessKey, config.SecretKey)
        .WithSSL(config.UseSSL)
        .Build();
});
```

```csharp
// MinioOptions (appsettings.json / environment variables)
public class MinioOptions
{
    public string Endpoint { get; set; } = string.Empty;   // e.g. minio:9000
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool UseSSL { get; set; } = false;              // true in production
    public string BucketName { get; set; } = "blinder-photos";
    public int PresignedUrlExpirySeconds { get; set; } = 300; // 5 minutes
}
```

## Presigned Upload URL (Client → MinIO direct upload)

```csharp
public async Task<string> GenerateUploadUrlAsync(string userId, CancellationToken ct)
{
    var objectKey = $"pending/{userId}/{Guid.NewGuid():N}.jpg";

    var url = await _minioClient.PresignedPutObjectAsync(
        new PresignedPutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithExpiry(_options.PresignedUrlExpirySeconds),
        ct);

    // Store pending reference in DB for scan tracking
    await _photoRepository.CreatePendingAsync(userId, objectKey, ct);

    return url;
}
```

## Presigned Download URL (Reveal-Gated)

```csharp
public async Task<string?> GeneratePhotoUrlAsync(
    string requestingUserId,
    string targetUserId,
    CancellationToken ct)
{
    var revealState = await _revealRepository
        .GetStateAsync(requestingUserId, targetUserId, ct);

    // Only generate URL if mutual reveal is confirmed
    if (revealState != RevealState.MutuallyRevealed)
        return null;

    var objectKey = await _photoRepository
        .GetApprovedKeyAsync(targetUserId, ct);

    if (objectKey is null) return null;

    return await _minioClient.PresignedGetObjectAsync(
        new PresignedGetObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithExpiry(_options.PresignedUrlExpirySeconds),
        ct);
}
```

## Moving Object After CSAM Scan

```csharp
public async Task ApprovePendingPhotoAsync(string objectKey, CancellationToken ct)
{
    var approvedKey = objectKey.Replace("pending/", "approved/");

    // MinIO copy + delete (no native move)
    await _minioClient.CopyObjectAsync(
        new CopyObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(approvedKey)
            .WithCopyObjectSource(
                new CopySourceObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(objectKey)),
        ct);

    await _minioClient.RemoveObjectAsync(
        new RemoveObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey),
        ct);

    await _photoRepository.MarkApprovedAsync(objectKey, approvedKey, ct);
}
```

## Photo Revocation (Unmatch / Account Deletion)

When users unmatch or delete their account:
- Do **not** delete the MinIO object immediately (retain for moderation audit)
- Update DB record to mark photo as revoked
- Presigned URL generation logic already checks reveal state — revoked photos
  return `null` naturally without needing object deletion

```csharp
public async Task RevokePhotoAccessAsync(string userId, CancellationToken ct)
{
    await _photoRepository.RevokeAsync(userId, ct);
    // No MinIO deletion here — URL gating handles access control
    // Schedule hard delete via background job after retention period
}
```

## Environment Variables (Docker Compose)

```yaml
environment:
  Minio__Endpoint: "minio:9000"
  Minio__AccessKey: "${MINIO_ACCESS_KEY}"
  Minio__SecretKey: "${MINIO_SECRET_KEY}"
  Minio__UseSSL: "false"
  Minio__BucketName: "blinder-photos"
  Minio__PresignedUrlExpirySeconds: "300"
```

## Key Rules

- NEVER serve photos from `pending/` bucket
- NEVER hardcode MinIO credentials — always use environment variables
- ALWAYS gate presigned URL generation on reveal state from DB
- ALWAYS use short expiry (≤5 min) on presigned URLs
- Presigned URLs are the ONLY mechanism for photo delivery — no public bucket ACLs
- Photo object keys include userId for easy per-user audit/purge
