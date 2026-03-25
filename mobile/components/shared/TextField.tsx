import React from 'react';
import {
  StyleProp,
  StyleSheet,
  TextInput,
  TextInputProps,
  View,
  ViewStyle,
  KeyboardTypeOptions,
} from 'react-native';
import { colors, radii, spacing, typography } from '../../constants/theme';
import { ThemedText } from './ThemedText';

type TextFieldProps = {
  label: string;
  value: string;
  onChangeText: (text: string) => void;
  /** Error state. Falsy values (null, undefined, empty string) render no error border. Truthy values show error styling. */
  error?: string | null;
  secureTextEntry?: boolean;
  keyboardType?: KeyboardTypeOptions;
  autoComplete?: TextInputProps['autoComplete'];
  placeholder?: string;
  style?: StyleProp<ViewStyle>;
};

/**
 * Accessible text input with label, optional error state, and WCAG 2.1 AA compliance.
 * - Label is required and displayed above input. Long labels (80+ chars) will wrap; consider truncating at parent level.
 * - allowFontScaling explicitly enabled for font scaling accessibility.
 * - error prop: falsy = no error; truthy = show red border and danger styling.
 */


export function TextField({
  label,
  value,
  onChangeText,
  error,
  secureTextEntry,
  keyboardType,
  autoComplete,
  placeholder,
  style,
}: TextFieldProps): React.JSX.Element {
  return (
    <View style={style}>
      <ThemedText
        variant="labelMd"
        color={colors.text.secondary}
        style={styles.label}
        numberOfLines={1}
        ellipsizeMode="tail"
      >
        {label}
      </ThemedText>
      <TextInput
        value={value}
        onChangeText={onChangeText}
        secureTextEntry={secureTextEntry}
        keyboardType={keyboardType}
        autoComplete={autoComplete}
        placeholder={placeholder}
        placeholderTextColor={colors.text.muted}
        allowFontScaling={true}
        accessibilityLabel={label}
        style={StyleSheet.flatten([
          styles.input,
          error ? styles.inputError : null,
        ])}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  label: {
    marginBottom: spacing[2],
  },
  input: {
    backgroundColor: colors.background.input,
    color: colors.text.primary,
    borderRadius: radii.sm,
    paddingHorizontal: spacing[4],
    paddingVertical: spacing[3],
    fontSize: typography.size.bodyLg,
    minHeight: 44,
    marginBottom: spacing[4],
  },
  inputError: {
    borderColor: colors.danger,
    borderWidth: 1.5,
  },
});
