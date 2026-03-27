import * as SecureStore from "expo-secure-store";

// ARCH-7, project-context rule 6: NEVER use AsyncStorage for auth tokens.
// AsyncStorage is unencrypted. expo-secure-store maps to:
//   - iOS: Keychain Services
//   - Android: Android Keystore System
//
// Token contract established in Story 2-0 (OAuth2/OIDC Foundation):
//   access_token  — 15-minute JWT, validated remotely by Blinder.Api via OIDC discovery
//   refresh_token — 30-day rolling, rotated on each use by OpenIddict
//   token_expiry  — UTC milliseconds; used for pre-refresh (5-min window) without a server round-trip

const KEYS = {
  ACCESS_TOKEN: "access_token",
  REFRESH_TOKEN: "refresh_token",
  TOKEN_EXPIRY: "token_expiry",
} as const;

export const storageService = {
  async getAccessToken(): Promise<string | null> {
    return SecureStore.getItemAsync(KEYS.ACCESS_TOKEN);
  },

  async getRefreshToken(): Promise<string | null> {
    return SecureStore.getItemAsync(KEYS.REFRESH_TOKEN);
  },

  /**
   * Persists the token pair returned by POST /api/auth/oauth/token.
   * @param accessToken  The OAuth2 access token (JWT).
   * @param refreshToken The OAuth2 refresh token.
   * @param expiresIn    Seconds until access token expiry (from the `expires_in` response field).
   */
  async setTokens(
    accessToken: string,
    refreshToken: string,
    expiresIn: number
  ): Promise<void> {
    const expiry = Date.now() + expiresIn * 1000;
    await Promise.all([
      SecureStore.setItemAsync(KEYS.ACCESS_TOKEN, accessToken),
      SecureStore.setItemAsync(KEYS.REFRESH_TOKEN, refreshToken),
      SecureStore.setItemAsync(KEYS.TOKEN_EXPIRY, String(expiry)),
    ]);
  },

  /**
   * Removes all auth tokens — call on logout or on unrecoverable auth error.
   * Always call this regardless of revocation result (project-context rule 22).
   */
  async clearTokens(): Promise<void> {
    await Promise.all([
      SecureStore.deleteItemAsync(KEYS.ACCESS_TOKEN),
      SecureStore.deleteItemAsync(KEYS.REFRESH_TOKEN),
      SecureStore.deleteItemAsync(KEYS.TOKEN_EXPIRY),
    ]);
  },

  /**
   * Returns true if the stored access token will expire within 5 minutes.
   * Use this to trigger a silent refresh before making API calls.
   * Returns true (treat as expired) if no expiry is stored.
   */
  async isTokenExpiringSoon(): Promise<boolean> {
    const expiry = await SecureStore.getItemAsync(KEYS.TOKEN_EXPIRY);
    if (!expiry) return true;
    const fiveMinutes = 5 * 60 * 1000;
    return Date.now() > Number(expiry) - fiveMinutes;
  },
} as const;
