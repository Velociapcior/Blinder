// TODO: implement in Story 6-2
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from './ThemedText';

type ModalProps = {
  visible: boolean;
  onDismiss: () => void;
  children?: React.ReactNode;
  title?: string;
};

export function Modal(_props: ModalProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">Modal</ThemedText>
    </View>
  );
}
