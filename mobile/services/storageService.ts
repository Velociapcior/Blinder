import * as SecureStore from "expo-secure-store";

// ARCH-7, project-context rule 6: NEVER use AsyncStorage for auth tokens.
// AsyncStorage is unencrypted. expo-secure-store maps to:
//   - iOS: Keychain Services
//   - Android: Android Keystore System

const KEYS = {
  JWT_TOKEN: "blinder_jwt_token",
  REFRESH_TOKEN: "blinder_refresh_token",
} as const;

export const storageService = {
  async saveToken(token: string): Promise<void> {
    await SecureStore.setItemAsync(KEYS.JWT_TOKEN, token);
  },

  async getToken(): Promise<string | null> {
    return SecureStore.getItemAsync(KEYS.JWT_TOKEN);
  },

  async deleteToken(): Promise<void> {
    await SecureStore.deleteItemAsync(KEYS.JWT_TOKEN);
  },

  async saveRefreshToken(token: string): Promise<void> {
    await SecureStore.setItemAsync(KEYS.REFRESH_TOKEN, token);
  },

  async getRefreshToken(): Promise<string | null> {
    return SecureStore.getItemAsync(KEYS.REFRESH_TOKEN);
  },

  async deleteRefreshToken(): Promise<void> {
    await SecureStore.deleteItemAsync(KEYS.REFRESH_TOKEN);
  },

  async clearAll(): Promise<void> {
    await Promise.all([
      SecureStore.deleteItemAsync(KEYS.JWT_TOKEN),
      SecureStore.deleteItemAsync(KEYS.REFRESH_TOKEN),
    ]);
  },
} as const;
