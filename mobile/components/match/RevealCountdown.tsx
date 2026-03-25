// TODO: implement in Story 6-1
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from '../shared/ThemedText';

type RevealCountdownProps = Record<string, never>;

export function RevealCountdown(_props: RevealCountdownProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">RevealCountdown</ThemedText>
    </View>
  );
}
