// TODO: implement in Story 5-2
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from './ThemedText';

type SkeletonLoaderProps = {
  width: number | string;
  height: number;
  borderRadius?: number;
};

export function SkeletonLoader(_props: SkeletonLoaderProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">SkeletonLoader</ThemedText>
    </View>
  );
}
