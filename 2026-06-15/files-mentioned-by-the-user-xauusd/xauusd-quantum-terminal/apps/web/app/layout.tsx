import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'XAUUSD Quantum 3.0 — Institutional Intelligence Terminal',
  description: 'Real-time multi-factor statistical trading framework for XAU/USD',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" className="dark">
      <body>{children}</body>
    </html>
  );
}
