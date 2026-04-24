/**
 * Token Showcase — render helpers for the dev-only /dev/tokens route.
 *
 * Mirrors the packaged Blinder design-system sections so the mobile token
 * implementation can be visually diffed against the HTML kit.
 */

import type { ReactNode } from 'react'

import { ScrollView, Text, View } from 'react-native'

import { duration, easing, shadow } from './motion'
import { radius, space, tracking, typeScale } from './metrics'
import { palette } from './palette'

type SectionProps = {
  title: string
  children: ReactNode
}

type SwatchProps = {
  name: string
  hex: string
}

type TypeRowProps = {
  label: string
  sample: string
  typeStyle: (typeof typeScale)[keyof typeof typeScale]
  weight: string
}

type TokenRowProps = {
  name: string
  value: number
}

type RadiusRowProps = {
  name: string
  value: number
  usage: string
}

type MotionRowProps = {
  name: string
  motionDuration: string
  motionEasing: string
  notes: string
}

const swatchGroups = [
  {
    title: 'Backgrounds',
    swatches: [
      { name: 'bg.base', hex: palette.bgBase },
      { name: 'bg.surface', hex: palette.bgSurface },
      { name: 'bg.elevated', hex: palette.bgElevated },
      { name: 'bg.dark', hex: palette.bgDark },
    ],
  },
  {
    title: 'Brand',
    swatches: [
      { name: 'primary', hex: palette.primary },
      { name: 'primary.light', hex: palette.primaryLight },
      { name: 'accent', hex: palette.accent },
    ],
  },
  {
    title: 'Text',
    swatches: [
      { name: 'text.primary', hex: palette.textPrimary },
      { name: 'text.secondary', hex: palette.textSecondary },
      { name: 'text.muted', hex: palette.textMuted },
    ],
  },
  {
    title: 'UI',
    swatches: [
      { name: 'border', hex: palette.border },
      { name: 'error', hex: palette.error },
      { name: 'offline', hex: palette.offline },
    ],
  },
  {
    title: 'On-colour pairings',
    swatches: [
      { name: 'on-primary', hex: palette.onPrimary },
      { name: 'on-reveal', hex: palette.onReveal },
      { name: 'on-dark', hex: palette.onDark },
    ],
  },
] as const

const typographyRows = [
  {
    label: 'text.display',
    sample: 'Reveal ceremony heading',
    typeStyle: typeScale.display,
    weight: '900',
  },
  {
    label: 'text.h1',
    sample: 'Screen titles, onboarding headings',
    typeStyle: typeScale.h1,
    weight: '700',
  },
  {
    label: 'text.h2',
    sample: 'Section headings, gate title',
    typeStyle: typeScale.h2,
    weight: '700',
  },
  {
    label: 'text.h3',
    sample: 'Subsection labels, chat header name',
    typeStyle: typeScale.h3,
    weight: '700',
  },
  {
    label: 'text.body',
    sample: 'Message bubbles, body copy. Generous line-height for readability in trust-critical flows.',
    typeStyle: typeScale.body,
    weight: '400',
  },
  {
    label: 'text.bodySm',
    sample: 'Starter prompts, secondary descriptions',
    typeStyle: typeScale.bodySm,
    weight: '400',
  },
  {
    label: 'text.caption',
    sample: 'Timestamps, status indicators',
    typeStyle: typeScale.caption,
    weight: '300',
  },
  {
    label: 'text.button',
    sample: 'Button Labels',
    typeStyle: typeScale.button,
    weight: '700',
  },
] as const

const spacingRows = [
  { name: '$xs', value: space.xs },
  { name: '$sm', value: space.sm },
  { name: '$md', value: space.md },
  { name: '$lg', value: space.lg },
  { name: '$xl', value: space.xl },
  { name: '$2xl', value: space['2xl'] },
] as const

const radiiRows = [
  { name: '$radius.sm', value: radius.sm, usage: 'Chips, tags' },
  { name: '$radius.md', value: radius.md, usage: 'Input fields' },
  { name: '$radius.lg', value: radius.lg, usage: 'Conversation bubbles' },
  { name: '$radius.xl', value: radius.xl, usage: 'Cards, gate card, modals' },
  { name: '$radius.full', value: radius.full, usage: 'Avatars, send button, pill buttons' },
] as const

