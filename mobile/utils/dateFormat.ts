// All datetimes from API are ISO 8601 UTC strings (project-context rule 3).
// Display formatting uses Polish locale (tech-preferences section 16).
// NEVER accept Unix timestamps — always ISO 8601 input.

/**
 * Format a message timestamp for display in chat.
 * Returns HH:MM in Polish locale.
 */
export function formatMessageTime(iso8601: string): string {
  const date = new Date(iso8601);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  return new Intl.DateTimeFormat("pl-PL", {
    hour: "2-digit",
    minute: "2-digit",
  }).format(date);
}

/**
 * Format a conversation's last activity date.
 * Today: returns time only (HH:MM).
 * Earlier: returns date (DD.MM.YYYY).
 */
export function formatConversationDate(iso8601: string): string {
  const date = new Date(iso8601);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  const today = new Date();

  const isToday =
    date.getDate() === today.getDate() &&
    date.getMonth() === today.getMonth() &&
    date.getFullYear() === today.getFullYear();

  if (isToday) {
    return new Intl.DateTimeFormat("pl-PL", {
      hour: "2-digit",
      minute: "2-digit",
    }).format(date);
  }

  return new Intl.DateTimeFormat("pl-PL", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(date);
}
