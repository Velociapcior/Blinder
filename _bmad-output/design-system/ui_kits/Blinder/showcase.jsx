// Showcase mounts — renders palette, spacing, and each component into the page

// ---------- Palette --------------------------------------------------
const PALETTE = [
  { name: "bg-base",       hex: "#FBF5EE", role: "App background" },
  { name: "bg-surface",    hex: "#EDE3D8", role: "Cards, bubbles" },
  { name: "bg-elevated",   hex: "#F5EDE2", role: "Modals, gate" },
  { name: "bg-dark",       hex: "#2C1C1A", role: "Gate backdrop" },
  { name: "primary",       hex: "#8B4E6E", role: "Actions, me-bubble" },
  { name: "primary-light", hex: "#B87A98", role: "Pressed, emphasis" },
  { name: "reveal",        hex: "#D4A85A", role: "Reveal only" },
  { name: "accent",        hex: "#C4825A", role: "Starter, links" },
  { name: "text-primary",  hex: "#2C1C1A", role: "All primary text" },
  { name: "text-secondary",hex: "#7A5A52", role: "Timestamps" },
  { name: "text-muted",    hex: "#A08878", role: "Placeholders" },
  { name: "border",        hex: "#DDD0C4", role: "Dividers" },
  { name: "error",         hex: "#B85050", role: "Inline errors" },
  { name: "offline",       hex: "#9A9090", role: "Offline state" },
];

const Palette = () => (
  <>
    {PALETTE.map((p) => (
      <div className="swatch" key={p.name} style={{ minHeight: 0 }}>
        <div className="chip" style={{ background: p.hex }} />
        <div className="meta">
          <b>--{p.name}</b>
          <span>{p.hex}</span>
          <div style={{ marginTop: 4, color: "var(--text-secondary)" }}>{p.role}</div>
        </div>
      </div>
    ))}
  </>
);

// ---------- Spacing / Radii / Elevation ------------------------------
const SpacingDemo = () => (
  <div className="demo wide">
    <div className="demo-label">8-pt spacing scale</div>
    <div style={{ display: "grid", gap: 8 }}>
      {[
        ["xs", 4], ["sm", 8], ["md", 16], ["lg", 24], ["xl", 32], ["2xl", 48],
      ].map(([name, px]) => (
        <div key={name} style={{ display: "grid", gridTemplateColumns: "80px 60px 1fr", gap: 14, alignItems: "center" }}>
          <span className="text-caption" style={{ color: "var(--text-muted)" }}>--space-{name}</span>
          <span className="text-caption" style={{ color: "var(--text-secondary)", fontVariantNumeric: "tabular-nums" }}>{px}px</span>
          <div style={{ height: 10, width: px * 3, background: "var(--primary)", opacity: 0.82, borderRadius: 4 }} />
        </div>
      ))}
    </div>
  </div>
);

const RadiiDemo = () => (
  <div className="demo">
    <div className="demo-label">Radii</div>
    <div style={{ display: "grid", gridTemplateColumns: "repeat(5, 1fr)", gap: 10, alignItems: "end" }}>
      {[
        ["sm", 8], ["md", 14], ["lg", 18], ["xl", 20], ["full", "9999"],
      ].map(([name, value]) => (
        <div key={name} style={{ display: "grid", gap: 6, textAlign: "center" }}>
          <div style={{
            height: 54, background: "var(--bg-surface)",
            border: "1px solid var(--border)",
            borderRadius: value === "9999" ? 9999 : value,
          }} />
          <span className="text-caption" style={{ color: "var(--text-muted)" }}>{name}</span>
        </div>
      ))}
    </div>
  </div>
);

const ElevationDemo = () => (
  <div className="demo">
    <div className="demo-label">Elevation — used sparingly</div>
    <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 14, padding: "8px 4px" }}>
      {[
        { label: "CTA", shadow: "var(--shadow-cta)", bg: "var(--bg-elevated)" },
        { label: "Modal", shadow: "var(--shadow-modal)", bg: "var(--bg-elevated)" },
        { label: "Reveal", shadow: "var(--shadow-reveal)", bg: "var(--reveal)" },
      ].map((s) => (
        <div key={s.label} style={{ display: "grid", gap: 8, placeItems: "center" }}>
          <div style={{ width: 56, height: 56, borderRadius: 14, background: s.bg, boxShadow: s.shadow }} />
          <span className="text-caption" style={{ color: "var(--text-muted)" }}>{s.label}</span>
        </div>
      ))}
    </div>
  </div>
);

