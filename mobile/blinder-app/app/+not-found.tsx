import { Link, Stack } from 'expo-router'
import { View, Text } from 'tamagui'

export default function NotFoundScreen() {
  return (
    <>
      <Stack.Screen options={{ title: 'Oops!' }} />
      <View m="$5" gap="$sm">
        <Text>This screen doesn't exist.</Text>
        <Link href="/">
          <Text color="$accent">Go to home screen!</Text>
        </Link>
      </View>
    </>
  )
}
