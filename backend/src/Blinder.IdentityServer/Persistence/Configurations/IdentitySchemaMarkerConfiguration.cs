using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blinder.IdentityServer.Persistence.Configurations;

internal sealed class IdentitySchemaMarkerConfiguration : IEntityTypeConfiguration<IdentitySchemaMarker>
{
    /// <summary>
    /// Maps the identity boundary marker table used to bootstrap isolated migrations safely.
    /// </summary>
    public void Configure(EntityTypeBuilder<IdentitySchemaMarker> builder)
    {
        builder.ToTable(IdentityPersistenceDefaults.SchemaMarkerTable);
        builder.HasKey(marker => marker.Id);
        builder.Property(marker => marker.Id).ValueGeneratedOnAdd();
    }
}