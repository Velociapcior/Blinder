import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { storageService } from "./storageService";

// SINGLETON — module-level, never instantiated per component (UX-DR20, tech-preferences 10.4)
// Components NEVER manage connection state directly.
// Components subscribe via hooks ONLY: useConversation, useRevealState

let connection: HubConnection | null = null;
let startPromise: Promise<void> | null = null;

const HUB_URL = `${
  (process.env.EXPO_PUBLIC_API_URL ?? "http://localhost/api").replace(
    /\/api$/,
    ""
  )
}/hubs/chat`;

export const signalrService = {
  async start(): Promise<void> {
    if (connection?.state === HubConnectionState.Connected) {
      return;
    }

    if (startPromise !== null) {
      await startPromise;
      return;
    }

    if (connection === null) {
      connection = new HubConnectionBuilder()
        .withUrl(HUB_URL, {
          accessTokenFactory: async () => (await storageService.getToken()) ?? "",
        })
        .withAutomaticReconnect() // Handles VPS restarts and network drops
        .configureLogging(LogLevel.Warning)
        .build();
    }

    if (connection.state !== HubConnectionState.Disconnected) {
      return;
    }

    startPromise = connection.start();
    try {
      await startPromise;
    } finally {
      startPromise = null;
    }
  },

  async stop(): Promise<void> {
    if (startPromise !== null) {
      await startPromise.catch(() => undefined);
    }

    await connection?.stop();
    connection = null;
    startPromise = null;
  },

  getConnection(): HubConnection | null {
    return connection;
  },

  isConnected(): boolean {
    return connection?.state === HubConnectionState.Connected;
  },
} as const;
