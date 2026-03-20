import React from "react";
import {
  AccessibilityRole,
  AccessibilityState,
  Insets,
  Pressable,
  PressableProps,
} from "react-native";
import { touchTarget } from "../../constants/theme";

// ---------------------------------------------------------------------------
// Props
// ---------------------------------------------------------------------------

type AccessiblePressableProps = Omit<PressableProps, "hitSlop" | "style"> & {
  /** Required — every interactive element must declare its role (UX-DR15, AC 6). */
  accessibilityRole: AccessibilityRole;
  /** Required — every interactive element must have a descriptive label (UX-DR15, AC 6). */
  accessibilityLabel: string;
  /** Optional hint when behaviour is non-obvious to screen reader users. */
  accessibilityHint?: string;
  /** Optional accessibility state (disabled, selected, checked, etc.). */
  accessibilityState?: AccessibilityState;
  /** Override default hitSlop from theme; applies default when omitted (AC 5). */
  hitSlop?: Insets;
  style?: PressableProps["style"];
};

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

/**
 * Thin Pressable wrapper that enforces WCAG 2.1 AA touch-target requirements (AC 5)
 * and VoiceOver/TalkBack accessibility props (AC 6) for icon-only controls.
 *
 * - Enforces minHeight/minWidth = 44px from theme (touchTarget.minSize)
 * - Applies hitSlop from theme by default (overrideable via prop)
 * - accessibilityRole and accessibilityLabel are required at the TypeScript level
 */
export function AccessiblePressable({
  accessibilityRole,
  accessibilityLabel,
  accessibilityHint,
  accessibilityState,
  hitSlop = touchTarget.hitSlop,
  style,
  children,
  ...rest
}: AccessiblePressableProps): React.JSX.Element {
  const baseStyle = {
    minHeight: touchTarget.minSize,
    minWidth: touchTarget.minSize,
    justifyContent: "center" as const,
    alignItems: "center" as const,
  };

  return (
    <Pressable
      accessibilityRole={accessibilityRole}
      accessibilityLabel={accessibilityLabel}
      accessibilityHint={accessibilityHint}
      accessibilityState={accessibilityState}
      hitSlop={hitSlop}
      style={(state) => {
        if (typeof style === "function") {
          return [baseStyle, style(state)];
        }

        return [baseStyle, style];
      }}
      {...rest}
    >
      {children}
    </Pressable>
  );
}
