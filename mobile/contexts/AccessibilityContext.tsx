import React, { createContext, useContext, useEffect, useState } from "react";
import { AccessibilityInfo, useWindowDimensions } from "react-native";

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

type AccessibilityContextValue = {
  reduceMotion: boolean;
  fontScale: number;
  isScreenReaderEnabled: boolean;
};

// ---------------------------------------------------------------------------
// Reduce Motion Usage Pattern (copy this into every animated component)
// ---------------------------------------------------------------------------
//
// import { useAccessibility } from '../hooks/useAccessibility';
//
// const { reduceMotion } = useAccessibility();
//
// For Animated.timing / Animated.spring:
//   const duration = reduceMotion ? 0 : motion.standard;
//
// For the reveal animation (UX-DR13):
//   const revealDuration = reduceMotion ? 0 : motion.reveal; // 700ms → 0ms
//
// When reduceMotion is true: use opacity fade only (no translation/scale).
// When reduceMotion is false: full motion treatment.
// ---------------------------------------------------------------------------

// ---------------------------------------------------------------------------
// Context
// ---------------------------------------------------------------------------

const AccessibilityContext = createContext<AccessibilityContextValue | undefined>(
  undefined
);

// ---------------------------------------------------------------------------
// Provider
// ---------------------------------------------------------------------------

/**
 * Wraps the app root to provide system accessibility state to all components.
 * Default reduceMotion = true (accessibility-first) until confirmed by OS.
 */
export function AccessibilityProvider({
  children,
}: {
  children: React.ReactNode;
}): React.JSX.Element {
  const { fontScale } = useWindowDimensions();

  const [reduceMotion, setReduceMotion] = useState<boolean>(true);
  const [isScreenReaderEnabled, setIsScreenReaderEnabled] =
    useState<boolean>(false);

  useEffect(() => {
    let isMounted = true;

    const syncAccessibilityState = async (): Promise<void> => {
      try {
        const [nextReduceMotion, nextScreenReaderEnabled] = await Promise.all([
          AccessibilityInfo.isReduceMotionEnabled(),
          AccessibilityInfo.isScreenReaderEnabled(),
        ]);

        if (!isMounted) {
          return;
        }

        setReduceMotion(nextReduceMotion);
        setIsScreenReaderEnabled(nextScreenReaderEnabled);
      } catch {
        if (!isMounted) {
          return;
        }

        // Preserve accessibility-first defaults if the native query fails.
        setReduceMotion(true);
        setIsScreenReaderEnabled(false);
      }
    };

    void syncAccessibilityState();

    // Listen for changes
    const reduceMotionSub = AccessibilityInfo.addEventListener(
      "reduceMotionChanged",
      setReduceMotion
    );
    const screenReaderSub = AccessibilityInfo.addEventListener(
      "screenReaderChanged",
      setIsScreenReaderEnabled
    );

    return () => {
      isMounted = false;
      reduceMotionSub.remove();
      screenReaderSub.remove();
    };
  }, []);

  return (
    <AccessibilityContext.Provider
      value={{ reduceMotion, fontScale, isScreenReaderEnabled }}
    >
      {children}
    </AccessibilityContext.Provider>
  );
}

// ---------------------------------------------------------------------------
// Hook
// ---------------------------------------------------------------------------

/**
 * Returns the current accessibility context values.
 * Throws if called outside AccessibilityProvider — prevents silent fallback-to-defaults bugs.
 */
export function useAccessibility(): AccessibilityContextValue {
  const ctx = useContext(AccessibilityContext);
  if (ctx === undefined) {
    throw new Error(
      "useAccessibility must be used within an AccessibilityProvider"
    );
  }
  return ctx;
}

export { AccessibilityContext };
