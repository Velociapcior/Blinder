// TODO: implement in Story 8-2
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from '../shared/ThemedText';

type ReportButtonProps = {
  conversationId: string;
  onReported: () => void;
};

export function ReportButton(_props: ReportButtonProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">ReportButton</ThemedText>
    </View>
  );
}