const motionRows = [
  {
    name: 'Utility (nav, back)',
    motionDuration: '120–180ms',
    motionEasing: easing.standard,
    notes: 'Standard system transitions. Handled by navigation-level react-native-reanimated.',
  },
  {
    name: 'In-screen feedback',
    motionDuration: `${duration.quick}ms`,
    motionEasing: easing.out,
    notes: 'Button press response, send confirmation, tap ripple. Handled by Tamagui animation driver.',
  },
  {
    name: 'Gate appearance',
    motionDuration: `${duration.gate}ms`,
    motionEasing: easing.emphasize,
    notes: 'Decision gate entrance — emphasised decel. Uses react-native-reanimated for choreography.',
  },
  {
    name: 'Resolution-wait pulse',
    motionDuration: `${duration.resolutionWait}ms loop`,
    motionEasing: 'ease-out',
    notes: 'Your answer is with them. Intentionally unhurried — must stay a pulse, never a spinner.',
  },
  {
    name: 'Reveal ceremony',
    motionDuration: `${duration.reveal}–${duration.revealMax}ms`,
    motionEasing: easing.emphasize,
    notes: 'Multi-stage warm reveal. Reduced-motion collapses to a 200ms cross-fade.',
  },
] as const

function Section({ title, children }: SectionProps) {
  return (
    <View style={{ marginBottom: space.xl + space.sm }}>
      <Text
        style={{
          fontFamily: 'Lato_700Bold',
          fontSize: typeScale.caption.fontSize,
          lineHeight: typeScale.caption.lineHeight,
          letterSpacing: tracking.eyebrow,
          textTransform: 'uppercase',
          color: palette.textMuted,
          marginBottom: space.md,
        }}
      >
        {title}
      </Text>
      {children}
    </View>
  )
}

function Row({ children }: { children: ReactNode }) {
  return (
    <View style={{ flexDirection: 'row', flexWrap: 'wrap', gap: space.sm, marginBottom: space.sm }}>
      {children}
    </View>
  )
}

function GroupHeading({ title }: { title: string }) {
  return (
    <Text
      style={{
        fontFamily: 'Lato_700Bold',
        fontSize: typeScale.caption.fontSize,
        lineHeight: typeScale.caption.lineHeight,
        color: palette.textSecondary,
        marginBottom: space.sm,
      }}
    >
      {title}
    </Text>
  )
}

function Swatch({ name, hex }: SwatchProps) {
  return (
    <View style={{ alignItems: 'center', width: space['2xl'] + space.lg }}>
      <View
        style={{
          width: space.xl + space.lg,
          height: space.xl + space.lg,
          borderRadius: radius.sm,
          backgroundColor: hex,
          borderWidth: 1,
          borderColor: palette.border,
          marginBottom: space.xs,
        }}
      />
      <Text
        style={{
          fontFamily: 'Lato_400Regular',
          fontSize: typeScale.caption.fontSize - 1,
          lineHeight: typeScale.caption.lineHeight - tracking.eyebrow,
          color: palette.textSecondary,
          textAlign: 'center',
        }}
      >
        {name}
      </Text>
      <Text
        style={{
          fontFamily: 'Lato_300Light',
          fontSize: typeScale.caption.fontSize - 1,
          lineHeight: typeScale.caption.lineHeight - tracking.eyebrow,
          color: palette.textMuted,
          textAlign: 'center',
        }}
      >
        {hex}
      </Text>
    </View>
  )
}

function TypeRow({ label, sample, typeStyle, weight }: TypeRowProps) {
  return (
    <View
      style={{
        marginBottom: space.md + space.xs,
        borderBottomWidth: 1,
        borderBottomColor: palette.bgSurface,
        paddingBottom: space.md,
      }}
    >
      <Text
        style={{
          fontFamily: 'Lato_400Regular',
          fontSize: typeScale.caption.fontSize - 1,
          lineHeight: typeScale.caption.lineHeight - tracking.eyebrow,
          color: palette.textMuted,
          marginBottom: space.xs,
        }}
      >
        {label} · {typeStyle.fontSize}px / {typeStyle.lineHeight.toFixed(2)} lh
        {'letterSpacing' in typeStyle ? ' · +0.04em' : ''} · {weight}
      </Text>
      <Text style={{ ...typeStyle, color: palette.textPrimary }} maxFontSizeMultiplier={1.3}>
        {sample}
      </Text>
    </View>
  )
}

function TokenRow({ name, value }: TokenRowProps) {
  return (
    <View style={{ flexDirection: 'row', alignItems: 'center', marginBottom: space.sm + (space.xs / 2) }}>
      <Text
        style={{
          fontFamily: 'Lato_400Regular',
          fontSize: typeScale.bodySm.fontSize - 1,
          lineHeight: typeScale.bodySm.lineHeight - (space.xs / 5),
          color: palette.textPrimary,
          width: space['2xl'] + space.sm,
        }}
      >
        {name}
      </Text>
      <View
        style={{
          width: value,
          height: space.md,
          backgroundColor: palette.primaryLight,
          borderRadius: space.xs / 2,
          marginRight: space.sm,
        }}
      />
      <Text
        style={{
          fontFamily: 'Lato_300Light',
          fontSize: typeScale.caption.fontSize,
          lineHeight: typeScale.caption.lineHeight,
          color: palette.textMuted,
        }}
      >
        {value}px
      </Text>
    </View>
  )
}

