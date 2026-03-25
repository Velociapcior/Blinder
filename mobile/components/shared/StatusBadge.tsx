// TODO: implement in Story 5-1
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from './ThemedText';

type StatusBadgeProps = {
  label: string;
};

export function StatusBadge(_props: StatusBadgeProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">StatusBadge</ThemedText>
    </View>
  );
}
