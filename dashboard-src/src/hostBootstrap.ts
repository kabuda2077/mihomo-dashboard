import { getBackendFromUrl } from '@/helper/utils'
import { addBackend } from '@/store/setup'
import type { Backend } from '@/types'

type HostState = {
  apiUrl?: string
  secret?: string
}

type HostWindow = Window & {
  __mihomoApplyBackend?: (state: HostState) => void
}

const disableSelfManagedUpdates = () => {
  localStorage.setItem('config/auto-upgrade', 'false')
  localStorage.setItem('config/check-upgrade-core', 'false')
  localStorage.setItem('config/auto-upgrade-core', 'false')
}

const normalizePath = (pathname: string) => {
  const path = pathname.replace(/\/$/, '')
  return path === '' || path === '/' ? '' : path
}

const backendFromApiUrl = (apiUrl: string | undefined, secret: string | undefined) => {
  if (!apiUrl) return null

  try {
    const url = new URL(apiUrl)
    return {
      protocol: url.protocol.replace(':', ''),
      host: url.hostname,
      port: url.port || (url.protocol === 'https:' ? '443' : '80'),
      secondaryPath: normalizePath(url.pathname),
      password: secret || '',
      label: 'Mihomo Dashboard',
      disableUpgradeCore: true,
    } satisfies Omit<Backend, 'uuid'>
  } catch {
    return null
  }
}

const applyBackend = (backend: Omit<Backend, 'uuid'> | null) => {
  if (!backend?.protocol || !backend.host || !backend.port) return
  addBackend(backend)
}

disableSelfManagedUpdates()
applyBackend(getBackendFromUrl())

;(window as HostWindow).__mihomoApplyBackend = (state) => {
  applyBackend(backendFromApiUrl(state.apiUrl, state.secret))
}