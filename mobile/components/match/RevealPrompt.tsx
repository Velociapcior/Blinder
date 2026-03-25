// TODO: implement in Story 6-1
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from '../shared/ThemedText';

type RevealPromptProps = {
  onRevealPress: () => void;
};

export function RevealPrompt(_props: RevealPromptProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">RevealPrompt</ThemedText>
    </View>
  );
}
