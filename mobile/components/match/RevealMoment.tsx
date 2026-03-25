// TODO: implement in Story 6-4
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from '../shared/ThemedText';

type RevealMomentProps = {
  photoUrl: string;
  onContinue: () => void;
  onDecline: () => void;
};

export function RevealMoment(_props: RevealMomentProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">RevealMoment</ThemedText>
    </View>
  );
}
