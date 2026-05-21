import AppsOutlinedIcon from '@mui/icons-material/AppsOutlined'
import ArticleOutlinedIcon from '@mui/icons-material/ArticleOutlined'
import BusinessOutlinedIcon from '@mui/icons-material/BusinessOutlined'
import DarkModeOutlinedIcon from '@mui/icons-material/DarkModeOutlined'
import DashboardOutlinedIcon from '@mui/icons-material/DashboardOutlined'
import GroupOutlinedIcon from '@mui/icons-material/GroupOutlined'
import KeyOutlinedIcon from '@mui/icons-material/KeyOutlined'
import LightModeOutlinedIcon from '@mui/icons-material/LightModeOutlined'
import LogoutOutlinedIcon from '@mui/icons-material/LogoutOutlined'
import MenuIcon from '@mui/icons-material/Menu'
import PersonOutlinedIcon from '@mui/icons-material/PersonOutlined'
import SecurityOutlinedIcon from '@mui/icons-material/SecurityOutlined'
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined'
import VpnKeyOutlinedIcon from '@mui/icons-material/VpnKeyOutlined'
import {
  AppBar,
  Box,
  Chip,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Tooltip,
  Typography,
} from '@mui/material'
import type { ReactElement } from 'react'
import { useMemo, useState } from 'react'
import { Link, Outlet, useLocation } from 'react-router'
import { PlatformBrand } from './ui/PlatformBrand'
import { useAuth } from '../contexts/AuthContext'
import { useThemeMode } from '../contexts/ThemeModeContext'
import { useTenant } from '../contexts/TenantContext'
import { buildLogoutUrl } from '../services/oidcService'
import { layout } from '../theme'
import { clearAuthSession } from '../utils/authStorage'

const appBarHeight = 64

interface NavItem {
  to: string
  label: string
  icon: ReactElement
}

interface NavGroup {
  label: string
  items: NavItem[]
}

function buildNavGroups(isPlatformAdmin: boolean): NavGroup[] {
  const groups: NavGroup[] = [
    {
      label: 'Geral',
      items: [{ to: '/', label: 'Dashboard', icon: <DashboardOutlinedIcon /> }],
    },
    {
      label: 'Conta',
      items: [
        { to: '/profile', label: 'Meu Perfil', icon: <PersonOutlinedIcon /> },
        { to: '/sessions', label: 'Sessões', icon: <SecurityOutlinedIcon /> },
      ],
    },
    {
      label: 'Tenants',
      items: [
        { to: '/tenants', label: 'Tenants', icon: <BusinessOutlinedIcon /> },
        { to: '/memberships', label: 'Memberships', icon: <GroupOutlinedIcon /> },
        { to: '/tenant-roles', label: 'Papéis do Tenant', icon: <VpnKeyOutlinedIcon /> },
      ],
    },
    {
      label: 'Aplicações',
      items: [{ to: '/applications', label: 'Applications', icon: <AppsOutlinedIcon /> }],
    },
    {
      label: 'Segurança',
      items: [
        { to: '/audit-logs', label: 'Audit Logs', icon: <ArticleOutlinedIcon /> },
        { to: '/jwks', label: 'JWKS', icon: <KeyOutlinedIcon /> },
      ],
    },
  ]

  if (isPlatformAdmin) {
    groups.push({
      label: 'Plataforma',
      items: [{ to: '/identity-providers', label: 'Identity Providers', icon: <SettingsOutlinedIcon /> }],
    })
  }

  return groups
}

function isNavActive(pathname: string, to: string): boolean {
  if (to === '/') {
    return pathname === '/'
  }
  return pathname === to || pathname.startsWith(`${to}/`)
}