function RadiusRow({ name, value, usage }: RadiusRowProps) {
  return (
    <View style={{ flexDirection: 'row', alignItems: 'center', marginBottom: space.md - space.xs, gap: space.md - space.xs }}>
      <View
        style={{
          width: space.xl + space.md,
          height: space.xl + space.md,
          backgroundColor: palette.primary,
          borderRadius: Math.min(value, space.lg),
          opacity: 0.85,
        }}
      />
      <View>
        <Text
          style={{
            fontFamily: 'Lato_700Bold',
            fontSize: typeScale.bodySm.fontSize - 1,
            lineHeight: typeScale.bodySm.lineHeight - (space.xs / 5),
            color: palette.textPrimary,
          }}
        >
          {name} — {value === radius.full ? 'full' : `${value}px`}
        </Text>
        <Text
          style={{
            fontFamily: 'Lato_400Regular',
            fontSize: typeScale.caption.fontSize,
            lineHeight: typeScale.caption.lineHeight,
            color: palette.textMuted,
          }}
        >
          {usage}
        </Text>
      </View>
    </View>
  )
}

function MotionRow({ name, motionDuration, motionEasing, notes }: MotionRowProps) {
  return (
    <View
      style={{
        marginBottom: space.md,
        paddingBottom: space.md,
        borderBottomWidth: 1,
        borderBottomColor: palette.bgSurface,
      }}
    >
      <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <Text
          style={{
            fontFamily: 'Lato_700Bold',
            fontSize: typeScale.bodySm.fontSize,
            lineHeight: typeScale.bodySm.lineHeight,
            color: palette.textPrimary,
            flex: 1,
          }}
        >
          {name}
        </Text>
        <Text
          style={{
            fontFamily: 'Lato_300Light',
            fontSize: typeScale.caption.fontSize,
            lineHeight: typeScale.caption.lineHeight,
            color: palette.primary,
          }}
        >
          {motionDuration}
        </Text>
      </View>
      <Text
        style={{
          fontFamily: 'Lato_400Regular',
          fontSize: typeScale.caption.fontSize,
          lineHeight: typeScale.caption.lineHeight,
          color: palette.textMuted,
          marginTop: space.xs,
        }}
      >
        {motionEasing}
      </Text>
      <Text
        style={{
          fontFamily: 'Lato_400Regular',
          fontSize: typeScale.bodySm.fontSize - 1,
          lineHeight: typeScale.bodySm.lineHeight - (space.xs / 5),
          color: palette.textSecondary,
          marginTop: space.xs,
        }}
      >
        {notes}
      </Text>
    </View>
  )
}

export function PaletteSection() {
  return (
    <Section title="Palette — Warm Dusk">
      {swatchGroups.map((group) => (
        <View key={group.title} style={{ marginBottom: space.md - space.xs }}>
          <GroupHeading title={group.title} />
          <Row>
            {group.swatches.map((swatch) => (
              <Swatch key={swatch.name} name={swatch.name} hex={swatch.hex} />
            ))}
          </Row>
        </View>
      ))}

      <View
        style={{
          backgroundColor: palette.bgBase,
          borderLeftWidth: space.xs - 1,
          borderLeftColor: palette.reveal,
          padding: space.md - space.xs,
          borderRadius: radius.sm,
          marginBottom: space.md - space.xs,
        }}
      >
        <Text
          style={{
            fontFamily: 'Lato_700Bold',
            fontSize: typeScale.caption.fontSize,
            lineHeight: typeScale.caption.lineHeight,
            color: palette.textSecondary,
          }}
        >
          reveal {palette.reveal} — RESERVED
        </Text>
        <Text
          style={{
            fontFamily: 'Lato_400Regular',
            fontSize: typeScale.caption.fontSize,
            lineHeight: typeScale.caption.lineHeight,
            color: palette.textMuted,
            marginTop: space.xs,
          }}
        >
          Appears only at the Reveal gate option and during the mutual reveal ceremony.
          Its distinctiveness is the product.
        </Text>
        <View
          style={{
            width: space.xl + space.lg,
            height: space.xl + space.lg,
            borderRadius: radius.sm,
            backgroundColor: palette.reveal,
            marginTop: space.sm,
          }}
        />
      </View>

      <View style={{ backgroundColor: palette.bgSurface, padding: space.md - space.xs, borderRadius: radius.sm }}>
        <Text
          style={{
            fontFamily: 'Lato_300Light',
            fontSize: typeScale.caption.fontSize,
            lineHeight: typeScale.caption.lineHeight,
            color: palette.textSecondary,
          }}
        >
          Contrast: text.primary ({palette.textPrimary}) on bg.base ({palette.bgBase}) = 15.06:1 — WCAG AAA ✓
        </Text>
      </View>
    </Section>
  )
}

