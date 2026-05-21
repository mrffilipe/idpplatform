import { Navigate, useLocation } from 'react-router-dom'
import { isLoggedIn } from '../utils/authStorage'

export function RequireAuth({ children }: { children: React.ReactNode }) {
  const location = useLocation()
  if (!isLoggedIn()) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }
  return <>{children}</>
}
