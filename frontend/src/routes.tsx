import { createBrowserRouter } from 'react-router'
import { AppLayout } from './components/AppLayout'
import { RouteHydrateFallback } from './components/RouteHydrateFallback'
import { requireAuthLoader, loginLoader } from './routes/loaders'
import {
  AcceptInvitePage,
  AuthCallbackPage,
  ApplicationDetailPage,
  ApplicationsPage,
  AuditLogsPage,
  HomePage,
  IdentityProvidersPage,
  JwksPage,
  LoginPage,
  MembershipsPage,
  NotFoundPage,
  ProfilePage,
  SessionsPage,
  TenantRolesPage,
  TenantsPage,
} from './pages'

export const router = createBrowserRouter([
  {
    path: '/login',
    loader: loginLoader,
    HydrateFallback: RouteHydrateFallback,
    Component: LoginPage,
  },
  {
    path: '/auth/callback',
    Component: AuthCallbackPage,
  },
  {
    path: '/',
    loader: requireAuthLoader,
    HydrateFallback: RouteHydrateFallback,
    Component: AppLayout,
    children: [
      { index: true, Component: HomePage },
      { path: 'profile', Component: ProfilePage },
      { path: 'sessions', Component: SessionsPage },
      { path: 'tenants', Component: TenantsPage },
      { path: 'memberships', Component: MembershipsPage },
      { path: 'tenant-roles', Component: TenantRolesPage },
      { path: 'applications', Component: ApplicationsPage },
      { path: 'applications/:applicationId', Component: ApplicationDetailPage },
      { path: 'audit-logs', Component: AuditLogsPage },
      { path: 'identity-providers', Component: IdentityProvidersPage },
      { path: 'accept-invite', Component: AcceptInvitePage },
      { path: 'jwks', Component: JwksPage },
      { path: '*', Component: NotFoundPage },
    ],
  },
  {
    path: '*',
    Component: NotFoundPage,
  },
])
