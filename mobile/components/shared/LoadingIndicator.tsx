import React, { useEffect, useRef } from 'react';
import { Animated, StyleSheet, View } from 'react-native';
import { colors, motion } from '../../constants/theme';
import { useAccessibility } from '../../hooks/useAccessibility';

type LoadingIndicatorProps = {
  size?: 'sm' | 'md';
  /** Color for dots. Must be valid React Native color string (hex, rgb, named color, or theme token). */
  color?: string;
};

/**
 * Animated loading indicator with three pulsing dots.
 * - Respects reduceMotion accessibility context: when enabled, renders static dots with no animation.
 * - If color prop is invalid RN color, render may fail (parent must validate or catch errors).
 * - Performance: uses useNativeDriver: true for optimized animations.
 */


export function LoadingIndicator({
  size = 'md',
  color = colors.text.primary,
}: LoadingIndicatorProps): React.JSX.Element {
  const { reduceMotion } = useAccessibility();

  const dot1 = useRef(new Animated.Value(0.3)).current;
  const dot2 = useRef(new Animated.Value(0.3)).current;
  const dot3 = useRef(new Animated.Value(0.3)).current;

  useEffect(() => {
    if (reduceMotion) return;

    const pulse = (dot: Animated.Value, delay: number) =>
      Animated.loop(
        Animated.sequence([
          Animated.delay(delay),
          Animated.timing(dot, {
            toValue: 1,
            duration: motion.standard,
            useNativeDriver: true,
          }),
          Animated.timing(dot, {
            toValue: 0.3,
            duration: motion.standard,
            useNativeDriver: true,
          }),
          Animated.delay(600 - delay),
        ])
      );

    const a1 = pulse(dot1, 0);
    const a2 = pulse(dot2, 200);
    const a3 = pulse(dot3, 400);
    a1.start();
    a2.start();
    a3.start();

    return () => {
      a1.stop();
      a2.stop();
      a3.stop();
    };
  }, [reduceMotion, dot1, dot2, dot3]);

  const dotSize = size === 'sm' ? 6 : 8;
  const dotStyle = {
    width: dotSize,
    height: dotSize,
    borderRadius: dotSize / 2,
    backgroundColor: color,
  };

  if (reduceMotion) {
    return (
      <View style={styles.container}>
        <View style={dotStyle} />
        <View style={dotStyle} />
        <View style={dotStyle} />
      </View>
    );
  }

  return (
    <View style={styles.container}>
      {([dot1, dot2, dot3] as Animated.Value[]).map((opacity, i) => (
        <Animated.View key={i} style={[dotStyle, { opacity }]} />
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    gap: 6,
    alignItems: 'center',
    justifyContent: 'center',
  },
});
