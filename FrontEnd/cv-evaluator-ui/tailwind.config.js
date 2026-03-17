/** @type {import('tailwindcss').Config} */
export default {
  content: ["./src/**/*.{html,ts}"],
  theme: {
    extend: {
      fontFamily: {
        heading: ['Sora', 'sans-serif'],
        body: ['DM Sans', 'sans-serif'],
      },
      colors: {
        surface: {
          950: '#09090f',
          900: '#0f0f18',
          800: '#16161f',
          700: '#1c1c28',
          600: '#252533',
          500: '#2e2e3e',
        },
        border: {
          DEFAULT: '#2a2a3a',
          light: '#3a3a4e',
        },
        accent: {
          DEFAULT: '#e8a23e',
          light: '#f0c060',
          dark: '#c4872e',
          50: 'rgba(232, 162, 62, 0.08)',
          100: 'rgba(232, 162, 62, 0.15)',
        },
        muted: '#8b8ba0',
      },
      boxShadow: {
        glow: '0 0 20px rgba(232, 162, 62, 0.15)',
        card: '0 1px 3px rgba(0, 0, 0, 0.3), 0 1px 2px rgba(0, 0, 0, 0.2)',
        'card-hover': '0 8px 25px rgba(0, 0, 0, 0.4), 0 0 0 1px rgba(232, 162, 62, 0.1)',
      },
      animation: {
        'fade-in': 'fadeIn 0.5s ease-out',
        'fade-in-up': 'fadeInUp 0.5s ease-out',
        'slide-in-right': 'slideInRight 0.3s ease-out',
        'pulse-glow': 'pulseGlow 2s ease-in-out infinite',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        fadeInUp: {
          '0%': { opacity: '0', transform: 'translateY(12px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        slideInRight: {
          '0%': { opacity: '0', transform: 'translateX(20px)' },
          '100%': { opacity: '1', transform: 'translateX(0)' },
        },
        pulseGlow: {
          '0%, 100%': { boxShadow: '0 0 5px rgba(232, 162, 62, 0.2)' },
          '50%': { boxShadow: '0 0 20px rgba(232, 162, 62, 0.4)' },
        },
      },
    },
  },
  plugins: [],
};
