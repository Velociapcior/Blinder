// ConversationBubble — message display unit. Receding chrome, conversation-first.
// Variants: them (left, surface) · me (right, primary)
// States: sent · sending (optimistic, slightly faded) · failed (error underline)

const bubbleBase = {
  maxWidth: "78%",
  padding: "10px 14px",
  fontFamily: "var(--font-sans)",
  fontSize: "var(--text-body-size)",
  lineHeight: "var(--text-body-lh)",
  borderRadius: "var(--radius-lg)",
  wordBreak: "break-word",
};

const ConversationBubble = ({
  children,
  variant = "them",           // them | me
  state = "sent",             // sent | sending | failed
  timestamp,
  style,
}) => {
  const isMe = variant === "me";
  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: isMe ? "flex-end" : "flex-start",
        gap: 2,
        ...style,
      }}
    >
      <div
        style={{
          ...bubbleBase,
          background: isMe ? "var(--primary)" : "var(--bg-surface)",
          color: isMe ? "var(--on-primary)" : "var(--text-primary)",
          borderBottomLeftRadius: isMe ? "var(--radius-lg)" : 6,
          borderBottomRightRadius: isMe ? 6 : "var(--radius-lg)",
          opacity: state === "sending" ? 0.62 : 1,
          textDecoration: state === "failed" ? "underline wavy var(--error)" : "none",
          transition: "opacity var(--dur-standard) var(--ease-out)",
        }}
      >
        {children}
      </div>
      {timestamp && (
        <span
          className="text-caption"
          style={{
            color: state === "failed" ? "var(--error)" : "var(--text-muted)",
            padding: "0 6px",
          }}
        >
          {state === "failed" ? "Didn't send — tap to retry" : timestamp}
        </span>
      )}
    </div>
  );
};

window.ConversationBubble = ConversationBubble;
