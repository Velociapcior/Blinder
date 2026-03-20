import React, { useEffect, useReducer, useRef, useState } from "react";
import {
  View,
  TextInput,
  Switch,
  StyleSheet,
  ScrollView,
  Animated,
} from "react-native";
import { useRouter } from "expo-router";
import { authService } from "../../services/authService";
import { ThemedText } from "../../components/shared/ThemedText";
import { AccessiblePressable } from "../../components/shared/AccessiblePressable";
import { colors, typography, spacing } from "../../constants/theme";
import { ERRORS } from "../../constants/errors";
import type { AsyncState } from "../../types";
import type { RegisterRequest } from "../../types/api";

function LoadingDots() {
  const dot1 = useRef(new Animated.Value(0.3)).current;
  const dot2 = useRef(new Animated.Value(0.3)).current;
  const dot3 = useRef(new Animated.Value(0.3)).current;

  useEffect(() => {
    const pulse = (dot: Animated.Value, delay: number) =>
      Animated.loop(
        Animated.sequence([
          Animated.delay(delay),
          Animated.timing(dot, { toValue: 1, duration: 300, useNativeDriver: true }),
          Animated.timing(dot, { toValue: 0.3, duration: 300, useNativeDriver: true }),
          Animated.delay(600 - delay),
        ])
      );
    const a1 = pulse(dot1, 0);
    const a2 = pulse(dot2, 200);
    const a3 = pulse(dot3, 400);
    a1.start(); a2.start(); a3.start();
    return () => { a1.stop(); a2.stop(); a3.stop(); };
  }, []);

  return (
    <View style={{ flexDirection: "row", gap: 6, alignItems: "center", justifyContent: "center" }}>
      {[dot1, dot2, dot3].map((opacity, i) => (
        <Animated.View key={i} style={{ width: 6, height: 6, borderRadius: 3, backgroundColor: colors.text.primary, opacity }} />
      ))}
    </View>
  );
}

// Gender values must match the backend UserGender enum (UserGender.cs).
// 0 (Unspecified) is intentionally excluded — the backend rejects it.
type GenderOption = { label: string; value: RegisterRequest["gender"] };
const GENDER_OPTIONS: GenderOption[] = [
  { label: "Male", value: 1 },
  { label: "Female", value: 2 },
  { label: "Non-binary", value: 3 },
];

interface FormState {
  email: string;
  password: string;
  confirmPassword: string;
  gender: RegisterRequest["gender"] | null;
  over18Declaration: boolean;
}

type FormAction =
  | { type: "SET_EMAIL"; value: string }
  | { type: "SET_PASSWORD"; value: string }
  | { type: "SET_CONFIRM_PASSWORD"; value: string }
  | { type: "SET_GENDER"; value: RegisterRequest["gender"] }
  | { type: "TOGGLE_OVER18" };

function formReducer(state: FormState, action: FormAction): FormState {
  switch (action.type) {
    case "SET_EMAIL":
      return { ...state, email: action.value };
    case "SET_PASSWORD":
      return { ...state, password: action.value };
    case "SET_CONFIRM_PASSWORD":
      return { ...state, confirmPassword: action.value };
    case "SET_GENDER":
      return { ...state, gender: action.value };
    case "TOGGLE_OVER18":
      return { ...state, over18Declaration: !state.over18Declaration };
  }
}

const initialForm: FormState = {
  email: "",
  password: "",
  confirmPassword: "",
  gender: null,
  over18Declaration: false,
};

