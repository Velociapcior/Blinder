import { defineConfig } from '@playwright/test'

const port = 3838

export default defineConfig({
  testDir: 'tests',
  reporter: [['list']],

  use: {
    baseURL: `http://localhost:${port}`,
  },

  webServer: {
    command: `yarn build:web && yarn dlx serve dist -l ${port}`,
    url: `http://localhost:${port}`,
    reuseExistingServer: true,
    timeout: 180_000,
  },

  fullyParallel: false,
  workers: 1,
  retries: process.env.CI ? 1 : 0,
  maxFailures: 1,
  timeout: 30_000,
})
