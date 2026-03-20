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

// --- Story 2.1 ---

/** Payload sent to POST /api/auth/register */
export interface RegisterRequest {
  email: string;
  password: string;
  /** Must not be 0 (Unspecified). Backend enum: Male=1, Female=2, NonBinary=3 */
  gender: 1 | 2 | 3;
  /** Must be true; backend returns 400 if false. Pass the actual form value — never hardcode. */
  over18Declaration: boolean;
}

// Future types added per story:
// Story 3.1: QuizResponse, QuizSubmissionRequest
// Story 4.x: MatchDto, ConversationSummaryDto
// Story 5.x: MessageDto, SendMessageRequest
// Story 6.x: RevealStateDto, RevealReadyRequest
