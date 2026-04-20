// StarterCard — tappable conversation prompt. Dissolves blank-page anxiety.
// Full tap target. Auto-populates input when tapped.

const StarterCard = ({ prompt, hint, onSelect, selected = false, style }) => (
  <button
    onClick={onSelect}
    style={{
      appearance: "none",
      textAlign: "left",
      width: "100%",
      padding: "14px 16px",
      borderRadius: "var(--radius-lg)",
      border: `1px solid ${selected ? "var(--accent)" : "var(--border)"}`,
      background: selected ? "var(--bg-elevated)" : "var(--bg-surface)",
      color: "var(--text-primary)",
      fontFamily: "var(--font-sans)",
      fontSize: "var(--text-body-size)",
      lineHeight: "var(--text-body-lh)",
      display: "grid",
      gap: 6,
      cursor: "pointer",
      transition: "border-color var(--dur-quick) var(--ease-out), background var(--dur-quick) var(--ease-out), transform var(--dur-quick) var(--ease-out)",
      boxShadow: selected ? "var(--shadow-cta)" : "none",
      ...style,
    }}
  >
    <span style={{ fontWeight: "var(--fw-regular)" }}>{prompt}</span>
    {hint && (
      <span
        className="text-caption"
        style={{ color: "var(--text-secondary)", fontSize: 12, letterSpacing: 0 }}
      >
        {hint}
      </span>
    )}
  </button>
);

window.StarterCard = StarterCard;
