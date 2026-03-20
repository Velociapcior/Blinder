import React from "react";
import {
  AccessibilityRole,
  AccessibilityState,
  Insets,
  Pressable,
  PressableProps,
  StyleSheet,
} from "react-native";
import { touchTarget } from "../../constants/theme";

type AccessiblePressableProps = Omit<PressableProps, "hitSlop" | "style"> & {
  accessibilityRole: AccessibilityRole;
  accessibilityLabel: string;
  accessibilityHint?: string;
  accessibilityState?: AccessibilityState;
  hitSlop?: Insets;
  style?: PressableProps["style"];
};

const baseStyle = {
  minHeight: touchTarget.minSize,
  minWidth: touchTarget.minSize,
};

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
  return (
    <Pressable
      accessibilityRole={accessibilityRole}
      accessibilityLabel={accessibilityLabel}
      accessibilityHint={accessibilityHint}
      accessibilityState={accessibilityState}
      hitSlop={hitSlop}
      style={
        typeof style === "function"
          ? (pressState) =>
              StyleSheet.flatten([baseStyle, style(pressState)])
          : StyleSheet.flatten([baseStyle, style])
      }
      {...rest}
    >
      {children}
    </Pressable>
  );
}