import "../global.css";

import { SafeAreaProvider } from "react-native-safe-area-context";
import { Stack } from "expo-router";
import { AccessibilityProvider } from "../contexts/AccessibilityContext";

export default function RootLayout() {
  return (
    <SafeAreaProvider>
      <AccessibilityProvider>
        <Stack
          screenOptions={{
            headerShown: false,
            contentStyle: { backgroundColor: "#1A1814" },
          }}
        />
      </AccessibilityProvider>
    </SafeAreaProvider>
  );
}
