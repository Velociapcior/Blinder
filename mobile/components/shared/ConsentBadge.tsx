// TODO: implement in Story 5-1
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from './ThemedText';

type ConsentBadgeProps = Record<string, never>;

export function ConsentBadge(_props: ConsentBadgeProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">ConsentBadge</ThemedText>
    </View>
  );
}