export function AppLayout() {
  const location = useLocation()
  const { logoutLocal, email, platformRoles } = useAuth()
  const { tenantId } = useTenant()
  const { mode, toggleMode } = useThemeMode()
  const [sidebarOpen, setSidebarOpen] = useState(false)

  const currentPath = useMemo(() => location.pathname, [location.pathname])
  const isPlatformAdmin = platformRoles.includes('plat_admin')
  const navGroups = useMemo(() => buildNavGroups(isPlatformAdmin), [isPlatformAdmin])

  function handleLogout(): void {
    logoutLocal()
    clearAuthSession()
    window.location.assign(buildLogoutUrl(`${window.location.origin}/login`))
  }

  function handleToggleSidebar(): void {
    setSidebarOpen((prev) => !prev)
  }

  function handleCloseSidebar(): void {
    setSidebarOpen(false)
  }

  const sidebarHeader = (
    <Box
      sx={{
        px: 2,
        height: appBarHeight,
        flexShrink: 0,
        display: 'flex',
        alignItems: 'center',
        borderBottom: 1,
        borderColor: 'divider',
      }}
    >
      <PlatformBrand logoSize={36} to="/" />
    </Box>
  )

  const navList = (
    <List sx={{ px: 1.5, py: 1, flex: 1, overflowY: 'auto' }}>
      {navGroups.map((group, groupIndex) => (
        <Box key={group.label}>
          {groupIndex > 0 ? <Divider sx={{ my: 1.5 }} /> : null}
          <Typography
            variant="caption"
            color="text.secondary"
            sx={{ px: 1.5, py: 0.5, display: 'block', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.06em' }}
          >
            {group.label}
          </Typography>
          {group.items.map((item) => (
            <ListItemButton
              key={item.to}
              component={Link}
              to={item.to}
              selected={isNavActive(currentPath, item.to)}
              onClick={handleCloseSidebar}
            >
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} slotProps={{ primary: { sx: { fontWeight: 500 } } }} />
            </ListItemButton>
          ))}
        </Box>
      ))}
    </List>
  )

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <Drawer
        variant="temporary"
        open={sidebarOpen}
        onClose={handleCloseSidebar}
        ModalProps={{ keepMounted: true }}
        sx={{
          zIndex: (theme) => theme.zIndex.modal,
          '& .MuiBackdrop-root': {
            backgroundColor: 'rgba(0, 0, 0, 0.45)',
          },
          '& .MuiDrawer-paper': {
            boxSizing: 'border-box',
            width: layout.sidebarWidth,
            top: 0,
            height: '100vh',
            display: 'flex',
            flexDirection: 'column',
          },
        }}
      >
        {sidebarHeader}
        {navList}
      </Drawer>

      <Box sx={{ display: 'flex', flexDirection: 'column', flexGrow: 1, minWidth: 0, width: '100%' }}>
        <AppBar position="fixed" color="inherit" elevation={0} sx={{ width: '100%', left: 0 }}>
          <Toolbar sx={{ gap: 1, minHeight: appBarHeight, color: 'text.primary' }}>
            <Tooltip title={sidebarOpen ? 'Recolher menu' : 'Abrir menu'}>
              <IconButton
                edge="start"
                color="inherit"
                onClick={handleToggleSidebar}
                aria-label={sidebarOpen ? 'Recolher menu' : 'Abrir menu'}
              >
                <MenuIcon />
              </IconButton>
            </Tooltip>
            <PlatformBrand logoSize={32} to="/" />
            <Box sx={{ ml: 'auto', display: 'flex', alignItems: 'center', gap: 1 }}>
              {tenantId ? (
                <Chip
                  size="small"
                  label={`Tenant: ${tenantId.slice(0, 8)}…`}
                  variant="outlined"
                  sx={{ display: { xs: 'none', sm: 'flex' } }}
                />
              ) : (
                <Chip size="small" label="Sem tenant" variant="outlined" color="warning" sx={{ display: { xs: 'none', sm: 'flex' } }} />
              )}
              <Tooltip title={mode === 'light' ? 'Modo escuro' : 'Modo claro'}>
                <IconButton color="inherit" onClick={toggleMode} aria-label="Alternar tema">
                  {mode === 'light' ? <DarkModeOutlinedIcon /> : <LightModeOutlinedIcon />}
                </IconButton>
              </Tooltip>
              {email ? (
                <Typography variant="body2" color="text.secondary" sx={{ display: { xs: 'none', lg: 'block' }, maxWidth: 180 }} noWrap>
                  {email}
                </Typography>
              ) : null}
              <Tooltip title="Sair">
                <IconButton onClick={() => void handleLogout()} aria-label="Logout" color="inherit">
                  <LogoutOutlinedIcon />
                </IconButton>
              </Tooltip>
            </Box>
          </Toolbar>
        </AppBar>

        <Box
          component="main"
          sx={{
            flexGrow: 1,
            mt: `${appBarHeight}px`,
            px: { xs: 2, sm: 3, lg: 4 },
            py: 3,
          }}
        >
          <Box sx={{ width: '100%', maxWidth: layout.contentMaxWidth, mx: 'auto' }}>
            <Outlet />
          </Box>
        </Box>
      </Box>
    </Box>
  )
}
