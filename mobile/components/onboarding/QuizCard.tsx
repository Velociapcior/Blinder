// TODO: implement in Story 3-1
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from '../shared/ThemedText';

type QuizCardProps = {
  question: string;
  options: string[];
  onAnswer: (answer: string) => void;
};

export function QuizCard(_props: QuizCardProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">QuizCard</ThemedText>
    </View>
  );
}