export default function RegisterScreen() {
  const router = useRouter();
  const isSubmitting = useRef(false);
  const [submitted, setSubmitted] = useState(false);
  const [form, dispatch] = useReducer(formReducer, initialForm);
  const [state, setState] = useReducer(
    (_prev: AsyncState<void>, next: AsyncState<void>) => next,
    { data: null, error: null, isLoading: false }
  );

  function validate(): string | null {
    if (!form.email.trim() || !form.email.includes("@"))
      return "Please enter a valid email address.";
    if (form.password.length < 6)
      return "Password must be at least 6 characters.";
    if (form.password.length > 128)
      return "Password must not exceed 128 characters.";
    if (form.password !== form.confirmPassword)
      return "Passwords do not match.";
    if (form.gender === null)
      return "Please select a gender.";
    if (!form.over18Declaration)
      return "You must confirm you are 18 or older.";
    return null;
  }

  async function handleRegister() {
    // Synchronous ref guard prevents double-tap from firing two concurrent requests
    // before the isLoading state update propagates through a re-render.
    if (isSubmitting.current) return;
    const validationError = validate();
    if (validationError) {
      setState({ data: null, error: validationError, isLoading: false });
      return;
    }

    isSubmitting.current = true;
    setState({ data: null, error: null, isLoading: true });

    try {
      // Routes through the backend Identity-backed RegistrationService —
      // same ruleset as the web/admin Razor Page (project rule #17).
      // Both success and duplicate email return 202 Accepted (OWASP anti-enumeration).
      await authService.register({
        email: form.email.trim(),
        password: form.password,
        gender: form.gender!,
        over18Declaration: form.over18Declaration,
      });
      setState({ data: undefined, error: null, isLoading: false });
      setSubmitted(true);
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : ERRORS.UNEXPECTED_ERROR;
      setState({ data: null, error: message, isLoading: false });
      isSubmitting.current = false;
    }
  }

  // OWASP anti-enumeration: both success and duplicate email return 202 Accepted
  // with the same body, so we always show this neutral confirmation.
  if (submitted) {
    return (
      <View
        style={[styles.scroll, styles.container]}
        accessibilityLiveRegion="polite"
      >
        <ThemedText variant="titleMd" style={styles.heading}>
          Registration submitted
        </ThemedText>
        <ThemedText variant="bodySm" style={styles.checkLabel}>
          If this email is not already registered, your account has been
          created. You will be able to log in once login is available.
        </ThemedText>
        <AccessiblePressable
          accessibilityLabel="Go back"
          accessibilityRole="button"
          onPress={() => router.back()}
          style={styles.submitButton}
        >
          <ThemedText variant="labelMd" style={styles.submitText}>
            Back
          </ThemedText>
        </AccessiblePressable>
      </View>
    );
  }

  return (
    <ScrollView
      style={styles.scroll}
      contentContainerStyle={styles.container}
      keyboardShouldPersistTaps="handled"
    >
      <ThemedText variant="titleMd" style={styles.heading}>
        Create account
      </ThemedText>

      {state.error ? (
        <ThemedText variant="bodySm" style={styles.errorBanner}>
          {state.error}
        </ThemedText>
      ) : null}

      <ThemedText variant="labelMd" style={styles.label}>
        Email
      </ThemedText>
      <TextInput
        style={styles.input}
        value={form.email}
        onChangeText={(v) => dispatch({ type: "SET_EMAIL", value: v })}
        keyboardType="email-address"
        autoCapitalize="none"
        autoComplete="email"
        placeholder="you@example.com"
        placeholderTextColor={colors.text.muted}
        accessibilityLabel="Email address"
      />

      <ThemedText variant="labelMd" style={styles.label}>
        Password
      </ThemedText>
      <TextInput
        style={styles.input}
        value={form.password}
        onChangeText={(v) => dispatch({ type: "SET_PASSWORD", value: v })}
        secureTextEntry
        autoComplete="new-password"
        placeholder="At least 6 characters"
        placeholderTextColor={colors.text.muted}
        accessibilityLabel="Password"
      />

      <ThemedText variant="labelMd" style={styles.label}>
        Confirm password
      </ThemedText>
      <TextInput
        style={styles.input}
        value={form.confirmPassword}
        onChangeText={(v) =>
          dispatch({ type: "SET_CONFIRM_PASSWORD", value: v })
        }
        secureTextEntry
        autoComplete="new-password"
        placeholder="Repeat password"
        placeholderTextColor={colors.text.muted}
        accessibilityLabel="Confirm password"
      />

      <ThemedText variant="labelMd" style={styles.label}>
        Gender
      </ThemedText>
      <View style={styles.genderRow} accessibilityRole="radiogroup">
        {GENDER_OPTIONS.map((opt) => (
          <AccessiblePressable
            key={opt.value}
            accessibilityLabel={opt.label}
            accessibilityRole="radio"
            accessibilityState={{ selected: form.gender === opt.value }}
            onPress={() => dispatch({ type: "SET_GENDER", value: opt.value })}
            style={[
              styles.genderChip,
              form.gender === opt.value && styles.genderChipSelected,
            ]}
          >
            <ThemedText
              variant="labelMd"
            >
              {opt.label}
            </ThemedText>
          </AccessiblePressable>
        ))}
      </View>

      <View style={styles.checkRow}>
        <Switch
          value={form.over18Declaration}
          onValueChange={() => dispatch({ type: "TOGGLE_OVER18" })}
          trackColor={{ false: colors.background.input, true: colors.safety }}
          thumbColor={colors.text.primary}
          accessibilityLabel="I confirm I am 18 years of age or older"
        />
        <ThemedText variant="bodySm" style={styles.checkLabel}>
          I confirm I am 18 years of age or older
        </ThemedText>
      </View>

      <AccessiblePressable
        accessibilityLabel="Register"
        accessibilityRole="button"
        onPress={handleRegister}
        disabled={state.isLoading}
        style={[styles.submitButton, state.isLoading && styles.submitDisabled]}
      >
        {state.isLoading ? (
          <LoadingDots />
        ) : (
          <ThemedText variant="labelMd" style={styles.submitText}>
            Register
          </ThemedText>
        )}
      </AccessiblePressable>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  scroll: {
    flex: 1,
    backgroundColor: colors.background.primary,
  },
  container: {
    paddingHorizontal: spacing[5],
    paddingVertical: spacing[4],
    paddingBottom: spacing[8],
  },
  heading: {
    marginBottom: spacing[6],
  },
  label: {
    marginBottom: spacing[2],
    color: colors.text.secondary,
  },
  input: {
    backgroundColor: colors.background.input,
    color: colors.text.primary,
    borderRadius: 8,
    paddingHorizontal: spacing[4],
    paddingVertical: spacing[3],
    fontSize: typography.size.bodyLg,
    marginBottom: spacing[4],
    minHeight: 44, // WCAG 2.1 AA touch target
  },
  genderRow: {
    flexDirection: "row",
    gap: spacing[3],
    marginBottom: spacing[4],
  },
  genderChip: {
    borderRadius: 8,
    paddingHorizontal: spacing[4],
    paddingVertical: spacing[3],
    backgroundColor: colors.background.input,
    minHeight: 44,
    justifyContent: "center",
    borderWidth: 1.5,
    borderColor: colors.text.muted,
  },
  genderChipSelected: {
    borderColor: colors.accent.primary,
    backgroundColor: colors.background.surface,
  },
  submitText: {
    color: colors.text.primary,
  },
  submitDisabled: {
    opacity: 0.4,
  },
  checkRow: {
    flexDirection: "row",
    alignItems: "center",
    gap: spacing[3],
    marginBottom: spacing[6],
  },
  checkLabel: {
    flex: 1,
    color: colors.text.secondary,
    marginBottom: spacing[4],
  },
  submitButton: {
    backgroundColor: colors.accent.primary,
    borderRadius: 8,
    paddingVertical: spacing[4],
    alignItems: "center",
    minHeight: 44,
    justifyContent: "center",
  },
  errorBanner: {
    color: colors.danger,
    backgroundColor: colors.background.surface,
    paddingHorizontal: spacing[4],
    paddingVertical: spacing[3],
    borderRadius: 8,
    marginBottom: spacing[4],
  },
});
