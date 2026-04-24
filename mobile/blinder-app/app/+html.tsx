import { ScrollViewStyleReset } from 'expo-router/html'

// This file is web-only and configures the root HTML for every web page.
// Contents run in Node.js during static rendering — no DOM/browser API access.
export default function Root({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <head>
        <meta charSet="utf-8" />
        <meta httpEquiv="X-UA-Compatible" content="IE=edge" />
        <meta
          name="viewport"
          content="width=device-width,initial-scale=1,minimum-scale=1,maximum-scale=1.00001,viewport-fit=cover"
        />
        {/* Lato — 4 weights used by the Blinder type scale (web only; native loads via expo-font) */}
        <link rel="preconnect" href="https://fonts.googleapis.com" />
        <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin="anonymous" />
        <link
          href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400;700;900&display=swap"
          rel="stylesheet"
        />
        <ScrollViewStyleReset />
        {/* Lock body background to Warm Dusk base — no dark-mode flash (MVP is light-only) */}
        <style dangerouslySetInnerHTML={{ __html: bodyBackground }} />
      </head>
      <body>{children}</body>
    </html>
  )
}

// MVP: light theme only, dark is post-MVP. Prevent white-flash by setting bg.base directly.
const bodyBackground = `
body {
  background-color: #FBF5EE;
}
`