// ---------- Components ----------------------------------------------

const ButtonsDemo = () => (
  <div className="demo">
    <div className="demo-label">Button hierarchy</div>
    <div className="demo-body">
      <Button variant="primary">Enter conversation</Button>
      <Button variant="secondary">Not right now</Button>
      <Button variant="reveal">Reveal</Button>
      <Button variant="destructive">End this conversation</Button>
      <Button variant="link">Take a break</Button>
    </div>
  </div>
);

const AvatarsDemo = () => (
  <div className="demo">
    <div className="demo-label">Blind avatar · seeded hues</div>
    <div className="demo-row" style={{ gap: 16, padding: "12px 0 4px" }}>
      <BlindAvatar size="sm" seed={0} />
      <BlindAvatar size="md" seed={1} />
      <BlindAvatar size="md" seed={2} />
      <BlindAvatar size="lg" seed={3} />
    </div>
    <span className="text-caption" style={{ color: "var(--text-muted)", textTransform: "none", letterSpacing: 0 }}>
      Warm gradient — never a silhouette, never a placeholder bust.
    </span>
  </div>
);

const BubblesDemo = () => (
  <div className="demo">
    <div className="demo-label">Conversation bubbles</div>
    <div className="demo-body">
      <ConversationBubble variant="them" timestamp="20:13">
        Your profile says you learned to cook last winter — what was the first
        thing you ruined?
      </ConversationBubble>
      <ConversationBubble variant="me" timestamp="20:14">
        Risotto. I learned what "al dente" means the hard way.
      </ConversationBubble>
      <ConversationBubble variant="me" state="sending" timestamp="sending…">
        You?
      </ConversationBubble>
      <ConversationBubble variant="me" state="failed" timestamp="20:15">
        Failed message
      </ConversationBubble>
    </div>
  </div>
);

const StarterDemo = () => {
  const [i, set] = React.useState(0);
  const starters = [
    { prompt: "What's a small thing that brought you joy this week?", hint: "Anti-small-talk" },
    { prompt: "What are you learning that's making you awkward right now?" },
    { prompt: "Where were you happiest this year?" },
  ];
  return (
    <div className="demo">
      <div className="demo-label">Starter cards</div>
      <div className="demo-body">
        {starters.map((s, idx) => (
          <StarterCard key={idx} {...s} selected={i === idx} onSelect={() => set(idx)} />
        ))}
      </div>
    </div>
  );
};

const GateDemo = () => (
  <div className="demo wide">
    <div className="demo-label">Gate — equal weight, no default</div>
    <div className="demo-body">
      <GateOptionCard
        variant="reveal"
        title="Reveal"
        body="Show my photo and see theirs. Decision is private — you find out together."
      />
      <GateOptionCard
        variant="continue"
        title="Keep talking"
        body="Stay in conversation for another day. No pressure to decide yet."
      />
      <GateOptionCard
        variant="abandon"
        title="End here"
        body="This conversation has ended. Gentle closure — no attribution."
      />
    </div>
  </div>
);

const RevealDemo = () => (
  <div className="demo">
    <div className="demo-label">Reveal portrait</div>
    <div style={{ display: "grid", placeItems: "center", padding: "24px 0 8px" }}>
      <RevealPortrait initials="AP" size={120} />
    </div>
    <span className="text-caption" style={{ color: "var(--text-muted)", textTransform: "none", letterSpacing: 0 }}>
      Amber glow uses <code>--shadow-reveal</code>. The only ceremony moment.
    </span>
  </div>
);

const WaitDemo = () => (
  <div className="demo">
    <div className="demo-label">Resolution wait</div>
    <div style={{ display: "grid", placeItems: "center", padding: "12px 0" }}>
      <ResolutionWait />
    </div>
    <span className="text-caption" style={{ color: "var(--text-muted)", textTransform: "none", letterSpacing: 0 }}>
      2s loop · not a spinner. "Your answer is with them."
    </span>
  </div>
);

