import { SafeAreaView } from 'react-native-safe-area-context'
import { Paragraph, Separator, Text, YStack } from 'tamagui'

export function WaitingStateScreen() {
  return (
    <SafeAreaView style={{ flex: 1 }} edges={['top', 'right', 'bottom', 'left']}>
      <YStack flex={1} px="$6" py="$6" gap="$5" bg="$background">
        <YStack gap="$3">
          <Text fontSize="$8" fontWeight="700" maxFontSizeMultiplier={1.3}>
            Mobile foundation ready
          </Text>
          <Paragraph color="$color11" maxFontSizeMultiplier={1.3}>
            Navigation is configured for a stack-only Expo Router shell.
          </Paragraph>
        </YStack>

        <Separator />

        <YStack gap="$3">
          <Paragraph maxFontSizeMultiplier={1.3}>
            Feature modules now live under `src/features`, and cross-cutting integration
            stubs live under `src/services`.
          </Paragraph>
          <Paragraph color="$color11" maxFontSizeMultiplier={1.3}>
            Product screens, tokens, auth wiring, and realtime behavior are intentionally
            deferred to later stories.
          </Paragraph>
        </YStack>
      </YStack>
    </SafeAreaView>
  )
}