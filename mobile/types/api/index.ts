// All API response types must match backend DTOs exactly.
// Backend uses DateTimeOffset → ISO 8601 strings in API responses (project-context rule 3).
// Populated story-by-story as endpoints are implemented.

/**
 * RFC 7807 Problem Details — shape returned by backend on all 4xx/5xx errors.
 * Backend uses AddProblemDetails() + AppErrors.cs (ARCH-10).
 */
export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
}

// Future types added per story:
// Story 2.1: AuthTokenDto, RegisterRequest, LoginRequest
// Story 3.1: QuizResponse, QuizSubmissionRequest
// Story 4.x: MatchDto, ConversationSummaryDto
// Story 5.x: MessageDto, SendMessageRequest
// Story 6.x: RevealStateDto, RevealReadyRequest