const ChromeDemo = () => (
  <div className="demo">
    <div className="demo-label">TopBar · Profile · Pill</div>
    <div style={{ border: "1px solid var(--border)", borderRadius: 14, overflow: "hidden" }}>
      <TopBar
        back={() => {}}
        avatar={<BlindAvatar size="sm" seed={1} />}
        title="Conversation 1 of 1"
        subtitle="Day 2 · decision available"
        trailing={<ProfileAvatar initials="M" />}
      />
    </div>
    <div className="demo-row">
      <Pill icon={<IconClock size={12} stroke={2} />}>Daily Rhythm</Pill>
      <Pill>Decision available</Pill>
    </div>
  </div>
);

const InputDemo = () => {
  const [v, set] = React.useState("Risotto. I learned what ");
  return (
    <div className="demo">
      <div className="demo-label">Message input</div>
      <div style={{ border: "1px solid var(--border)", borderRadius: 14, overflow: "hidden" }}>
        <MessageInput value={v} onChange={set} onSend={() => set("")} />
      </div>
      <div style={{ border: "1px solid var(--border)", borderRadius: 14, overflow: "hidden" }}>
        <MessageInput value="" onChange={set} onSend={() => {}} />
      </div>
    </div>
  );
};

// ---------- Screens ---------------------------------------------------

const Phone = ({ children, label }) => (
  <div className="demo phone">
    <div style={{ padding: "14px 18px 6px" }}>
      <span className="demo-label">{label}</span>
    </div>
    <div className="phone-frame">{children}</div>
  </div>
);

const ScreenWaiting = () => (
  <Phone label="Waiting state">
    <TopBar title="Blinder" trailing={<ProfileAvatar initials="M" />} />
    <WaitingState nextMatchLabel="Tomorrow, 8:00" />
  </Phone>
);

const ScreenEntry = () => (
  <Phone label="Match entry">
    <TopBar title="New match" trailing={<ProfileAvatar initials="M" />} />
    <MatchEntryCard onEnter={() => {}} />
  </Phone>
);

const ScreenConversation = () => (
  <Phone label="Active conversation">
    <TopBar
      back={() => {}}
      avatar={<BlindAvatar size="sm" seed={1} />}
      title="Day 2 · Decision available"
      subtitle="Private · no names yet"
      trailing={<ProfileAvatar initials="M" />}
    />
    <div style={{ flex: 1, overflow: "auto", padding: "16px 16px 12px", display: "grid", gap: 10 }}>
      <span className="text-caption" style={{ alignSelf: "center", color: "var(--text-muted)", letterSpacing: 0, textTransform: "none" }}>
        Today
      </span>
      <ConversationBubble variant="them" timestamp="20:13">
        What was the first thing you ruined when you started cooking?
      </ConversationBubble>
      <ConversationBubble variant="me" timestamp="20:14">
        Risotto. I learned what "al dente" means the hard way.
      </ConversationBubble>
      <ConversationBubble variant="them" timestamp="20:15">
        Honestly that's a rite of passage.
      </ConversationBubble>
    </div>
    <MessageInput value="" onSend={() => {}} />
  </Phone>
);

const ScreenGate = () => (
  <Phone label="Decision gate">
    <TopBar title="A decision is here" />
    <div style={{ padding: "24px 18px 18px", display: "grid", gap: 14 }}>
      <h1 style={{ margin: 0, fontSize: "var(--text-h2-size)", lineHeight: "var(--text-h2-lh)", fontWeight: "var(--fw-bold)" }}>
        You've had a real conversation.
      </h1>
      <p style={{ margin: 0, color: "var(--text-secondary)", fontSize: "var(--text-body-sm-size)", lineHeight: "var(--text-body-sm-lh)" }}>
        Your choice is private. You'll find out together.
      </p>
      <div style={{ display: "grid", gap: 10 }}>
        <GateOptionCard variant="reveal" title="Reveal" body="Show my photo and see theirs." />
        <GateOptionCard variant="continue" title="Keep talking" body="Stay in conversation for another day." />
        <GateOptionCard variant="abandon" title="End here" body="Gentle closure, no attribution." />
      </div>
    </div>
  </Phone>
);

