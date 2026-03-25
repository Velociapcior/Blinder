// TODO: implement in Story 5-2
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from '../shared/ThemedText';

type ConversationRowProps = {
  name: string;
  lastMessage: string;
  unreadCount: number;
  onPress: () => void;
};

export function ConversationRow(_props: ConversationRowProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">ConversationRow</ThemedText>
    </View>
  );
}
