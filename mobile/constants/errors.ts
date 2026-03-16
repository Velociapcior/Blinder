// All user-facing error message strings.
// project-context rule 10: NEVER expose raw exception messages or stack traces in UI.
// Error strings come from here ONLY — never inline string literals in components.
// Future stories add domain-specific error keys.

export const ERRORS = {
  NETWORK_ERROR: "Brak połączenia. Sprawdź połączenie z internetem.",
  UNEXPECTED_ERROR: "Coś poszło nie tak. Spróbuj ponownie.",
  SESSION_EXPIRED: "Sesja wygasła. Zaloguj się ponownie.",
  INVALID_CREDENTIALS: "Nieprawidłowy e-mail lub hasło.",
  EMAIL_ALREADY_REGISTERED: "Konto z tym adresem e-mail już istnieje.",
  INVITE_REQUIRED: "Rejestracja kobiet wymaga ważnego linku zaproszenia.",
  INVALID_INVITE: "Ten link zaproszenia jest nieprawidłowy lub został już użyty.",
  CONVERSATION_LIMIT: "Osiągnąłeś maksymalną liczbę aktywnych rozmów.",
  REVEAL_THRESHOLD_NOT_MET: "Potrzeba więcej wiadomości, zanim ujawnienie będzie dostępne.",
  PHOTO_SCAN_FAILED: "Nie można przetworzyć zdjęcia. Spróbuj inne zdjęcie.",
  UPLOAD_FAILED: "Przesyłanie zdjęcia nie powiodło się. Spróbuj ponownie.",
} as const;

export type ErrorKey = keyof typeof ERRORS;
