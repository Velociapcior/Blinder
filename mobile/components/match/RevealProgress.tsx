// TODO: implement in Story 6-1
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from '../shared/ThemedText';

type RevealProgressProps = {
  current: number;
  threshold: number;
};

export function RevealProgress(_props: RevealProgressProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">RevealProgress</ThemedText>
    </View>
  );
}
