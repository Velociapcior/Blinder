// Button — hierarchy per spec.
// Primary · Secondary · Reveal (amber, ceremony only) · Destructive · Link
// Min touch target 44px · full radius · label tracking +0.04em

const buttonBase = {
  border: "none",
  borderRadius: "var(--radius-full)",
  minHeight: 46,
  padding: "0 18px",
  fontFamily: "var(--font-sans)",
  fontWeight: "var(--text-button-weight)",
  fontSize: "var(--text-button-size)",
  letterSpacing: "var(--tracking-button)",
  cursor: "pointer",
  display: "inline-flex",
  alignItems: "center",
  justifyContent: "center",
  gap: 8,
  transition: "transform var(--dur-quick) var(--ease-out), filter var(--dur-quick) var(--ease-out), background var(--dur-quick) var(--ease-out)",
  WebkitTapHighlightColor: "transparent",
};

const buttonVariants = {
  primary: {
    background: "var(--primary)",
    color: "var(--on-primary)",
    boxShadow: "var(--shadow-cta)",
  },
  secondary: {
    background: "var(--bg-surface)",
    color: "var(--text-primary)",
    border: "1px solid var(--border)",
  },
  reveal: {
    background: "var(--reveal)",
    color: "var(--on-reveal)",
    boxShadow: "var(--shadow-cta)",
  },
  destructive: {
    background: "var(--bg-surface)",
    color: "var(--error)",
    border: "1px solid var(--border)",
  },
  link: {
    background: "transparent",
    color: "var(--accent)",
    textDecoration: "underline",
    textUnderlineOffset: 3,
    minHeight: "auto",
    padding: "6px 0",
    boxShadow: "none",
  },
};

const Button = ({
  variant = "primary",
  children,
  onClick,
  icon,
  fullWidth = false,
  style,
  ...rest
}) => {
  const [pressed, setPressed] = React.useState(false);
  const vStyle = buttonVariants[variant] || buttonVariants.primary;
  return (
    <button
      onClick={onClick}
      onMouseDown={() => setPressed(true)}
      onMouseUp={() => setPressed(false)}
      onMouseLeave={() => setPressed(false)}
      style={{
        ...buttonBase,
        ...vStyle,
        width: fullWidth ? "100%" : undefined,
        transform: pressed ? "scale(0.985)" : "scale(1)",
        filter: pressed ? "brightness(0.97)" : "brightness(1)",
        ...style,
      }}
      {...rest}
    >
      {icon}
      <span>{children}</span>
    </button>
  );
};

window.Button = Button;
