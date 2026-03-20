import { useWindowDimensions } from "react-native";

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

type LayoutBreakpoint = "compact" | "regular" | "expanded";

type ResponsiveLayout = {
  /** compact: width < 375 | regular: 375–427 | expanded: ≥428 */
  breakpoint: LayoutBreakpoint;
  width: number;
  height: number;
};

// ---------------------------------------------------------------------------
// Hook
// ---------------------------------------------------------------------------

/**
 * Exposes responsive breakpoints based on device screen width (UX-DR17).
 *
 * compact:  width < 375   (small phones — iPhone SE)
 * regular:  375 ≤ width < 428   (standard phones and intermediate widths)
 * expanded: width ≥ 428   (large phones — iPhone 14 Plus/Pro Max)
 */
export function useResponsiveLayout(): ResponsiveLayout {
  const { width, height } = useWindowDimensions();

  let breakpoint: LayoutBreakpoint;
  if (width < 375) {
    breakpoint = "compact";
  } else if (width < 428) {
    breakpoint = "regular";
  } else {
    breakpoint = "expanded";
  }

  return { breakpoint, width, height };
}
