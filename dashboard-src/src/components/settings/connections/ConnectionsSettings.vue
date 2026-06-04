<template>
  <!-- connections -->
  <div
    v-if="hasVisibleItems"
    class="flex flex-col gap-3 text-sm"
  >
    <div class="settings-grid">
      <div
        v-if="isVisibleConnectionStyle"
        class="setting-item"
      >
        <div class="setting-item-label">
          {{ $t('connectionStyle') }}
        </div>
        <select
          class="select select-sm min-w-24"
          v-model="connectionDisplayStyle"
        >
          <option :value="CONNECTION_DISPLAY_STYLE.AUTO">
            {{ $t('auto') }}
          </option>
          <option :value="CONNECTION_DISPLAY_STYLE.CARD">
            {{ $t('card') }}
          </option>
          <option :value="CONNECTION_DISPLAY_STYLE.TABLE">
            {{ $t('table') }}
          </option>
        </select>
      </div>
      <div
        v-if="isVisibleProxyChainDirection"
        class="setting-item"
      >
        <div class="setting-item-label">
          {{ $t('proxyChainDirection') }}
        </div>
        <select
          class="select select-sm w-24"
          v-model="proxyChainDirection"
        >
          <option
            v-for="opt in Object.values(PROXY_CHAIN_DIRECTION)"
            :key="opt"
            :value="opt"
          >
            {{ $t(opt) }}
          </option>
        </select>
      </div>
      <template v-if="!isConnectionCard">
        <div
          v-if="isVisibleTableWidthMode"
          class="setting-item"
        >
          <div class="setting-item-label">
            {{ $t('tableWidthMode') }}
          </div>
          <select
            class="select select-sm min-w-24"
            v-model="tableWidthMode"
          >
            <option
              v-for="opt in Object.values(TABLE_WIDTH_MODE)"
              :key="opt"
              :value="opt"
            >
              {{ $t(opt) }}
            </option>
          </select>
        </div>
        <div
          v-if="isVisibleTableSize"
          class="setting-item"
        >
          <div class="setting-item-label">
            {{ $t('tableSize') }}
          </div>
          <select
            class="select select-sm min-w-24"
            v-model="tableSize"
          >
            <option
              v-for="opt in Object.values(TABLE_SIZE)"
              :key="opt"
              :value="opt"
            >
              {{ $t(opt) }}
            </option>
          </select>
        </div>
      </template>
      <SourceIPLabels v-if="isVisibleSourceIPLabels" />
    </div>
  </div>
</template>

<script setup lang="ts">
import SourceIPLabels from '@/components/settings/connections/SourceIPLabels.vue'
import { useHasAnyVisibleSetting, useIsSettingVisible } from '@/composables/settings'
import { CONNECTIONS_ITEM_KEYS, getItemKeysByCategory } from '@/config/settingsItems'
import {
  CONNECTION_DISPLAY_STYLE,
  PROXY_CHAIN_DIRECTION,
  SETTINGS_MENU_KEY,
  TABLE_SIZE,
  TABLE_WIDTH_MODE,
} from '@/constant'
import {
  connectionDisplayStyle,
  isConnectionCard,
  proxyChainDirection,
  tableSize,
  tableWidthMode,
} from '@/store/settings'

const k = CONNECTIONS_ITEM_KEYS
const isVisibleConnectionStyle = useIsSettingVisible(k.connectionStyle)
const isVisibleProxyChainDirection = useIsSettingVisible(k.proxyChainDirection)
const isVisibleTableWidthMode = useIsSettingVisible(k.tableWidthMode)
const isVisibleTableSize = useIsSettingVisible(k.tableSize)
const isVisibleSourceIPLabels = useIsSettingVisible(k.sourceIPLabels)

const hasVisibleItems = useHasAnyVisibleSetting(
  getItemKeysByCategory(SETTINGS_MENU_KEY.connections),
)
</script>
