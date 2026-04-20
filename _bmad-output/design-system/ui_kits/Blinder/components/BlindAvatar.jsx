// BlindAvatar — warm, dignified placeholder during blind phase.
// NOT a silhouette, NOT a broken-image. A soft radial gradient shape with
// a subtle inner glow, anchored by a neutral glyph.
// Variants: sm (header), md (reply), lg (match entry / reveal wait).

const BlindAvatar = ({ size = "md", seed = 3, style }) => {
  const px = size === "sm" ? 40 : size === "lg" ? 112 : 64;

  // Seeded hue shift keeps each blind avatar visually unique within the palette.
  const hues = [
    ["#E8B994", "#C4825A"],
    ["#D9A6B8", "#8B4E6E"],
    ["#E4C28A", "#C4825A"],
    ["#C8A68C", "#8B4E6E"],
  ];
  const [a, b] = hues[seed % hues.length];

  return (
    <div
      role="img"
      aria-label="Your conversation partner — identity revealed after the decision gate"
      style={{
        width: px,
        height: px,
        borderRadius: "50%",
        background: `radial-gradient(circle at 30% 30%, ${a} 0%, ${b} 62%, #6b3a54 100%)`,
        display: "grid",
        placeItems: "center",
        position: "relative",
        flexShrink: 0,
        ...style,
      }}
    >
      {/* Inner dignity glyph — two concentric soft arcs */}
      <svg
        width={px * 0.5}
        height={px * 0.5}
        viewBox="0 0 40 40"
        fill="none"
        style={{ opacity: 0.55 }}
        aria-hidden="true"
      >
        <circle cx="20" cy="20" r="14" stroke="#FBF5EE" strokeWidth="1.2" opacity="0.55" />
        <path d="M8 24 Q20 14 32 24" stroke="#FBF5EE" strokeWidth="1.4" strokeLinecap="round" />
      </svg>
    </div>
  );
};

window.BlindAvatar = BlindAvatar;
