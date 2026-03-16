/**
 * Blinder design tokens — single source of truth for all visual decisions.
 * Dark mode only at MVP (UX-DR1). No light mode toggle.
 * Source: UX Design Specification + tech-preferences section 12.
 *
 * Amber bookend (UX-DR2): accent.primary (#C8833A) appears at TWO moments only:
 *   1. Onboarding entry (within first 3 seconds of app open)
 *   2. Reveal trigger screen
 * This bookend creates a subconscious visual through-line.
 */
export const colors = {
  background: {
    primary: "#1A1814",  // App background
    surface: "#252219",  // Cards, received bubbles, modals
    input: "#2E2922",    // Input fields
    reveal: "#0F0D0B",   // Reveal screen ONLY — deliberate exception (darker for reveal intimacy)
  },
  accent: {
    primary: "#C8833A",  // CTAs, active states, reveal trigger (amber)
    secondary: "#8A5A28",
    reveal: "#D4A843",   // Reveal screen gold — paired ONLY with background.reveal
  },
  text: {
    primary: "#F2EDE6",  // 13.4:1 on background.primary ✅ AAA
    secondary: "#9E9790", // 5.1:1 on background.primary ✅ AA
    muted: "#635D57",
  },
  safety: "#4A9E8A",    // 4.6:1 on background.primary ✅ AA — consent, privacy
  danger: "#D94F4F",    // 4.58:1 on background.primary ✅ AA — report only, never decorative
} as const;

/**
 * Typography scale — DM Sans typeface.
 * NEVER substitute with system-ui without explicit product approval.
 * DM Sans is brand delivery, not decoration (~85KB bundle, acceptable at MVP).
 */
export const typography = {
  family: {
    primary: "DM Sans",
    fallback: ["SF Pro Text", "Roboto", "system-ui", "sans-serif"],
  },
  size: {
    displayXl: 32,  // Reveal screen headline, onboarding hero
    displayLg: 26,  // Section headlines, match arrival
    titleMd: 20,    // Screen titles, name display
    titleSm: 17,    // Card titles, conversation name
    bodyLg: 16,     // Chat messages — primary reading size
    bodyMd: 15,     // Descriptions, quiz answers
    bodySm: 14,     // Supporting text
    captionMd: 12,  // Timestamps, metadata
    captionSm: 11,  // Never below 11px at default scale
    labelMd: 14,    // Button text, navigation labels
    labelSm: 12,    // Tags, small buttons
  },
  lineHeight: {
    tight: 1.2,
    snug: 1.35,
    normal: 1.5,
    relaxed: 1.6,   // Chat messages — generous for emotional readability
  },
} as const;

/**
 * Spacing — 4px base unit.
 * All spacing derived from this unit for consistency.
 */
export const spacing = {
  1: 4,
  2: 8,
  3: 12,
  4: 16,   // Standard component padding
  5: 20,
  6: 24,
  8: 32,
  10: 40,  // Generous breathing room — reveal screen, onboarding
  12: 48,
  16: 64,  // Screen-level top/bottom padding
} as const;

/**
 * Border radii.
 */
export const radii = {
  sm: 8,
  md: 12,
  lg: 16,   // Cards, modals
  full: 9999,
  bubbleSent: {
    topLeft: 18,
    topRight: 18,
    bottomRight: 4,  // WhatsApp-conventional
    bottomLeft: 18,
  },
  bubbleReceived: {
    topLeft: 18,
    topRight: 18,
    bottomRight: 18,
    bottomLeft: 4,
  },
} as const;

/**
 * Motion durations (ms).
 * All animations MUST respect AccessibilityInfo.isReduceMotionEnabled() (UX-DR13, project-context rule 15).
 * When reduce motion is enabled: fade-only variant, duration → 0ms or opacity fade.
 */
export const motion = {
  fast: 150,      // Micro-interactions (button press, input focus)
  standard: 300,  // Screen transitions, modal entry
  reveal: 700,    // The mutual reveal moment — ceremonial, 600–800ms range
} as const;

/**
 * Touch target minimums — WCAG 2.1 AA compliance (project-context rule 15).
 */
export const touchTarget = {
  minSize: 44,         // All interactive elements: minHeight: 44, minWidth: 44
  hitSlop: {           // hitSlop on icon-only controls (back button, report icon)
    top: 8,
    bottom: 8,
    left: 8,
    right: 8,
  },
} as const;
