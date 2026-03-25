// TODO: implement in Story 3-1
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from '../shared/ThemedText';

type ProgressStepperProps = {
  current: number;
  total: number;
};

export function ProgressStepper(_props: ProgressStepperProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">ProgressStepper</ThemedText>
    </View>
  );
}
