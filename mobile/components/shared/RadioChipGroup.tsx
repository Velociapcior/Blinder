import React from 'react';
import { StyleProp, StyleSheet, View, ViewStyle } from 'react-native';
import { colors, radii, spacing } from '../../constants/theme';
import { AccessiblePressable } from './AccessiblePressable';
import { ThemedText } from './ThemedText';

type RadioOption = {
  label: string;
  value: string | number;
};

type RadioChipGroupProps = {
  options: RadioOption[];
  value: string | number | null;
  onChange: (value: string | number) => void;
  style?: StyleProp<ViewStyle>;
};

/**
 * Accessible horizontal radio button group using chip components.
 * - Requires non-empty options array. If empty, renders empty group (add fallback UI at parent).
 * - All option.value entries must be unique; duplicates cause unexpected selection behavior.
 * - If value prop is not in options array, fallback highlights first option.
 * - onChange callback receives selected option.value; prop must be non-null at selection time.
 */


export function RadioChipGroup({
  options,
  value,
  onChange,
  style,
}: RadioChipGroupProps): React.JSX.Element {
  // Fallback: if value not in options, highlight first option
  const displayValue = options.some((opt) => opt.value === value) ? value : (options[0]?.value ?? null);

  return (
    <View
      // @ts-ignore — accessibilityRole="radiogroup" is valid on View in RN
      accessibilityRole="radiogroup"
      style={StyleSheet.flatten([styles.container, style])}
    >
      {options.map((opt) => {
        if (opt.value === null || opt.value === undefined) {
          console.warn(`RadioChipGroup: option.value is null/undefined. Skipping invalid option: ${JSON.stringify(opt)}`);
          return null;
        }
        const isSelected = displayValue === opt.value;
        return (
          <AccessiblePressable
            key={opt.value}
            accessibilityRole="radio"
            accessibilityLabel={opt.label}
            accessibilityState={{ selected: isSelected }}
            onPress={() => onChange(opt.value)}
            style={StyleSheet.flatten([
              styles.chip,
              isSelected && styles.chipSelected,
            ])}
          >
            <ThemedText variant="labelMd">{opt.label}</ThemedText>
          </AccessiblePressable>
        );
      })}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    gap: spacing[3],
    marginBottom: spacing[4],
  },
  chip: {
    borderRadius: radii.sm,
    paddingHorizontal: spacing[4],
    paddingVertical: spacing[3],
    backgroundColor: colors.background.input,
    borderWidth: 1.5,
    borderColor: colors.text.muted,
    justifyContent: 'center',
  },
  chipSelected: {
    backgroundColor: colors.background.surface,
    borderColor: colors.accent.primary,
  },
});
