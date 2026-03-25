// TODO: implement in Story 5-1
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from '../shared/ThemedText';

type ChatBubbleProps = {
  message: string;
  isMine: boolean;
  timestamp: string;
};

export function ChatBubble(_props: ChatBubbleProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">ChatBubble</ThemedText>
    </View>
  );
}