const ScreenReveal = () => (
  <Phone label="Mutual reveal">
    <TopBar title="" />
    <div style={{ flex: 1, display: "grid", placeItems: "center", padding: "24px", gap: 18, textAlign: "center" }}>
      <RevealPortrait initials="AP" size={128} />
      <h1 className="text-display" style={{ margin: 0, fontSize: 26, fontWeight: "var(--fw-black)" }}>
        Meet Ania
      </h1>
      <p style={{ margin: 0, color: "var(--text-secondary)", maxWidth: 240 }}>
        You both chose to reveal. The conversation continues with names and
        faces from here.
      </p>
      <Button variant="primary" fullWidth>Continue chatting</Button>
    </div>
  </Phone>
);

// ---------- Motion demos --------------------------------------------
const motionStyles = `
@keyframes blinderPressDemo {
  0%, 20%  { transform: scale(1); }
  30%, 50% { transform: scale(0.98); filter: brightness(0.95); }
  70%, 100%{ transform: scale(1); }
}
@keyframes blinderGateDemo {
  0%, 15%  { opacity: 0; transform: translateY(16px) scale(0.98); }
  55%,100% { opacity: 1; transform: translateY(0) scale(1); }
}
@keyframes blinderRevealDemo {
  0%   { transform: scale(0.86); box-shadow: 0 0 0 0 rgba(212,168,90,0); }
  60%  { transform: scale(1); box-shadow: 0 0 0 18px rgba(212,168,90,0.28), 0 0 0 34px rgba(212,168,90,0.12); }
  100% { transform: scale(1); box-shadow: 0 0 0 12px rgba(212,168,90,0.22), 0 0 0 26px rgba(212,168,90,0.12); }
}
.motion-press   { animation: blinderPressDemo 1.8s var(--ease-out) infinite; }
.motion-gate    { animation: blinderGateDemo 2.4s var(--ease-emphasize) infinite; }
.motion-reveal  { animation: blinderRevealDemo 2.4s var(--ease-emphasize) infinite; }
`;

const MotionDemo = () => (
  <>
    <style>{motionStyles}</style>
    <div className="demo">
      <div className="demo-label">Utility — press (150ms)</div>
      <div style={{ display: "grid", placeItems: "center", padding: "14px 0" }}>
        <div className="motion-press">
          <Button variant="primary">Send</Button>
        </div>
      </div>
    </div>
    <div className="demo">
      <div className="demo-label">Emotional — gate (420ms emphasize)</div>
      <div style={{ padding: "12px 0" }}>
        <div className="motion-gate">
          <GateOptionCard variant="reveal" title="Reveal" body="Show my photo and see theirs." />
        </div>
      </div>
    </div>
    <div className="demo">
      <div className="demo-label">Emotional — reveal (1600ms+)</div>
      <div style={{ display: "grid", placeItems: "center", padding: "20px 0 14px" }}>
        <div className="motion-reveal" style={{
          width: 96, height: 96, borderRadius: "50%",
          background: "linear-gradient(140deg, var(--accent), var(--primary))",
        }} />
      </div>
    </div>
  </>
);

// ---------- Mount ---------------------------------------------------
ReactDOM.createRoot(document.getElementById("palette-mount")).render(<Palette />);
ReactDOM.createRoot(document.getElementById("spacing-mount")).render(
  <>
    <SpacingDemo />
    <RadiiDemo />
    <ElevationDemo />
  </>
);
ReactDOM.createRoot(document.getElementById("components-mount")).render(
  <>
    <ButtonsDemo />
    <AvatarsDemo />
    <BubblesDemo />
    <StarterDemo />
    <GateDemo />
    <RevealDemo />
    <WaitDemo />
    <ChromeDemo />
    <InputDemo />
  </>
);
ReactDOM.createRoot(document.getElementById("screens-mount")).render(
  <>
    <ScreenWaiting />
    <ScreenEntry />
    <ScreenConversation />
    <ScreenGate />
    <ScreenReveal />
  </>
);
ReactDOM.createRoot(document.getElementById("motion-mount")).render(<MotionDemo />);
