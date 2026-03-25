import { apiClient } from "./apiClient";
import type { RegisterRequest } from "../types/api";

/**
 * Auth API service for mobile.
 * All calls route to the backend's Identity-backed endpoints — never to
 * scaffolded Razor UI pages (project rule #17).
 */
export const authService = {
  /**
   * Register a new user via POST /api/auth/register.
   * Returns void on HTTP 202 Accepted for both new accounts and duplicate emails
   * (OWASP anti-enumeration pattern — never reveals whether the address is registered).
   * Throws Error with ProblemDetails title on non-duplicate failures (e.g. password policy).
   */
  register: (payload: RegisterRequest): Promise<void> =>
    apiClient.post("/auth/register", payload),
};
