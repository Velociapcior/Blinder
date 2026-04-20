// Composed screen-level components: WaitingState · MatchEntryCard · OutcomeScreen · OfflineBlocker

// WaitingState — home screen with temporal anchor. Calm, not empty.
const WaitingState = ({ nextMatchLabel = "Tomorrow, 8:00", onOpenProfile, style }) => (
  <div
    style={{
      display: "grid",
      gap: 20,
      padding: "40px 24px",
      textAlign: "center",
      justifyItems: "center",
      alignContent: "center",
      flex: 1,
      ...style,
    }}
  >
    {/* Anticipation halo — subtle, slow */}
    <div
      style={{
        width: 132, height: 132,
        borderRadius: "50%",
        background:
          "radial-gradient(circle at 50% 50%, rgba(212,168,90,0.22) 0%, rgba(196,130,90,0.08) 45%, transparent 75%)",
        display: "grid",
        placeItems: "center",
      }}
    >
      <div
        style={{
          width: 62, height: 62,
          borderRadius: "50%",
          background: "linear-gradient(140deg, var(--bg-elevated), var(--bg-surface))",
          border: "1px solid var(--border)",
        }}
      />
    </div>

    <Pill icon={<IconClock size={12} stroke={2} />}>Daily Rhythm</Pill>

    <h1
      style={{
        margin: 0,
        fontSize: "var(--text-h1-size)",
        lineHeight: "var(--text-h1-lh)",
        fontWeight: "var(--fw-bold)",
        color: "var(--text-primary)",
        maxWidth: 280,
        textWrap: "pretty",
      }}
    >
      Your match arrives daily
    </h1>
    <p
      style={{
        margin: 0,
        fontSize: "var(--text-body-size)",
        lineHeight: "var(--text-body-lh)",
        color: "var(--text-secondary)",
        maxWidth: 280,
      }}
    >
      One focused conversation at a time. Next match — <strong style={{ color: "var(--text-primary)" }}>{nextMatchLabel}</strong>.
    </p>
  </div>
);

// MatchEntryCard — first impression of an actual match.
const MatchEntryCard = ({ onEnter, style }) => (
  <div
    style={{
      display: "grid",
      gap: 24,
      padding: "40px 24px 24px",
      textAlign: "center",
      justifyItems: "center",
      alignContent: "center",
      flex: 1,
      ...style,
    }}
  >
    <BlindAvatar size="lg" seed={1} />
    <div style={{ display: "grid", gap: 10, maxWidth: 280 }}>
      <h1
        style={{
          margin: 0,
          fontSize: "var(--text-h1-size)",
          lineHeight: "var(--text-h1-lh)",
          fontWeight: "var(--fw-bold)",
          textWrap: "pretty",
        }}
      >
        Someone is waiting to talk with you
      </h1>
      <p
        style={{
          margin: 0,
          fontSize: "var(--text-body-size)",
          lineHeight: "var(--text-body-lh)",
          color: "var(--text-secondary)",
        }}
      >
        No swiping. Begin with a real conversation, and see where it goes together.
      </p>
    </div>
    <Button variant="primary" fullWidth onClick={onEnter}>
      Enter conversation
    </Button>
  </div>
);

// OutcomeScreen — neutral closure for non-mutual endings, timeouts, abandons.
// Variant reveal hands off to RevealTransition elsewhere.
const OutcomeScreen = ({
  variant = "acceptance",           // acceptance | expiry
  onNext,
  onBreak,
  style,
}) => (
  <div
    style={{
      display: "grid",
      gap: 20,
      padding: "48px 24px 24px",
      textAlign: "center",
      justifyItems: "center",
      alignContent: "center",
      flex: 1,
      ...style,
    }}
  >
    <div
      style={{
        width: 64, height: 64,
        borderRadius: "50%",
        background: "var(--bg-surface)",
        border: "1px solid var(--border)",
        display: "grid",
        placeItems: "center",
        color: "var(--text-secondary)",
      }}
    >
      <IconSpark size={26} stroke={1.6} />
    </div>
    <div style={{ display: "grid", gap: 10, maxWidth: 290 }}>
      <h1
        style={{
          margin: 0,
          fontSize: "var(--text-h1-size)",
          lineHeight: "var(--text-h1-lh)",
          fontWeight: "var(--fw-bold)",
          textWrap: "pretty",
        }}
      >
        This conversation has ended
      </h1>
      <p
        style={{
          margin: 0,
          fontSize: "var(--text-body-size)",
          lineHeight: "var(--text-body-lh)",
          color: "var(--text-secondary)",
        }}
      >
        Thanks for showing up with intention. Your next match is being prepared.
      </p>
    </div>
    <div style={{ display: "grid", gap: 10, width: "100%", maxWidth: 320 }}>
      <Button variant="primary" fullWidth onClick={onNext}>
        Find my next match
      </Button>
      <Button variant="link" onClick={onBreak}>Take a break</Button>
    </div>
  </div>
);

// OfflineBlocker — calm, not alarming. Appears within 2s. Auto-recovers.
const OfflineBlocker = ({ visible = true, style }) => (
  <div
    role="status"
    aria-live="polite"
    style={{
      position: "absolute",
      inset: 0,
      display: visible ? "grid" : "none",
      gridTemplateRows: "1fr auto 1fr",
      background: "rgba(44, 28, 26, 0.92)",
      color: "var(--on-dark)",
      padding: 28,
      textAlign: "center",
      zIndex: 20,
      backdropFilter: "blur(6px)",
      WebkitBackdropFilter: "blur(6px)",
      ...style,
    }}
  >
    <div />
    <div style={{ display: "grid", gap: 14, justifyItems: "center" }}>
      <div
        style={{
          width: 56, height: 56,
          borderRadius: "50%",
          border: "1px solid rgba(246, 238, 229, 0.3)",
          display: "grid", placeItems: "center",
          color: "var(--on-dark)",
        }}
      >
        <IconCloud size={24} stroke={1.6} color="#F6EEE5" />
      </div>
      <h2 style={{ margin: 0, fontSize: 22, fontWeight: "var(--fw-bold)" }}>
        You are offline
      </h2>
      <p
        style={{
          margin: 0,
          lineHeight: 1.6,
          color: "#e7d7c5",
          maxWidth: 270,
          fontSize: 14,
        }}
      >
        Write actions are paused. We'll restore everything automatically when your connection returns.
      </p>
    </div>
    <div />
  </div>
);

Object.assign(window, { WaitingState, MatchEntryCard, OutcomeScreen, OfflineBlocker });
