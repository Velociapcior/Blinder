export interface AuthService {
  getAccessToken(): Promise<string | null>
}

export const authService: AuthService = {
  async getAccessToken() {
    return null
  },
}