/**
 * Raw Warm Dusk colour constants — for the rare cases where you cannot use Tamagui
 * theme tokens (e.g. pre-provider loading states, native StyleSheet calls outside
 * the Tamagui tree). Do not use these directly in product screens; consume via
 * Tamagui theme tokens ($bgBase, $primary, etc.) instead.
 *
 * Values are one-to-one with _bmad-output/design-system/colors_and_type.css.
 */
export const palette = {
  bgBase: '#FBF5EE',
  bgSurface: '#EDE3D8',
  bgElevated: '#F5EDE2',
  bgDark: '#2C1C1A',
  primary: '#8B4E6E',
  primaryLight: '#B87A98',
  // RESERVED: Reveal gate option and mutual reveal ceremony only
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
