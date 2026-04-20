// Icons — line icons, 1.75–2px stroke, rounded caps & joins, 24px grid.
// The app is intentionally icon-light.

const Icon = ({ children, size = 20, stroke = 1.75, color = "currentColor", style }) => (
  <svg
    width={size}
    height={size}
    viewBox="0 0 24 24"
    fill="none"
    stroke={color}
    strokeWidth={stroke}
    strokeLinecap="round"
    strokeLinejoin="round"
    style={style}
    aria-hidden="true"
  >
    {children}
  </svg>
);

const IconArrowLeft = (p) => (
  <Icon {...p}><path d="M15 6l-6 6 6 6" /></Icon>
);
const IconMore = (p) => (
  <Icon {...p}>
    <circle cx="5"  cy="12" r="1" fill="currentColor" stroke="none" />
    <circle cx="12" cy="12" r="1" fill="currentColor" stroke="none" />
    <circle cx="19" cy="12" r="1" fill="currentColor" stroke="none" />
  </Icon>
);
const IconSend = (p) => (
  <Icon {...p}><path d="M5 12l14 -7 -4 14 -3 -5 -7 -2z" /></Icon>
);
const IconUser = (p) => (
  <Icon {...p}>
    <circle cx="12" cy="9" r="3.2" />
    <path d="M5.5 19c1.3 -3 4 -4.3 6.5 -4.3S17 16 18.5 19" />
  </Icon>
);
const IconCloud = (p) => (
  <Icon {...p}>
    <path d="M7 17a4 4 0 0 1 -0.6 -7.95 5 5 0 0 1 9.75 -0.8 3.5 3.5 0 0 1 0.85 6.9 L7 17z" />
    <path d="M9 20l6 0" />
  </Icon>
);
const IconSpark = (p) => (
  <Icon {...p}>
    <path d="M12 4l1.4 4.6 4.6 1.4 -4.6 1.4 -1.4 4.6 -1.4 -4.6 -4.6 -1.4 4.6 -1.4z" />
  </Icon>
);
const IconClock = (p) => (
  <Icon {...p}>
    <circle cx="12" cy="12" r="8" />
    <path d="M12 8v4l2.5 1.5" />
  </Icon>
);
const IconChevronDown = (p) => (
  <Icon {...p}><path d="M6 9l6 6 6 -6" /></Icon>
);

Object.assign(window, {
  Icon,
  IconArrowLeft, IconMore, IconSend, IconUser,
  IconCloud, IconSpark, IconClock, IconChevronDown,
});