export function TypographySection() {
  return (
    <Section title="Typography — Lato">
      {typographyRows.map((row) => (
        <TypeRow
          key={row.label}
          label={row.label}
          sample={row.sample}
          typeStyle={row.typeStyle}
          weight={row.weight}
        />
      ))}
    </Section>
  )
}

export function SpacingSection() {
  return (
    <Section title="Spacing — 8pt grid">
      {spacingRows.map((row) => (
        <TokenRow key={row.name} name={row.name} value={row.value} />
      ))}
    </Section>
  )
}

export function RadiiSection() {
  return (
    <Section title="Radii">
      {radiiRows.map((row) => (
        <RadiusRow key={row.name} name={row.name} value={row.value} usage={row.usage} />
      ))}
    </Section>
  )
}

export function ElevationSection() {
  return (
    <Section title="Elevation (minimal)">
      <Text
        style={{
          fontFamily: 'Lato_400Regular',
          fontSize: typeScale.bodySm.fontSize,
          lineHeight: typeScale.bodySm.lineHeight,
          color: palette.textSecondary,
          marginBottom: space.md,
        }}
      >
        Elevation is minimal. Shadows appear only on the single primary CTA per screen,
        elevated modal or sheet surfaces, and the reveal portrait glow.
      </Text>

      <View style={{ gap: space.md }}>
        <View
          style={{
            backgroundColor: palette.bgBase,
            padding: space.md + space.xs,
            borderRadius: radius.md,
            ...shadow.cta,
          }}
        >
          <Text
            style={{
              fontFamily: 'Lato_700Bold',
              fontSize: typeScale.bodySm.fontSize,
              lineHeight: typeScale.bodySm.lineHeight,
              color: palette.textPrimary,
            }}
          >
            shadow-cta
          </Text>
          <Text
            style={{
              fontFamily: 'Lato_400Regular',
              fontSize: typeScale.caption.fontSize,
              lineHeight: typeScale.caption.lineHeight,
              color: palette.textMuted,
              marginTop: space.xs,
            }}
          >
            0 8px 20px rgba(44, 28, 26, 0.14) — primary CTA only
          </Text>
        </View>

        <View
          style={{
            backgroundColor: palette.bgElevated,
            padding: space.md + space.xs,
            borderRadius: radius.xl,
            ...shadow.modal,
          }}
        >
          <Text
            style={{
              fontFamily: 'Lato_700Bold',
              fontSize: typeScale.bodySm.fontSize,
              lineHeight: typeScale.bodySm.lineHeight,
              color: palette.textPrimary,
            }}
          >
            shadow-modal
          </Text>
          <Text
            style={{
              fontFamily: 'Lato_400Regular',
              fontSize: typeScale.caption.fontSize,
              lineHeight: typeScale.caption.lineHeight,
              color: palette.textMuted,
              marginTop: space.xs,
            }}
          >
            0 12px 32px rgba(44, 28, 26, 0.22) — elevated modal or sheet
          </Text>
        </View>
      </View>
    </Section>
  )
}

export function MotionSection() {
  return (
    <Section title="Motion vocabulary">
      <View
        style={{
          backgroundColor: palette.bgSurface,
          padding: space.md - space.xs,
          borderRadius: radius.sm,
          marginBottom: space.md,
        }}
      >
        <Text
          style={{
            fontFamily: 'Lato_700Bold',
            fontSize: typeScale.caption.fontSize,
            lineHeight: typeScale.caption.lineHeight,
            color: palette.textSecondary,
          }}
        >
          Two-speed rule
        </Text>
        <Text
          style={{
            fontFamily: 'Lato_400Regular',
            fontSize: typeScale.caption.fontSize,
            lineHeight: typeScale.caption.lineHeight,
            color: palette.textMuted,
            marginTop: space.xs,
          }}
        >
          Utility is near-instant. Emotional moments are slow and emphasised. Reduced motion collapses them to a 200ms cross-fade.
        </Text>
      </View>

      {motionRows.map((row) => (
        <MotionRow
          key={row.name}
          name={row.name}
          motionDuration={row.motionDuration}
          motionEasing={row.motionEasing}
          notes={row.notes}
        />
      ))}

      <View style={{ backgroundColor: palette.bgElevated, padding: space.md - space.xs, borderRadius: radius.sm }}>
        <Text
          style={{
            fontFamily: 'Lato_700Bold',
            fontSize: typeScale.caption.fontSize,
            lineHeight: typeScale.caption.lineHeight,
            color: palette.textSecondary,
          }}
        >
          Reduced-motion fallback
        </Text>
        <Text
          style={{
            fontFamily: 'Lato_400Regular',
            fontSize: typeScale.caption.fontSize,
            lineHeight: typeScale.caption.lineHeight,
            color: palette.textMuted,
            marginTop: space.xs,
          }}
        >
          200ms cross-fade replaces emotional animations. The resolution-wait pulse remains calm and readable.
        </Text>
      </View>
    </Section>
  )
}

