// Re-exports useAccessibility from the context module for import ergonomics.
// Components should import from hooks/ (project convention), not from contexts/ directly.
export { useAccessibility } from "../contexts/AccessibilityContext";
