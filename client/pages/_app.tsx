import '@/styles/globals.css'
import type { AppProps } from 'next/app'

export default function App({ Component, pageProps }: AppProps) {
  return (
    <main className={"flex flex-col items-center justify-center gap-8 min-h-screen bg-gradient-to-bl from-primary to-secondary"}>
      <Component {...pageProps} />
    </main>
  )
}
