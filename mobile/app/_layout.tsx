import "../global.css";

import { SafeAreaProvider } from "react-native-safe-area-context";
import { Stack } from "expo-router";

// AccessibilityContext comes in Story 1.5 — placeholder structure here
export default function RootLayout() {
  return (
    <SafeAreaProvider>
      <Stack
        screenOptions={{
          headerShown: false,
          contentStyle: { backgroundColor: "#1A1814" },
        }}
      />
    </SafeAreaProvider>
  );
}
