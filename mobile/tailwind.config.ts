import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./app/**/*.{js,jsx,ts,tsx}",
    "./components/**/*.{js,jsx,ts,tsx}",
  ],
  presets: [require("nativewind/preset")],
  theme: {
    extend: {
      colors: {
        // Blinder design tokens (UX-DR1, UX spec dark mode palette)
        // Dark mode only at MVP — no light mode toggle
        background: {
          primary: "#1A1814",   // App background — warm-tinted, not pure black
          surface: "#252219",   // Cards, received chat bubbles, modals
          input: "#2E2922",     // Input fields, secondary surfaces
          reveal: "#0F0D0B",   // Reveal screen ONLY — deliberate exception
        },
        accent: {
          primary: "#C8833A",   // CTAs, reveal trigger affordance, active states (amber)
          secondary: "#8A5A28", // Secondary actions
          reveal: "#D4A843",    // Reveal screen gold — more luminous, paired ONLY with background.reveal
        },
        text: {
          primary: "#F2EDE6",  // Body text, names — warm white (13.4:1 contrast ✅ AAA)
          secondary: "#9E9790", // Timestamps, labels, captions (5.1:1 contrast ✅ AA)
          muted: "#635D57",    // Placeholder text, disabled states
        },
        safety: "#4A9E8A",    // Consent statements, privacy indicators — calm, not urgent (4.6:1 ✅ AA)
        danger: "#D94F4F",    // Report button ONLY — never decorative (4.58:1 ✅ AA)
      },
      fontFamily: {
        // DM Sans — brand typeface. Never substitute with system-ui without explicit approval.
        sans: ["DM Sans", "SF Pro Text", "Roboto"],
      },
    },
  },
  plugins: [],
};

export default config;
