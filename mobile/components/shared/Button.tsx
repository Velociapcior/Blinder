import React from 'react';
import { StyleProp, StyleSheet, ViewStyle } from 'react-native';
import { colors } from '../../constants/theme';
import { AccessiblePressable } from './AccessiblePressable';
import { LoadingIndicator } from './LoadingIndicator';
import { ThemedText } from './ThemedText';

type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger';

type ButtonProps = {
  variant: ButtonVariant;
  onPress: () => void;
  accessibilityLabel: string;
  children?: React.ReactNode;
  isLoading?: boolean;
  disabled?: boolean;
  style?: StyleProp<ViewStyle>;
};

/**
 * Accessible button with variant support and loading state.
 * - When isLoading=true, renders LoadingIndicator. If LoadingIndicator fails to render, exception propagates to parent.
 * - Parent component is responsible for catching and handling LoadingIndicator or animation errors.
 * - minHeight: 44 per WCAG 2.1 AA touch target minimum.
 */


export function Button({
  variant,
  onPress,
  accessibilityLabel,
  children,
  isLoading = false,
  disabled = false,
  style,
}: ButtonProps): React.JSX.Element {
  const isDisabled = disabled || isLoading;

  const variantStyle = variantStyles[variant];

  return (
    <AccessiblePressable
      accessibilityRole="button"
      accessibilityLabel={accessibilityLabel}
      accessibilityState={{ disabled: isDisabled }}
      onPress={onPress}
      disabled={isDisabled}
      style={StyleSheet.flatten([
        styles.base,
        variantStyle.container,
        isDisabled && styles.disabled,
        style,
      ])}
    >
      {isLoading ? (
        <LoadingIndicator size="sm" />
      ) : (
        <ThemedText variant="labelMd" color={variantStyle.textColor}>
          {children}
        </ThemedText>
      )}
    </AccessiblePressable>
  );
}

const variantStyles: Record<ButtonVariant, { container: object; textColor: string }> = {
  primary: {
    container: {
      backgroundColor: colors.accent.primary,
    },
    textColor: colors.text.primary,
  },
  secondary: {
    container: {
      backgroundColor: colors.background.surface,
      borderWidth: 1.5,
      borderColor: colors.accent.primary,
    },
    textColor: colors.text.primary,
  },
  ghost: {
    container: {
      backgroundColor: 'transparent',
    },
    textColor: colors.text.secondary,
  },
  danger: {
    container: {
      backgroundColor: colors.danger,
    },
    textColor: colors.text.primary,
  },
};

const styles = StyleSheet.create({
  base: {
    borderRadius: 8,
    paddingVertical: 16,
    alignItems: 'center',
    justifyContent: 'center',
    minHeight: 44,
  },
  disabled: {
    opacity: 0.4,
  },
});
