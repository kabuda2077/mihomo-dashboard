import type { Backend } from '@/types'
import { useStorage } from '@vueuse/core'
import { v4 as uuid } from 'uuid'
import { computed } from 'vue'
import { sourceIPLabelList } from './settings'

export const backendList = useStorage<Backend[]>('setup/api-list', [])
export const activeUuid = useStorage<string>('setup/active-uuid', '')
export const activeBackend = computed(() =>
  backendList.value.find((backend) => backend.uuid === activeUuid.value),
)

export const switchActiveBackend = (direction: 1 | -1) => {
  if (backendList.value.length < 2) {
    return null
  }

  const currentIndex = backendList.value.findIndex((backend) => backend.uuid === activeUuid.value)
  const startIndex = currentIndex >= 0 ? currentIndex : 0
  const nextIndex = (startIndex + direction + backendList.value.length) % backendList.value.length

  const nextBackend = backendList.value[nextIndex]

  if (!nextBackend) {
    return null
  }

  activeUuid.value = nextBackend.uuid
  return nextBackend
}

const isSameBackendEndpoint = (saved: Backend, backend: Omit<Backend, 'uuid'>) => {
  return (
    saved.protocol === backend.protocol &&
    saved.host === backend.host &&
    saved.port === backend.port &&
    saved.secondaryPath === backend.secondaryPath &&
    saved.password === backend.password
  )
}

export const addBackend = (backend: Omit<Backend, 'uuid'>) => {
  const matchingBackends = backendList.value.filter((end) => isSameBackendEndpoint(end, backend))
  const currentEnd = matchingBackends[0]

  if (currentEnd) {
    Object.assign(currentEnd, backend)
    if (matchingBackends.length > 1) {
      const duplicateIds = new Set(matchingBackends.slice(1).map((end) => end.uuid))
      backendList.value = backendList.value.filter((end) => !duplicateIds.has(end.uuid))
    }
    activeUuid.value = currentEnd.uuid
    return
  }

  const id = uuid()

  backendList.value.push({
    ...backend,
    uuid: id,
  })
  activeUuid.value = id
}

export const updateBackend = (uuid: string, backend: Omit<Backend, 'uuid'>) => {
  const index = backendList.value.findIndex((end) => end.uuid === uuid)
  if (index !== -1) {
    backendList.value[index] = {
      ...backend,
      uuid,
    }
  }
}

export const removeBackend = (uuid: string) => {
  backendList.value = backendList.value.filter((end) => end.uuid !== uuid)
  sourceIPLabelList.value.forEach((label) => {
    if (label.scope && label.scope.includes(uuid)) {
      label.scope = label.scope.filter((scope) => scope !== uuid)
      if (!label.scope.length) {
        delete label.scope
      }
    }
  })
}
