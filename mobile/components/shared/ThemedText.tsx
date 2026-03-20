import React from "react";
import { StyleProp, Text, TextProps, TextStyle, StyleSheet } from "react-native";
import { colors, typography } from "../../constants/theme";

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

type TextVariant =
  | "displayXl"
  | "displayLg"
  | "titleMd"
  | "titleSm"
  | "bodyLg"
  | "bodyMd"
  | "bodySm"
  | "captionMd"
  | "captionSm"
  | "labelMd"
  | "labelSm";

type ThemedTextProps = Omit<TextProps, "allowFontScaling"> & {
  /** Maps to typography.size[variant]. Defaults to bodyMd. */
  variant?: TextVariant;
  /** Overrides default text color. Use colors.* values from theme. */
  color?: string;
  style?: StyleProp<TextStyle>;
};

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

/**
 * Thin Text wrapper that enforces WCAG 2.1 AA font-scaling requirements (AC 7).
 *
 * allowFontScaling is always true — WCAG 2.1 AA requirement (project-context rule 15).
 * Fixed-height containers that clip text at ×2.0 scale are a WCAG violation.
 *
 * - allowFontScaling={true} is always on — never a prop, never overrideable
 * - Default color: colors.text.primary
 * - Default font: typography.family.primary
 * - variant prop maps to typography.size[variant]
 */
export function ThemedText({
  variant = "bodyMd",
  color = colors.text.primary,
  style,
  children,
  ...rest
}: ThemedTextProps): React.JSX.Element {
  return (
    <Text
      allowFontScaling={true}
      style={StyleSheet.flatten([
        {
          color,
          fontFamily: typography.family.primary,
          fontSize: typography.size[variant],
        },
        style,
      ])}
      {...rest}
    >
      {children}
    </Text>
  );
}