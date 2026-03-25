// TODO: implement in Story 4-5
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from './ThemedText';

type EmptyStateProps = {
  title: string;
  body: string;
  action?: React.ReactNode;
};

export function EmptyState(_props: EmptyStateProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">EmptyState</ThemedText>
    </View>
  );
}
