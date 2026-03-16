import { Tabs } from "expo-router";

// Tab bar configuration comes in later stories when actual screens exist
export default function TabsLayout() {
  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarStyle: { backgroundColor: "#1A1814" },
      }}
    />
  );
}
