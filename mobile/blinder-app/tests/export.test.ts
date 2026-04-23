import { expect, test } from '@playwright/test'

test('static export hydrates the neutral shell', async ({ page }) => {
  const errors: string[] = []

  page.on('pageerror', (err) => {
    errors.push(err.message)
  })

  await page.goto('/', { waitUntil: 'networkidle' })

  await expect(page.getByText('Something went wrong')).not.toBeVisible()
  await expect(page.getByText('Missing theme')).not.toBeVisible()

  const heading = page.getByText('Mobile foundation ready')
  await expect(heading).toBeVisible()
  await expect(
    page.getByText('Navigation is configured for a stack-only Expo Router shell.')
  ).toBeVisible()

  expect(errors).toHaveLength(0)

  const color = await heading.evaluate((el) => window.getComputedStyle(el).color)
  expect(color).toBeTruthy()
  expect(color).not.toBe('')
})
