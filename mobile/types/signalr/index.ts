// SignalR hub method names and payload types.
// Hub method names are PascalCase on server; must match backend Hubs/ChatHub.cs EXACTLY.
// IMPORTANT: types/api/ and types/signalr/ are SEPARATE namespaces.
// Drift between them causes silent bugs (tech-preferences section 1).

/** Server-to-client hub method names. Must match ChatHub.cs exactly. */
export const HubMethods = {
  // Story 5.1: ReceiveMessage
  RECEIVE_MESSAGE: "ReceiveMessage",
  // Story 6.3: RevealStateUpdated — broadcast when BOTH reveal_ready flags are set (ARCH-13)
  REVEAL_STATE_UPDATED: "RevealStateUpdated",
  // Story 4.3: MatchAssigned — triggered by MatchGenerationJob
  MATCH_ASSIGNED: "MatchAssigned",
} as const;

export type HubMethodName = (typeof HubMethods)[keyof typeof HubMethods];

// Payload stubs — full types implemented per story:

/** Story 5.1 */
export interface ReceiveMessagePayload {
  conversationId: string;
  messageId: string;
  senderId: string;
  content: string;
  sentAt: string; // ISO 8601 UTC
}

/** Story 6.3 */
export interface RevealStateUpdatedPayload {
  conversationId: string;
  userAReady: boolean;
  userBReady: boolean;
}

/** Story 4.3 */
export interface MatchAssignedPayload {
  conversationId: string;
  matchedAt: string; // ISO 8601 UTC
}
