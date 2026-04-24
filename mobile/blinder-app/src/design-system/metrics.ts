export const space = {
  xs: 4,
  sm: 8,
  md: 16,
  lg: 24,
  xl: 32,
  '2xl': 48,
} as const

export const radius = {
  sm: 8,
  md: 14,
  lg: 18,
  xl: 20,
  full: 9999,
} as const

export const tracking = {
  caption: 0.44,
  button: 0.56,
  eyebrow: 1.5,
} as const

export const typeScale = {
  display: {
    fontFamily: 'Lato_900Black',
    fontSize: 32,
    lineHeight: 38.4,
  },
  h1: {
    fontFamily: 'Lato_700Bold',
    fontSize: 24,
    lineHeight: 31.2,
  },
  h2: {
    fontFamily: 'Lato_700Bold',
    fontSize: 20,
    lineHeight: 27,
  },
  h3: {
    fontFamily: 'Lato_700Bold',
    fontSize: 17,
    lineHeight: 23.8,
  },
  body: {
    fontFamily: 'Lato_400Regular',
    fontSize: 15,
    lineHeight: 24.75,
  },
  bodySm: {
    fontFamily: 'Lato_400Regular',
    fontSize: 13,
    lineHeight: 20.8,
  },
  caption: {
    fontFamily: 'Lato_300Light',
    fontSize: 11,
    lineHeight: 16.5,
    letterSpacing: 0.44,
  },
  button: {
    fontFamily: 'Lato_700Bold',
    fontSize: 14,
    lineHeight: 14,
    letterSpacing: 0.56,
  },
} as const