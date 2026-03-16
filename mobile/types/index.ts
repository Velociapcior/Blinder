// types/index.ts — shared types used across all mobile code

/**
 * Standard async state shape for all custom hooks.
 * - ARCH-7 / project-context rule 9: NEVER use raw try/catch in components
 * - data and error are NEVER both non-null simultaneously
 * - error strings come from constants/errors.ts ONLY — never raw exception messages
 */
export type AsyncState<T> = {
  data: T | null;
  error: string | null;
  isLoading: boolean;
};
