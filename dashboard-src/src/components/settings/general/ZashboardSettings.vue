<template>
  <div class="relative flex flex-col text-sm">
    <div class="flex items-center gap-2 px-1">
      <div class="indicator">
        <a
          href="https://github.com/Zephyruso/zashboard"
          target="_blank"
          class="text-lg font-semibold"
        >
          zashboard
          <span class="text-sm font-normal opacity-50">
            {{ zashboardVersion }}
            <span
              v-if="commitId"
              class="text-xs"
            >
              {{ commitId }}
            </span>
          </span>
        </a>
      </div>
    </div>

    <div
      v-if="isVisibleActions"
      class="my-3 flex flex-col gap-3"
    >
      <DashboardSettings />
    </div>

    <StyleSettings />
    <GeneralSettings />
  </div>
</template>

<script setup lang="ts">
import { zashboardVersion } from '@/api'
import { useIsSettingVisible } from '@/composables/settings'
import { GENERAL_ITEM_KEYS } from '@/config/settingsItems'
import DashboardSettings from '../../common/DashboardSettings.vue'
import GeneralSettings from './GeneralSettings.vue'
import StyleSettings from './StyleSettings.vue'

const k = GENERAL_ITEM_KEYS
const isVisibleActions = useIsSettingVisible(k.actions)

const commitId = __COMMIT_ID__
</script>
