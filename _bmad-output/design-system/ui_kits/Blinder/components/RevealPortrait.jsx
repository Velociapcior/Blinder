// RevealPortrait — full portrait circle for the reveal ceremony.
// Amber ring glow uses --shadow-reveal. This is the ONLY place outside the
// Reveal button where --reveal appears.

const RevealPortrait = ({ initials = "AP", size = 120, style }) => (
  <div
    style={{
      width: size,
      height: size,
      borderRadius: "50%",
      background: "linear-gradient(140deg, var(--accent) 0%, var(--primary) 100%)",
      boxShadow: "var(--shadow-reveal)",
      display: "grid",
      placeItems: "center",
      color: "var(--on-primary)",
      fontFamily: "var(--font-sans)",
      fontWeight: "var(--fw-black)",
      fontSize: Math.round(size * 0.28),
      letterSpacing: 0.5,
      ...style,
    }}
  >
    {initials}
  </div>
);

window.RevealPortrait = RevealPortrait;
