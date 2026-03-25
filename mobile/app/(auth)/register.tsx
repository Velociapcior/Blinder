import React, { useReducer, useRef, useState } from "react";
import {
  View,
  StyleSheet,
  ScrollView,
  KeyboardAvoidingView,
  Platform,
} from "react-native";
import { useRouter } from "expo-router";
import { authService } from "../../services/authService";
import { ThemedText } from "../../components/shared/ThemedText";
import { Button } from "../../components/shared/Button";
import { TextField } from "../../components/shared/TextField";
import { RadioChipGroup } from "../../components/shared/RadioChipGroup";
import { Toggle } from "../../components/shared/Toggle";
import { ErrorBanner } from "../../components/shared/ErrorBanner";
import { colors, spacing } from "../../constants/theme";
import { ERRORS } from "../../constants/errors";
import type { AsyncState } from "../../types";
import type { RegisterRequest } from "../../types/api";

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
  const isMounted = useRef(true);
  const abortControllerRef = useRef<AbortController | null>(null);
  const isSubmitting = useRef(false);
  const [submitted, setSubmitted] = useState(false);
  const [form, dispatch] = useReducer(formReducer, initialForm);
  const [state, setState] = useReducer(
    (_prev: AsyncState<void>, next: AsyncState<void>) => next,
    { data: null, error: null, isLoading: false }
  );

  // Cleanup: cancel pending requests and mark unmounted on component cleanup
  React.useEffect(() => {
    return () => {
      isMounted.current = false;
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, []);

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

    // Cancel any pending request (e.g., user navigated away)
    abortControllerRef.current?.abort();
    abortControllerRef.current = new AbortController();

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

      // Only update state if component is still mounted
      if (!isMounted.current) return;

      setState({ data: undefined, error: null, isLoading: false });
      setSubmitted(true);
    } catch (err: unknown) {
      // Catch AbortError (user navigated away) separately to avoid showing generic error
      if (err instanceof Error && err.name === 'AbortError') {
        isSubmitting.current = false;
        return;
      }

      // Only update state if component is still mounted
      if (!isMounted.current) return;

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
        <ThemedText variant="bodySm" color={colors.text.secondary} style={styles.confirmBody}>
          If this email is not already registered, your account has been
          created. You will be able to log in once login is available.
        </ThemedText>
        <Button
          variant="ghost"
          onPress={() => router.back()}
          accessibilityLabel="Go back"
        >
          Back
        </Button>
      </View>
    );
  }

  return (
    <KeyboardAvoidingView
      style={styles.scroll}
      behavior={Platform.OS === "ios" ? "padding" : "height"}
    >
      <ScrollView
        contentContainerStyle={styles.container}
        keyboardShouldPersistTaps="handled"
      >
        <ThemedText variant="titleMd" style={styles.heading}>
          Create account
        </ThemedText>

        <ErrorBanner error={state.error} />

        <TextField
          label="Email"
          value={form.email}
          onChangeText={(v) => dispatch({ type: "SET_EMAIL", value: v })}
          keyboardType="email-address"
          autoComplete="email"
          placeholder="you@example.com"
          style={styles.fieldSpacing}
        />

        <TextField
          label="Password"
          value={form.password}
          onChangeText={(v) => dispatch({ type: "SET_PASSWORD", value: v })}
          secureTextEntry
          autoComplete="new-password"
          placeholder="At least 6 characters"
          style={styles.fieldSpacing}
        />

        <TextField
          label="Confirm password"
          value={form.confirmPassword}
          onChangeText={(v) =>
            dispatch({ type: "SET_CONFIRM_PASSWORD", value: v })
          }
          secureTextEntry
          autoComplete="new-password"
          placeholder="Repeat password"
          style={styles.fieldSpacing}
        />

        <ThemedText variant="labelMd" color={colors.text.secondary} style={styles.genderLabel}>
          Gender
        </ThemedText>
        <RadioChipGroup
          options={GENDER_OPTIONS}
          value={form.gender}
          onChange={(v) =>
            dispatch({ type: "SET_GENDER", value: v as RegisterRequest["gender"] })
          }
        />

        <Toggle
          label="I confirm I am 18 years of age or older"
          value={form.over18Declaration}
          onValueChange={() => dispatch({ type: "TOGGLE_OVER18" })}
          accessibilityLabel="I confirm I am 18 years of age or older"
          style={styles.toggleSpacing}
        />
      </ScrollView>
      <View style={styles.footer}>
        <Button
          variant="primary"
          onPress={handleRegister}
          accessibilityLabel="Register"
          isLoading={state.isLoading}
          disabled={state.isLoading}
        >
          Register
        </Button>
      </View>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  scroll: {
    flex: 1,
    backgroundColor: colors.background.primary,
  },
  container: {
    paddingHorizontal: spacing[5],
    paddingVertical: spacing[16],
    paddingBottom: spacing[8],
  },
  heading: {
    marginBottom: spacing[6],
  },
  fieldSpacing: {
    // TextField already adds marginBottom via its internal input style
  },
  genderLabel: {
    marginBottom: spacing[2],
  },
  toggleSpacing: {
    marginBottom: spacing[6],
  },
  confirmBody: {
    marginBottom: spacing[4],
  },
  footer: {
    paddingHorizontal: spacing[5],
    paddingTop: spacing[3],
    paddingBottom: spacing[16],
    backgroundColor: colors.background.primary,
  },
});
