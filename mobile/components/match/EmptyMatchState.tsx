// TODO: implement in Story 4-5
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from '../shared/ThemedText';

type EmptyMatchStateProps = Record<string, never>;

export function EmptyMatchState(_props: EmptyMatchStateProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">EmptyMatchState</ThemedText>
    </View>
  );
}
