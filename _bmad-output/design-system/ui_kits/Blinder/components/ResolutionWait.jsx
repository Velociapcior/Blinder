// ResolutionWait — holding state between gate submission and outcome.
// NOT a spinner, NOT a progress bar. Unhurried concentric pulse.
// Duration 2s per ring, two offset rings for an organic cadence.

const resolutionStyles = `
@keyframes blinderResolutionPulse {
  0%   { width: 22px; height: 22px; opacity: 0.85; }
  100% { width: 120px; height: 120px; opacity: 0; }
}
.blinder-resolution-wait {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  position: relative;
  display: grid;
  place-items: center;
}
.blinder-resolution-wait::before,
.blinder-resolution-wait::after {
  content: "";
  position: absolute;
  inset: 0;
  margin: auto;
  border-radius: 50%;
  border: 2px solid rgba(139, 78, 110, 0.38);
  animation: blinderResolutionPulse 2s var(--ease-out) infinite;
}
.blinder-resolution-wait::after {
  animation-delay: 1s;
  border-color: rgba(196, 130, 90, 0.32);
}
.blinder-resolution-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: var(--primary);
  opacity: 0.7;
}
@media (prefers-reduced-motion: reduce) {
  .blinder-resolution-wait::before,
  .blinder-resolution-wait::after { animation: none; opacity: 0.25; width: 90px; height: 90px; }
}
`;

const ResolutionWait = ({ style }) => (
  <>
    <style>{resolutionStyles}</style>
    <div
      className="blinder-resolution-wait"
      role="status"
      aria-label="Waiting for the other person's choice"
      style={style}
    >
      <span className="blinder-resolution-dot" />
    </div>
  </>
);

window.ResolutionWait = ResolutionWait;
