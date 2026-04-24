import { Redirect } from 'expo-router'
import { SafeAreaView } from 'react-native-safe-area-context'

import { TokenShowcaseScrollView } from '../../src/design-system/TokenShowcase'

// Dev-only route: EXPO_PUBLIC_ENABLE_TOKEN_SHOWCASE=true must be set, or app runs in dev mode.
// Gate prevents the route from being accessible in production builds.
const isEnabled =
  process.env.EXPO_PUBLIC_ENABLE_TOKEN_SHOWCASE === 'true' || __DEV__

export default function TokensRoute() {
  if (!isEnabled) {
    return <Redirect href="/" />
  }

  return (
    <SafeAreaView style={{ flex: 1 }} edges={['top', 'right', 'bottom', 'left']}>
      <TokenShowcaseScrollView />
    </SafeAreaView>
  )
}
