// GateOptionCard — full-width equal-weight card for Reveal · Continue · Abandon.
// DESIGN RULE: no default highlighted. All three share the same size, weight, radius.
// Only the Reveal variant uses --reveal (amber); the others use --bg-surface.

const gateVariants = {
  reveal: {
    background: "#F4E3BF",
    border: "1px solid #E5C97D",
    titleColor: "var(--text-primary)",
    bodyColor: "var(--text-secondary)",
  },
  continue: {
    background: "var(--bg-surface)",
    border: "1px solid var(--border)",
    titleColor: "var(--text-primary)",
    bodyColor: "var(--text-secondary)",
  },
  abandon: {
    background: "var(--bg-surface)",
    border: "1px solid var(--border)",
    titleColor: "var(--text-secondary)",
    bodyColor: "var(--text-muted)",
  },
};

const GateOptionCard = ({ variant, title, body, onSelect, submitted = false, style }) => {
  const v = gateVariants[variant] || gateVariants.continue;
  const [press, setPress] = React.useState(false);
  return (
    <button
      role="radio"
      aria-checked={submitted ? true : false}
      onClick={onSelect}
      onMouseDown={() => setPress(true)}
      onMouseUp={() => setPress(false)}
      onMouseLeave={() => setPress(false)}
      disabled={submitted}
      style={{
        appearance: "none",
        textAlign: "left",
        width: "100%",
        padding: "18px 18px",
        borderRadius: "var(--radius-xl)",
        background: v.background,
        border: v.border,
        color: v.titleColor,
        fontFamily: "var(--font-sans)",
        display: "grid",
        gap: 6,
        cursor: submitted ? "default" : "pointer",
        opacity: submitted ? 0.5 : 1,
        transform: press ? "scale(0.992)" : "scale(1)",
        transition: "transform var(--dur-quick) var(--ease-out), opacity var(--dur-standard)",
        ...style,
      }}
    >
      <span
        style={{
          fontSize: "var(--text-h3-size)",
          lineHeight: "var(--text-h3-lh)",
          fontWeight: "var(--fw-bold)",
          color: v.titleColor,
        }}
      >
        {title}
      </span>
      <span
        style={{
          fontSize: "var(--text-body-sm-size)",
          lineHeight: "var(--text-body-sm-lh)",
          color: v.bodyColor,
        }}
      >
        {body}
      </span>
    </button>
  );
};

window.GateOptionCard = GateOptionCard;
