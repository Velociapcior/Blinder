import { defaultConfig } from '@tamagui/config/v5'
import { createFont, createTamagui, createTokens } from 'tamagui'

// Warm Dusk colour tokens — one-to-one from _bmad-output/design-system/colors_and_type.css
const warmDusk = {
  bgBase: '#FBF5EE',
  bgSurface: '#EDE3D8',
  bgElevated: '#F5EDE2',
  bgDark: '#2C1C1A',
  primary: '#8B4E6E',
  primaryLight: '#B87A98',
  // color.reveal — RESERVED: Reveal gate option and mutual reveal ceremony only
  reveal: '#D4A85A',
  accent: '#C4825A',
  textPrimary: '#2C1C1A',
  textSecondary: '#7A5A52',
  textMuted: '#A08878',
  border: '#DDD0C4',
  error: '#B85050',
  offline: '#9A9090',
  onPrimary: '#FFFFFF',
  onReveal: '#2C1C1A',
  onDark: '#F6EEE5',
} as const

// Lato — 4 weights matching colors_and_type.css (300 / 400 / 700 / 900)
// Android face mappings in `face` ensure correct weight rendering per platform
const latoFont = createFont({
  family: 'Lato, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif',
  size: {
    // Tamagui numeric scale (required by built-in component $size props)
    1: 11, 2: 12, 3: 13, 4: 14, 5: 15, 6: 16, 7: 17, 8: 20,
    9: 24, 10: 32, 11: 40, 12: 48, 13: 56, 14: 64, 15: 72, 16: 80,
    true: 15,
    // Blinder semantic type scale from colors_and_type.css
    caption: 11,
    button: 14,
    bodySm: 13,
    body: 15,
    h3: 17,
    h2: 20,
    h1: 24,
    display: 32,
  },
  lineHeight: {
    1: 16.5, 2: 18, 3: 20, 4: 21, 5: 24.75, 6: 26, 7: 23.8, 8: 27,
    9: 31.2, 10: 38.4, 11: 50, 12: 58, 13: 67, 14: 77, 15: 86, 16: 96,
    true: 24.75,
    caption: 16.5,   // 11 × 1.5
    button: 14,      // 14 × 1.0 (tight, per spec)
    bodySm: 20.8,    // 13 × 1.6
    body: 24.75,     // 15 × 1.65
    h3: 23.8,        // 17 × 1.4
    h2: 27,          // 20 × 1.35
    h1: 31.2,        // 24 × 1.3
    display: 38.4,   // 32 × 1.2
  },
  weight: {
    1: '400', 2: '400', 3: '400', 4: '700', 5: '400', 6: '400',
    7: '700', 8: '700', 9: '700', 10: '900',
    11: '900', 12: '900', 13: '900', 14: '900', 15: '900', 16: '900',
    true: '400',
    caption: '300',
    button: '700',
    bodySm: '400',
    body: '400',
    h3: '700',
    h2: '700',
    h1: '700',
    display: '900',
  },
  letterSpacing: {
    // +0.04em only for button (0.56px) and caption (0.44px); zero everywhere else
    1: 0, 2: 0, 3: 0, 4: 0.56, 5: 0, 6: 0, 7: 0, 8: 0,
    9: 0, 10: 0, 11: 0, 12: 0, 13: 0, 14: 0, 15: 0, 16: 0,
    true: 0,
    caption: 0.44,
    button: 0.56,
    bodySm: 0,
    body: 0,
    h3: 0,
    h2: 0,
    h1: 0,
    display: 0,
  },
  face: {
    // Android explicit face mappings — required for correct weight rendering on Android
    // Names must match font assets loaded via expo-font in AppProviders
    300: { normal: 'Lato_300Light' },
    400: { normal: 'Lato_400Regular' },
    700: { normal: 'Lato_700Bold' },
    900: { normal: 'Lato_900Black' },
  },
})

