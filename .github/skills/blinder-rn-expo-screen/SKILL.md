---
name: blinder-rn-expo-screen
description: |
  Scaffold and implement React Native screens for the Blinder app using Expo SDK 55.
  Use this skill when: creating new screens, implementing navigation between screens,
  building form components, connecting screens to the API, handling loading/error states,
  implementing real-time updates via SignalR, or managing local state with Zustand.
  Triggers: "new screen", "React Native", "Expo", "navigation", "screen component",
  "mobile screen", "RN component".
---

# Blinder React Native Screen Patterns (Expo SDK 55)

## Tech Stack

- **Expo SDK 55** (managed workflow)
- **React Navigation v6** — stack + tab navigators
- **TanStack Query v5** — server state, caching, background refetch
- **Zustand** — client/UI state (auth, reveal flags, chat draft)
- **Axios** — HTTP client with interceptors for JWT refresh
- **@microsoft/signalr** — real-time hub connection
- **NativeWind** — Tailwind-style styling for React Native

## File Structure Per Screen

```
src/screens/
└── MatchScreen/
    ├── index.tsx          # Screen component (exported)
    ├── MatchScreen.tsx    # Main implementation
    ├── useMatchData.ts    # TanStack Query hook
    └── types.ts           # Screen-local types (if needed)
```

## Screen Template

```tsx
// MatchScreen.tsx
import React from 'react';
import { View, ScrollView, ActivityIndicator } from 'react-native';
import { useMatchData } from './useMatchData';
import { ErrorMessage } from '@/components/ErrorMessage';
import { styled } from 'nativewind';

const StyledView = styled(View);
const StyledScrollView = styled(ScrollView);

interface Props {
  matchId: string;
}

export function MatchScreen({ matchId }: Props) {
  const { data: match, isLoading, error, refetch } = useMatchData(matchId);

  if (isLoading) {
    return (
      <StyledView className="flex-1 items-center justify-center">
        <ActivityIndicator size="large" />
      </StyledView>
    );
  }

  if (error) {
    return <ErrorMessage message={error.message} onRetry={refetch} />;
  }

  return (
    <StyledScrollView className="flex-1 bg-background p-4">
      {/* Screen content */}
    </StyledScrollView>
  );
}
```

## TanStack Query Hook Pattern

```ts
// useMatchData.ts
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/api/client';
import type { Match } from '@/types';

export function useMatchData(matchId: string) {
  return useQuery({
    queryKey: ['match', matchId],
    queryFn: () => apiClient.get<Match>(`/matches/${matchId}`).then(r => r.data),
    staleTime: 1000 * 30, // 30 seconds
    retry: 2,
  });
}

// Mutation pattern (e.g., sending reveal request)
export function useRequestReveal() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (matchId: string) =>
      apiClient.post(`/matches/${matchId}/reveal`).then(r => r.data),
    onSuccess: (_, matchId) => {
      queryClient.invalidateQueries({ queryKey: ['match', matchId] });
    },
  });
}
```

## Navigation Setup

```tsx
// Navigation types (src/navigation/types.ts)
export type RootStackParamList = {
  Onboarding: undefined;
  Main: undefined;
  Chat: { matchId: string };
  PhotoReveal: { matchId: string };
};

// Typed navigation hook usage in screens
import { useNavigation } from '@react-navigation/native';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';

type NavProp = NativeStackNavigationProp<RootStackParamList>;

export function SomeScreen() {
  const navigation = useNavigation<NavProp>();
  // navigation.navigate('Chat', { matchId: '123' });
}
```

## Zustand Store Pattern

```ts
// src/stores/authStore.ts
import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import AsyncStorage from '@react-native-async-storage/async-storage';

interface AuthState {
  token: string | null;
  userId: string | null;
  setToken: (token: string, userId: string) => void;
  clearAuth: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      userId: null,
      setToken: (token, userId) => set({ token, userId }),
      clearAuth: () => set({ token: null, userId: null }),
    }),
    {
      name: 'auth-storage',
      storage: createJSONStorage(() => AsyncStorage),
    }
  )
);
```

## SignalR Integration (Chat/Reveal Updates)

```ts
// src/hooks/useSignalR.ts
import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '@/stores/authStore';
import { useQueryClient } from '@tanstack/react-query';

export function useChatHub(matchId: string) {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const token = useAuthStore(s => s.token);
  const queryClient = useQueryClient();

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${process.env.EXPO_PUBLIC_API_URL}/hubs/chat`, {
        accessTokenFactory: () => token ?? '',
      })
      .withAutomaticReconnect()
      .build();

    connection.on('MessageReceived', (message) => {
      queryClient.setQueryData(['messages', matchId], (old: any[]) =>
        old ? [...old, message] : [message]
      );
    });

    connection.on('RevealStateChanged', () => {
      queryClient.invalidateQueries({ queryKey: ['match', matchId] });
    });

    connection.start().catch(console.error);
    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [matchId, token]);
}
```

## API Client Setup

```ts
// src/api/client.ts
import axios from 'axios';
import { useAuthStore } from '@/stores/authStore';

export const apiClient = axios.create({
  baseURL: process.env.EXPO_PUBLIC_API_URL,
  timeout: 10000,
});

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().clearAuth();
      // Navigate to login handled by root navigator auth state listener
    }
    return Promise.reject(error);
  }
);
```

## Environment Variables (Expo)

```
# .env.local
EXPO_PUBLIC_API_URL=http://localhost:5000
```

## Key Rules

- ALWAYS use `EXPO_PUBLIC_` prefix for env vars exposed to the client
- NEVER store sensitive data in AsyncStorage unencrypted (use expo-secure-store for tokens if needed)
- ALWAYS handle `isLoading`, `error`, and empty states in every screen
- Use TanStack Query for ALL server state — no manual `useState` + `useEffect` for API calls
- SignalR connections belong in hooks, NOT in screen components directly
- Navigation params must be typed via `RootStackParamList`
