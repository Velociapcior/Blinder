/**
 * Blinder Motion Vocabulary
 *
 * Source of truth: _bmad-output/design-system/colors_and_type.css (--dur-* / --ease-*)
 *
 * Two-speed rule:
 *   UTILITY — near-instant (120–180ms). Anything functional: nav transitions, back, close.
 *   EMOTIONAL — slow and emphasised. The gap between the two is part of the experience.
 *
 * Reduced-motion rule (prefers-reduced-motion: reduce):
 *   All emotional animations collapse to a 200ms cross-fade.
 *   The resolution-wait state is a 2s pulse — never a spinner.
 *
 * In-screen animations:     handled by Tamagui style props / animation driver.
 * Navigation-level / choreography: delegate to react-native-reanimated (already installed).
 */

import { AccessibilityInfo, Platform } from 'react-native'

// ─── Duration constants (ms) ─────────────────────────────────────────────────
// Matches --dur-* variables in colors_and_type.css

export const duration = {
  instant: 80,       // micro-feedback, ripple
  quick: 150,        // in-screen feedback: press, send confirmation
  standard: 220,     // utility: nav, back, dismiss
  gate: 420,         // gate appearance — emphasised decel
  resolutionWait: 2000, // resolution-wait pulse — intentionally unhurried
  reveal: 1600,      // reveal ceremony — multi-stage, slow bezier, warm
  revealMax: 2400,   // upper bound of reveal ceremony multi-stage sequence
  reducedMotion: 200, // prefers-reduced-motion fallback for all emotional animations
} as const

// ─── Easing curves ───────────────────────────────────────────────────────────
// React Native Animated accepts `Easing` from 'react-native'; for Reanimated
// use the string form with `withTiming({ easing: Easing.bezier(...) })`.

export const easing = {
  // Utility navigation and functional transitions
  standard: 'cubic-bezier(0.2, 0, 0, 1)',
  // Gate appearance, reveal entrance — strong decel for emotional weight
  emphasize: 'cubic-bezier(0.16, 1, 0.3, 1)',
  // In-screen feedback: press response, message send
  out: 'cubic-bezier(0.33, 1, 0.68, 1)',
} as const

// ─── Named animation presets ─────────────────────────────────────────────────
// Use these when building animated components to keep timings consistent.

export const animation = {
  /** Utility: nav stack push/pop, back gesture, modal dismiss */
  utility: {
    duration: duration.standard,
    easing: easing.standard,
  },
  /** In-screen feedback: button press, send confirmation, tap ripple */
  feedback: {
    duration: duration.quick,
    easing: easing.out,
  },
  /** Gate appearance — decision screen entrance */
  gate: {
    duration: duration.gate,
    easing: easing.emphasize,
  },
  /**
   * Resolution-wait pulse — "your answer is with them"
   * 2s loop, ease-out, intentionally unhurried.
   * Must be preserved as a pulse — never replaced with a spinner.
   */
  resolutionWait: {
    duration: duration.resolutionWait,
    easing: easing.out,
    loop: true,
  },
  /** Reveal ceremony — multi-stage, 1600–2400ms range */
  reveal: {
    durationMin: duration.reveal,
    durationMax: duration.revealMax,
    easing: easing.emphasize,
  },
  /** prefers-reduced-motion fallback — all emotional animations use this */
  reducedMotion: {
    duration: duration.reducedMotion,
    easing: easing.standard,
  },
} as const

// ─── Reduced-motion helper ───────────────────────────────────────────────────
// On native, Platform.isTV / AccessibilityInfo.isReduceMotionEnabled() is the
// correct check, but motion.ts intentionally stays import-free to remain
// build-time parseable. Callers should gate animated components with:
//
//   import { AccessibilityInfo } from 'react-native'
//   const isReduced = await AccessibilityInfo.isReduceMotionEnabled()
//
// This constant is web-only and provided for CSS media query documentation.
export const WEB_REDUCED_MOTION_QUERY = '(prefers-reduced-motion: reduce)'

// Reduced-motion helper for both web and native callers.
export async function prefersReducedMotion(): Promise<boolean> {
  if (Platform.OS === 'web' && typeof window !== 'undefined') {
    const query = window.matchMedia?.(WEB_REDUCED_MOTION_QUERY)
    return query?.matches ?? false
  }

  try {
    return await AccessibilityInfo.isReduceMotionEnabled()
  } catch {
    return false
  }
}

// Convenience: resolve the correct duration for an emotional animation,
// collapsing to reducedMotion.duration when reduced motion is enabled.
export async function emotionalDuration(nominalMs: number): Promise<number> {
  return (await prefersReducedMotion()) ? duration.reducedMotion : nominalMs
}

// ─── Elevation / shadow constants ────────────────────────────────────────────
// Minimal elevation per design: only CTA, modal, and reveal portrait get shadow.
// Values match --shadow-* variables in colors_and_type.css.

export const shadow = {
  cta: {
    shadowColor: '#2C1C1A',
    shadowOffset: { width: 0, height: 8 },
    shadowOpacity: 0.14,
    shadowRadius: 20,
    elevation: 8,
  },
  modal: {
    shadowColor: '#2C1C1A',
    shadowOffset: { width: 0, height: 12 },
    shadowOpacity: 0.22,
    shadowRadius: 32,
    elevation: 12,
  },
} as const
