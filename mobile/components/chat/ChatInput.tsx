// TODO: implement in Story 5-1
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from '../shared/ThemedText';

type ChatInputProps = {
  value: string;
  onChangeText: (text: string) => void;
  onSend: () => void;
  disabled?: boolean;
};

export function ChatInput(_props: ChatInputProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">ChatInput</ThemedText>
    </View>
  );
}
