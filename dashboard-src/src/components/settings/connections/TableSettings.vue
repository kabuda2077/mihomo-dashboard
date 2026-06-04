<template>
  <div class="setting-item">
    <div class="setting-item-label">
      {{ $t('showFullProxyChain') }}
    </div>
    <input
      type="checkbox"
      class="toggle"
      v-model="showFullProxyChain"
    />
  </div>
  <div class="flex flex-col">
    <div class="m-4 mb-0">{{ $t('customTableColumns') }}</div>
    <div class="flex gap-4 rounded-sm p-4">
      <Draggable
        class="bg-base-200 flex flex-1 flex-col gap-2 p-2"
        v-model="connectionTableColumns"
        group="list"
        :animation="150"
        :item-key="(id: string) => id"
      >
        <template #item="{ element }">
          <button class="btn btn-sm bg-base-100 cursor-move shadow-sm">
            {{ $t(element) }}
          </button>
        </template>
      </Draggable>
      <Draggable
        class="flex flex-1 flex-col gap-2 p-2"
        v-model="restOfColumns"
        group="list"
        :animation="150"
        :item-key="(id: string) => id"
      >
        <template #item="{ element }">
          <button class="btn btn-sm cursor-move">
            {{ $t(element) }}
          </button>
        </template>
      </Draggable>
    </div>
  </div>
</template>

<script setup lang="ts">
import { CONNECTIONS_TABLE_ACCESSOR_KEY } from '@/constant'
import { connectionTableColumns, showFullProxyChain } from '@/store/settings'
import { ref } from 'vue'
import Draggable from 'vuedraggable'

const restOfColumns = ref(
  Object.values(CONNECTIONS_TABLE_ACCESSOR_KEY).filter(
    (key) => !connectionTableColumns.value.includes(key),
  ),
)
</script>
