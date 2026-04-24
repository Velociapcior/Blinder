// @ts-check
import tsParser from '@typescript-eslint/parser'

/**
 * No-hardcoded-values rules:
 * - Hex colour literals are banned in app source; use design tokens from tamagui.config.ts
 * - fontSize / lineHeight / letterSpacing and spacing literals are banned in app source;
 *   use shared design-system constants or Tamagui tokens instead
 * - Allowlist: token definition files only
 *
 * Tamagui's allowedStyleValues provides complementary type-level strictness.
 * ESLint is the primary enforcement layer (AC 10).
 */

const HEX_COLOUR_SELECTOR =
  'Literal[value=/^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{4}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$/]'

const STYLE_NUMERIC_LITERAL_SELECTOR =
  'Property[key.name=/^(fontSize|lineHeight|letterSpacing|padding|paddingTop|paddingRight|paddingBottom|paddingLeft|paddingHorizontal|paddingVertical|margin|marginTop|marginRight|marginBottom|marginLeft|marginHorizontal|marginVertical|gap|rowGap|columnGap)$/] > Literal'

const JSX_NUMERIC_LITERAL_SELECTOR =
  'JSXAttribute[name.name=/^(fontSize|lineHeight|letterSpacing|padding|paddingTop|paddingRight|paddingBottom|paddingLeft|paddingHorizontal|paddingVertical|margin|marginTop|marginRight|marginBottom|marginLeft|marginHorizontal|marginVertical|gap|rowGap|columnGap|m|mt|mr|mb|ml|mx|my|p|pt|pr|pb|pl|px|py)$/] > JSXExpressionContainer > Literal'

export default [
  // Files to ignore globally
  {
    ignores: [
      'node_modules/**',
      '.expo/**',
      'dist/**',
      'build/**',
      '*.generated.*',
      'tamagui.generated.css',
      // Token definition files — intentionally contain hex values
      'tamagui.config.ts',
      'src/design-system/palette.ts',
      'src/design-system/motion.ts',
      'src/design-system/metrics.ts',
      // Web HTML root — has bg colour fallback comment-documented above
      'app/+html.tsx',
      // Tests
      '**/*.test.ts',
      '**/*.test.tsx',
      '**/*.spec.ts',
      '**/*.spec.tsx',
    ],
  },
  // App source files — enforce token usage for colour, type, and spacing
  {
    files: ['app/**/*.{ts,tsx}', 'src/**/*.{ts,tsx}'],
    languageOptions: {
      parser: tsParser,
      ecmaVersion: 2022,
      sourceType: 'module',
    },
    rules: {
      'no-restricted-syntax': [
        'error',
        {
          selector: HEX_COLOUR_SELECTOR,
          message:
            'Hardcoded hex colour detected. Use a design token from tamagui.config.ts instead (e.g. $primary, $bgBase, $error).',
        },
        {
          selector: STYLE_NUMERIC_LITERAL_SELECTOR,
          message:
            'Hardcoded type or spacing literal detected in a style object. Use shared design-system constants or Tamagui tokens instead.',
        },
        {
          selector: JSX_NUMERIC_LITERAL_SELECTOR,
          message:
            'Hardcoded type or spacing literal detected in a component prop. Use shared design-system constants or Tamagui tokens instead.',
        },
      ],
    },
  },
]
