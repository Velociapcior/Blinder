import type { ReactNode } from 'react'
import { useEffect, useState } from 'react'

import { DefaultTheme, ThemeProvider } from '@react-navigation/native'
import { StatusBar } from 'expo-status-bar'
import { useFonts } from 'expo-font'
import {
  Lato_300Light,
  Lato_400Regular,
  Lato_700Bold,
  Lato_900Black,
} from '@expo-google-fonts/lato'
import { View } from 'react-native'
import { SafeAreaProvider } from 'react-native-safe-area-context'
import { TamaguiProvider } from 'tamagui'

import { config } from '../../tamagui.config'
import { palette } from '../design-system/palette'

type AppProvidersProps = {
  children: ReactNode
}

export function AppProviders({ children }: AppProvidersProps) {
  // MVP: light theme only — dark is a post-MVP token swap per UX spec
  // Lato weights 300/400/700/900 loaded for native; web uses Google Fonts CSS via +html.tsx
  const [fontsLoaded, fontError] = useFonts({
    Lato_300Light,
    Lato_400Regular,
    Lato_700Bold,
    Lato_900Black,
  })
  const [fontLoadTimedOut, setFontLoadTimedOut] = useState(false)

  useEffect(() => {
    if (fontsLoaded || fontError) {
      return undefined
    }

    const timeoutHandle = globalThis.setTimeout(() => {
      setFontLoadTimedOut(true)
    }, 1500)

    return () => {
      globalThis.clearTimeout(timeoutHandle)
    }
  }, [fontError, fontsLoaded])

  useEffect(() => {
    if (fontError) {
      console.warn('Failed to load Lato fonts, continuing with fallback fonts.', fontError)
    }
  }, [fontError])

  useEffect(() => {
    if (fontLoadTimedOut && !fontsLoaded && !fontError) {
      console.warn('Lato fonts are taking too long to load, continuing with fallback fonts.')
    }
  }, [fontError, fontLoadTimedOut, fontsLoaded])

  if (!fontsLoaded && !fontError && !fontLoadTimedOut) {
    // Hold bg colour while fonts load — avoids white flash on native
    return <View style={{ flex: 1, backgroundColor: palette.bgBase }} />
  }

  return (
    <SafeAreaProvider>
      <TamaguiProvider config={config} defaultTheme="light">
        <ThemeProvider value={DefaultTheme}>
          <StatusBar style="dark" />
          {children}
        </ThemeProvider>
      </TamaguiProvider>
    </SafeAreaProvider>
  )
}
