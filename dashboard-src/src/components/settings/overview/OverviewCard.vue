<template>
  <!-- overview -->
  <div class="mb-3 flex flex-col gap-3">
    <ChartsCard
      v-if="isVisibleOverviewCard"
      class="shadow-none"
    />
    <NetworkCard
      v-if="isVisibleNetworkCard"
      class="shadow-none"
    />
  </div>
</template>

<script setup lang="ts">
import ChartsCard from '@/components/overview/ChartsCard.vue'
import NetworkCard from '@/components/overview/NetworkCard.vue'
import { useIsSettingVisible } from '@/composables/settings'
import { OVERVIEW_ITEM_KEYS } from '@/config/settingsItems'

const isVisibleOverviewCard = useIsSettingVisible(OVERVIEW_ITEM_KEYS.chartsCard)
const isVisibleNetworkCard = useIsSettingVisible(OVERVIEW_ITEM_KEYS.networkCard)
</script>
