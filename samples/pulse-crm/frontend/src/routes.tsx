import { createBrowserRouter, Navigate, redirect } from 'react-router-dom'
import { Layout } from './components/Layout'
import { RequireAuth } from './components/RequireAuth'
import { AuthCallbackPage } from './pages/AuthCallbackPage'
import { ContactsPage } from './pages/ContactsPage'
import { DashboardPage } from './pages/DashboardPage'
import { LoginPage } from './pages/LoginPage'
import { OnboardingPage } from './pages/OnboardingPage'
import { PaymentPage } from './pages/PaymentPage'
import { getMe } from './services/crmApi'
import { isLoggedIn } from './utils/authStorage'

async function onboardingGuard() {
  if (!isLoggedIn()) {
    return redirect('/login')
  }
  try {
    const me = await getMe()
    if (me.hasSubscription) {
      return redirect('/dashboard')
    }
  } catch {
    /* allow onboarding */
  }
  return null
}

export const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  { path: '/auth/callback', element: <AuthCallbackPage /> },
  {
    path: '/onboarding',
    loader: onboardingGuard,
    element: (
      <RequireAuth>
        <OnboardingPage />
      </RequireAuth>
    ),
  },
  {
    path: '/payment',
    element: (
      <RequireAuth>
        <PaymentPage />
      </RequireAuth>
    ),
  },
  {
    path: '/',
    element: (
      <RequireAuth>
        <Layout />
      </RequireAuth>
    ),
    children: [
      { index: true, element: <Navigate to="/dashboard" replace /> },
      { path: 'dashboard', element: <DashboardPage /> },
      { path: 'contacts', element: <ContactsPage /> },
    ],
  },
  { path: '*', element: <Navigate to="/" replace /> },
])
