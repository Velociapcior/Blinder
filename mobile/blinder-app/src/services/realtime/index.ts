export interface RealtimeService {
  connect(): Promise<void>
  disconnect(): Promise<void>
}

export const realtimeService: RealtimeService = {
  async connect() {},
  async disconnect() {},
}