export function TokenUsageSection() {
  return (
    <Section title="Token usage examples">
      <View style={{ marginBottom: space.md - space.xs }}>
        <Text
          style={{
            fontFamily: 'Lato_400Regular',
            fontSize: typeScale.caption.fontSize,
            lineHeight: typeScale.caption.lineHeight,
            color: palette.textMuted,
            marginBottom: space.sm,
          }}
        >
          Primary button
        </Text>
        <View
          style={{
            backgroundColor: palette.primary,
            paddingHorizontal: space.lg,
            paddingVertical: space.md - 2,
            borderRadius: radius.full,
            alignSelf: 'flex-start',
            ...shadow.cta,
          }}
        >
          <Text style={{ ...typeScale.button, color: palette.onPrimary }}>Start conversation</Text>
        </View>
      </View>

      <View style={{ marginBottom: space.md - space.xs }}>
        <Text
          style={{
            fontFamily: 'Lato_400Regular',
            fontSize: typeScale.caption.fontSize,
            lineHeight: typeScale.caption.lineHeight,
            color: palette.textMuted,
            marginBottom: space.sm,
          }}
        >
          Reveal button (reserved)
        </Text>
        <View
          style={{
            backgroundColor: palette.reveal,
            paddingHorizontal: space.lg,
            paddingVertical: space.md - 2,
            borderRadius: radius.full,
            alignSelf: 'flex-start',
          }}
        >
          <Text style={{ ...typeScale.button, color: palette.onReveal }}>Reveal</Text>
        </View>
      </View>

      <View>
        <Text
          style={{
            fontFamily: 'Lato_400Regular',
            fontSize: typeScale.caption.fontSize,
            lineHeight: typeScale.caption.lineHeight,
            color: palette.textMuted,
            marginBottom: space.sm,
          }}
        >
          Conversation bubbles
        </Text>
        <View
          style={{
            backgroundColor: palette.primary,
            padding: space.md - space.xs,
            borderRadius: radius.lg,
            borderBottomRightRadius: space.xs,
            maxWidth: '78%',
            alignSelf: 'flex-end',
            marginBottom: space.sm,
          }}
        >
          <Text style={{ ...typeScale.body, color: palette.onPrimary }}>
            What kind of music do you listen to when it rains?
          </Text>
        </View>
        <View
          style={{
            backgroundColor: palette.bgSurface,
            padding: space.md - space.xs,
            borderRadius: radius.lg,
            borderBottomLeftRadius: space.xs,
            maxWidth: '78%',
            alignSelf: 'flex-start',
          }}
        >
          <Text style={{ ...typeScale.body, color: palette.textPrimary }}>
            Something slow and a little nostalgic. Piano mostly.
          </Text>
        </View>
      </View>
    </Section>
  )
}

export function TokenShowcaseScrollView() {
  return (
    <ScrollView
      style={{ flex: 1, backgroundColor: palette.bgBase }}
      contentContainerStyle={{ padding: space.lg, paddingBottom: space['2xl'] + space.xl }}
      showsVerticalScrollIndicator={false}
    >
      <Text style={{ ...typeScale.display, color: palette.textPrimary, marginBottom: space.sm }} maxFontSizeMultiplier={1.3}>
        Blinder
      </Text>
      <Text style={{ ...typeScale.body, color: palette.textSecondary, marginBottom: space.xl + space.sm }} maxFontSizeMultiplier={1.3}>
        Design token showcase · Warm Dusk · dev build only
      </Text>

      <PaletteSection />
      <TypographySection />
      <SpacingSection />
      <RadiiSection />
      <ElevationSection />
      <MotionSection />
      <TokenUsageSection />
    </ScrollView>
  )
}
