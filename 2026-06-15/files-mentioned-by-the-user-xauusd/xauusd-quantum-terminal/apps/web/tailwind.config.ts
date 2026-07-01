import type { Config } from 'tailwindcss';

const config: Config = {
  content: ['./app/**/*.{ts,tsx}', './components/**/*.{ts,tsx}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        terminal: {
          bg: '#0a0a0f',
          panel: '#111118',
          border: '#1e1e2a',
          highlight: '#2a2a3a',
          green: '#00c853',
          red: '#ff1744',
          yellow: '#ffd600',
          blue: '#2979ff',
          purple: '#d500f9',
          cyan: '#00e5ff',
          text: '#e0e0e0',
          muted: '#6b6b80',
        },
      },
      fontFamily: {
        mono: ['JetBrains Mono', 'Fira Code', 'monospace'],
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      fontSize: {
        '2xs': '0.65rem',
      },
      gridTemplateColumns: {
        dashboard: '280px 1fr 320px',
      },
    },
  },
  plugins: [],
};

export default config;
