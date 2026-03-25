import React from 'react';
import { StyleProp, StyleSheet, Switch, View, ViewStyle } from 'react-native';
import { colors, spacing } from '../../constants/theme';
import { ThemedText } from './ThemedText';

type ToggleProps = {
  label: string;
  value: boolean;
  /** Callback when switch toggled. If callback throws, exception propagates to parent (parent must handle errors). */
  onValueChange: (value: boolean) => void;
  accessibilityLabel: string;
  style?: StyleProp<ViewStyle>;
};

/**
 * Accessible toggle switch with label.
 * - Parent component is responsible for error handling in onValueChange callback.
 * - Label is displayed beside switch and will wrap if longer than available space.
 */


export function Toggle({
  label,
  value,
  onValueChange,
  accessibilityLabel,
  style,
}: ToggleProps): React.JSX.Element {
  return (
    <View style={StyleSheet.flatten([styles.container, style])}>
      <Switch
        value={value}
        onValueChange={onValueChange}
        trackColor={{ false: colors.background.input, true: colors.safety }}
        thumbColor={colors.text.primary}
        accessibilityLabel={accessibilityLabel}
      />
      <ThemedText variant="bodySm" color={colors.text.secondary} style={styles.label}>
        {label}
      </ThemedText>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing[3],
  },
  label: {
    flex: 1,
  },
});
