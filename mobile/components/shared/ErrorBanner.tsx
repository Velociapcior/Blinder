import React from 'react';
import { StyleSheet, View } from 'react-native';
import { colors, radii, spacing } from '../../constants/theme';
import { ThemedText } from './ThemedText';

type ErrorBannerProps = {
  error: string | null;
};

export function ErrorBanner({ error }: ErrorBannerProps): React.JSX.Element | null {
  if (error === null) return null;

  return (
    <View
      accessibilityLiveRegion="polite"
      style={styles.container}
    >
      <ThemedText variant="bodySm" color={colors.danger}>
        {error}
      </ThemedText>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: colors.background.surface,
    borderRadius: radii.sm,
    paddingHorizontal: spacing[4],
    paddingVertical: spacing[3],
    marginBottom: spacing[4],
  },
});
