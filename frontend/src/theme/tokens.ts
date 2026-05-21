export const layout = {
  sidebarWidth: 280,
  contentMaxWidth: 1200,
  authMaxWidth: 440,
  bootstrapMaxWidth: 560,
} as const

export const radius = {
  sm: 8,
  md: 10,
  lg: 14,
  xl: 18,
} as const

export const paletteTokens = {
  light: {
    primary: { main: '#5563e8', dark: '#4654d4', light: '#7b89f0', contrastText: '#ffffff' },
    secondary: { main: '#0d9488', dark: '#0f766e', light: '#2dd4bf', contrastText: '#ffffff' },
    text: { primary: '#0f172a', secondary: '#5b6478' },
    background: { default: '#f5f6fa', paper: '#ffffff' },
    gradient: 'linear-gradient(160deg, #f5f6fa 0%, #eef1fb 48%, #f9fafb 100%)',
    authPattern:
      'radial-gradient(ellipse 70% 55% at 50% -10%, rgba(85, 99, 232, 0.1) 0%, transparent 55%), radial-gradient(circle at 100% 100%, rgba(13, 148, 136, 0.07) 0%, transparent 42%)',
  },
  dark: {
    primary: { main: '#8b8ff5', dark: '#7276e8', light: '#a5a9f8', contrastText: '#0d0f14' },
    secondary: { main: '#5ec9b8', dark: '#3db5a3', light: '#7dd4c6', contrastText: '#0d0f14' },
    text: { primary: '#eceef2', secondary: '#9ca3af' },
    background: { default: '#0d0f14', paper: '#151820' },
    gradient: 'linear-gradient(165deg, #0d0f14 0%, #12151c 52%, #0f1218 100%)',
    authPattern:
      'radial-gradient(ellipse 80% 50% at 50% -15%, rgba(139, 143, 245, 0.14) 0%, transparent 58%), radial-gradient(circle at 100% 100%, rgba(94, 201, 184, 0.07) 0%, transparent 40%)',
  },
} as const
