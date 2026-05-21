import { Link, Outlet } from 'react-router-dom'
import { env } from '../config/env'
import { clearSession } from '../utils/authStorage'

export function Layout() {
  function handleLogout() {
    clearSession()
    const redirect = `${window.location.origin}/login`
    window.location.href = `${env.idpAuthority}/connect/logout?client_id=${encodeURIComponent(env.idpClientId)}&post_logout_redirect_uri=${encodeURIComponent(redirect)}`
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <Link to="/dashboard" className="brand">
          PulseCRM
        </Link>
        <nav>
          <Link to="/dashboard">Dashboard</Link>
          <Link to="/contacts">Contatos</Link>
        </nav>
        <button type="button" className="btn-ghost" onClick={handleLogout}>
          Sair
        </button>
      </header>
      <main className="main">
        <Outlet />
      </main>
    </div>
  )
}
