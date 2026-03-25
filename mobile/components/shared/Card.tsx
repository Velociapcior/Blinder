// TODO: implement in Story 4-5
import React from 'react';
import { StyleProp, View, ViewStyle } from 'react-native';
import { ThemedText } from './ThemedText';

type CardProps = {
  children?: React.ReactNode;
  style?: StyleProp<ViewStyle>;
};

export function Card(_props: CardProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">Card</ThemedText>
    </View>
  );
}
