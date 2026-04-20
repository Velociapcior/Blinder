// TopBar — minimal header with optional back, avatar, name/subtitle, trailing action.
// Navigation chrome should recede during active conversation.

const TopBar = ({ back, avatar, title, subtitle, trailing, style }) => (
  <header
    style={{
      display: "flex",
      alignItems: "center",
      gap: 12,
      padding: "14px 16px",
      borderBottom: "1px solid var(--border)",
      background: "rgba(251, 245, 238, 0.88)",
      backdropFilter: "blur(8px)",
      WebkitBackdropFilter: "blur(8px)",
      ...style,
    }}
  >
    {back && (
      <button
        onClick={back}
        aria-label="Back"
        style={{
          width: 40, height: 40,
          borderRadius: "50%",
          background: "transparent",
          border: "none",
          color: "var(--text-secondary)",
          cursor: "pointer",
          display: "grid", placeItems: "center",
          flexShrink: 0,
        }}
      >
        <IconArrowLeft size={22} />
      </button>
    )}
    {avatar}
    <div style={{ flex: 1, minWidth: 0 }}>
      {title && (
        <div
          style={{
            fontFamily: "var(--font-sans)",
            fontSize: "var(--text-h3-size)",
            fontWeight: "var(--fw-bold)",
            color: "var(--text-primary)",
            lineHeight: 1.25,
            overflow: "hidden",
            textOverflow: "ellipsis",
            whiteSpace: "nowrap",
          }}
        >
          {title}
        </div>
      )}
      {subtitle && (
        <div
          className="text-caption"
          style={{
            color: "var(--text-muted)",
            letterSpacing: 0,
            marginTop: 2,
            textTransform: "none",
          }}
        >
          {subtitle}
        </div>
      )}
    </div>
    {trailing}
  </header>
);

// ProfileAvatar — top-right entry to ProfileSheet. Must look interactive.
const ProfileAvatar = ({ initials = "M", onClick, pendingModeration = false, unread = false }) => (
  <button
    onClick={onClick}
    aria-label="Your profile and settings"
    style={{
      width: 40, height: 40,
      borderRadius: "50%",
      background: "linear-gradient(140deg, #E4C28A, var(--accent))",
      color: "var(--on-primary)",
      border: "1.5px solid var(--bg-elevated)",
      boxShadow: "0 2px 6px rgba(44, 28, 26, 0.12)",
      cursor: "pointer",
      display: "grid",
      placeItems: "center",
      fontFamily: "var(--font-sans)",
      fontWeight: "var(--fw-bold)",
      fontSize: 15,
      position: "relative",
      flexShrink: 0,
    }}
  >
    {initials}
    {(unread || pendingModeration) && (
      <span
        style={{
          position: "absolute",
          top: -2, right: -2,
          width: 10, height: 10,
          borderRadius: "50%",
          background: pendingModeration ? "var(--text-muted)" : "var(--primary)",
          border: "2px solid var(--bg-base)",
        }}
      />
    )}
  </button>
);

// Pill — small status tag. Anchors waiting state, gate trigger, etc.
const Pill = ({ children, icon, style }) => (
  <span
    style={{
      display: "inline-flex",
      alignItems: "center",
      gap: 6,
      padding: "6px 12px",
      background: "var(--bg-surface)",
      border: "1px solid var(--border)",
      borderRadius: "var(--radius-full)",
      fontFamily: "var(--font-sans)",
      fontSize: 12,
      fontWeight: "var(--fw-bold)",
      letterSpacing: "var(--tracking-button)",
      color: "var(--text-secondary)",
      textTransform: "uppercase",
      ...style,
    }}
  >
    {icon}
    {children}
  </span>
);

// MessageInput — single-line composer, expands up to 4 lines, send via opacity change.
const MessageInput = ({ value = "", onChange, onSend, placeholder = "Message…" }) => {
  const hasText = value.trim().length > 0;
  return (
    <div
      style={{
        display: "flex",
        alignItems: "flex-end",
        gap: 10,
        padding: "12px 14px",
        borderTop: "1px solid var(--border)",
        background: "var(--bg-base)",
      }}
    >
      <div
        style={{
          flex: 1,
          borderRadius: "var(--radius-full)",
          background: "var(--bg-surface)",
          border: "1px solid var(--border)",
          padding: "10px 16px",
          fontFamily: "var(--font-sans)",
          fontSize: "var(--text-body-size)",
          color: value ? "var(--text-primary)" : "var(--text-muted)",
          minHeight: 22,
        }}
      >
        {value || placeholder}
      </div>
      <button
        onClick={onSend}
        aria-label="Send message"
        disabled={!hasText}
        style={{
          width: 40, height: 40,
          borderRadius: "50%",
          background: "var(--primary)",
          color: "var(--on-primary)",
          border: "none",
          display: "grid", placeItems: "center",
          cursor: hasText ? "pointer" : "default",
          opacity: hasText ? 1 : 0.45,
          transition: "opacity var(--dur-quick) var(--ease-out)",
          flexShrink: 0,
        }}
      >
        <IconSend size={18} stroke={2} />
      </button>
    </div>
  );
};

Object.assign(window, { TopBar, ProfileAvatar, Pill, MessageInput });
