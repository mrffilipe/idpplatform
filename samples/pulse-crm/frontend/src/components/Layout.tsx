import { Link, Outlet } from 'react-router-dom'
import { kyvoClient } from '../config/kyvoClient'
import { clearSession } from '../utils/authStorage'

export function Layout() {
  function handleLogout() {
    clearSession()
    kyvoClient.oidc.signOut(`${window.location.origin}/login`)
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