// Token set: Tamagui v5 numeric scale + Blinder semantic tokens
// Numeric space/size/radius values are reconstructed from tamagui.generated.css
const tokens = createTokens({
  color: {
    ...warmDusk,
    transparent: 'rgba(0,0,0,0)',
    white: '#FFFFFF',
    black: '#000000',
  },
  space: {
    // Tamagui v5 numeric scale — keeps existing components working ($4, $5, $6 etc.)
    0: 0, 0.25: 0.5, 0.5: 1, 0.75: 1.5,
    1: 2, 1.5: 4, 2: 7, 2.5: 10, 3: 13, 3.5: 16, 4: 18, 4.5: 21,
    5: 24, 6: 32, 7: 39, 8: 46, 9: 53, 10: 60, 11: 74, 12: 88,
    13: 102, 14: 116, 15: 130, 16: 144, 17: 144, 18: 158, 19: 172, 20: 186,
    true: 18,
    // Blinder semantic spacing — 8pt grid from colors_and_type.css
    xs: 4, sm: 8, md: 16, lg: 24, xl: 32, '2xl': 48,
  },
  size: {
    // Tamagui v5 size scale
    0: 0, 0.25: 2, 0.5: 4, 0.75: 8,
    1: 20, 1.5: 24, 2: 28, 2.5: 32, 3: 36, 3.5: 40, 4: 44, 4.5: 48,
    5: 52, 6: 64, 7: 74, 8: 84, 9: 94, 10: 104, 11: 124, 12: 144,
    13: 164, 14: 184, 15: 204, 16: 224, 17: 224, 18: 244, 19: 264, 20: 284,
    true: 44,
  },
  radius: {
    // Tamagui v5 radius scale
    0: 0, 1: 3, 2: 5, 3: 7, 4: 9, 5: 10, 6: 16, 7: 19, 8: 22, 9: 26,
    10: 34, 11: 42, 12: 50,
    true: 9,
    // Blinder semantic radius from colors_and_type.css
    sm: 8, md: 14, lg: 18, xl: 20, full: 9999,
  },
  zIndex: {
    0: 0, 1: 100, 2: 200, 3: 300, 4: 400, 5: 500,
  },
})

// Blinder light theme — Warm Dusk colour mapping to Tamagui semantic keys
// Verified contrast: text.primary (#2C1C1A) on bg.base (#FBF5EE) = 15.06:1
// WCAG AAA — far exceeds AA minimum of 4.5:1
const blinderLight = {
  // Standard Tamagui theme keys used by built-in components
  background: warmDusk.bgBase,
  backgroundHover: warmDusk.bgElevated,
  backgroundPress: warmDusk.bgSurface,
  backgroundFocus: warmDusk.bgElevated,
  backgroundStrong: warmDusk.bgSurface,
  backgroundTransparent: 'rgba(251,245,238,0)',
  color: warmDusk.textPrimary,
  colorHover: warmDusk.textPrimary,
  colorPress: warmDusk.textPrimary,
  colorFocus: warmDusk.primary,
  colorTransparent: 'rgba(44,28,26,0)',
  // Numbered colour scale (1=darkest, 12=lightest) — used by Tamagui component colour props
  color1: warmDusk.textPrimary,
  color2: '#3A2A26',
  color3: '#4A3830',
  color4: '#5A4840',
  color5: warmDusk.textSecondary,
  color6: '#8A6A60',
  color7: warmDusk.textMuted,
  color8: '#B09888',
  color9: '#C0A898',
  color10: warmDusk.border,
  color11: warmDusk.textSecondary,   // used in WaitingStateScreen scaffold
  color12: warmDusk.bgSurface,
  borderColor: warmDusk.border,
  borderColorHover: warmDusk.primaryLight,
  borderColorFocus: warmDusk.primary,
  borderColorPress: warmDusk.primary,
  shadowColor: 'rgba(44, 28, 26, 0.14)',
  shadowColorHover: 'rgba(44, 28, 26, 0.22)',
  placeholderColor: warmDusk.textMuted,
  // Blinder semantic theme keys — accessed as $bgBase, $primary, $reveal, etc.
  bgBase: warmDusk.bgBase,
  bgSurface: warmDusk.bgSurface,
  bgElevated: warmDusk.bgElevated,
  bgDark: warmDusk.bgDark,
  primary: warmDusk.primary,
  primaryLight: warmDusk.primaryLight,
  // reveal — RESERVED: Reveal gate option and mutual reveal ceremony only
  reveal: warmDusk.reveal,
  accent: warmDusk.accent,
  textPrimary: warmDusk.textPrimary,
  textSecondary: warmDusk.textSecondary,
  textMuted: warmDusk.textMuted,
  border: warmDusk.border,
  error: warmDusk.error,
  offline: warmDusk.offline,
  onPrimary: warmDusk.onPrimary,
  onReveal: warmDusk.onReveal,
  onDark: warmDusk.onDark,
}

// MVP: dark aliases light — no bespoke dark palette in this story
// Dark theme is a post-MVP token swap per the UX spec
const blinderDark = { ...blinderLight }

export const config = createTamagui({
  ...defaultConfig,
  tokens,
  fonts: {
    body: latoFont,
    heading: latoFont,
  },
  themes: {
    light: blinderLight,
    dark: blinderDark,
  },
  settings: {
    ...(defaultConfig.settings ?? {}),
    allowedStyleValues: 'somewhat-strict-web',
    defaultFont: 'body',
  },
})

export default config

export type Conf = typeof config

declare module 'tamagui' {
  interface TamaguiCustomConfig extends Conf {}
}
