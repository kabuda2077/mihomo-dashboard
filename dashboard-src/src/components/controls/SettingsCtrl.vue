<template>
  <CtrlsBar>
    <div
      ref="menuRef"
      class="scrollbar-hidden p-1 px-2"
      @touchstart.passive.stop
      @touchmove.passive.stop
      @touchend.passive.stop
    >
      <div class="flex w-full gap-2">
        <div class="relative mx-auto flex max-w-6xl flex-1 gap-2">
          <div
            v-if="showActiveIndicator"
            class="bg-neutral absolute top-1 left-0 -z-1 h-8 rounded-lg"
            :class="[!isSwiping ? 'transition-transform duration-300 will-change-transform' : '']"
            :style="activeStyle"
          ></div>
          <div
            v-for="item in menuItems"
            :key="item.key"
            ref="menuItemRefs"
            :data-key="item.key"
            :id="`menu-item-${item.key}`"
            class="btn btn-ghost btn-sm my-1 flex-1"
            :class="[
              !showActiveIndicator
                ? 'hover:btn hover:btn-neutral'
                : activeMenuKey === item.key
                  ? 'text-neutral-content bg-transparent'
                  : '',
            ]"
            @click="handleMenuClick(item.key)"
          >
            <component
              :is="item.icon"
              class="h-5 w-5"
            />
            <span class="hidden text-sm lg:block">
              {{ $t(item.label) }}
            </span>
          </div>
        </div>
        <button
          class="btn btn-circle btn-sm my-auto"
          @click="showVisibilityDialog = true"
        >
          <Cog6ToothIcon class="h-4 w-4" />
        </button>
      </div>
      <SettingsVisibilityDialog
        v-model="showVisibilityDialog"
        :two-columns-available="twoColumnsAvailable"
      />
    </div>
  </CtrlsBar>
</template>

<script setup lang="ts">
import { useCtrlsBar } from '@/composables/useCtrlsBar'
import { SETTINGS_MENU_KEY } from '@/constant'
import { Cog6ToothIcon } from '@heroicons/vue/24/outline'
import { useElementSize, useSwipe } from '@vueuse/core'
import type { Component } from 'vue'
import { computed, nextTick, ref, watch } from 'vue'
import CtrlsBar from '../common/CtrlsBar.vue'
import SettingsVisibilityDialog from './SettingsVisibilityDialog.vue'

type MenuItem = {
  key: SETTINGS_MENU_KEY
  label: string
  icon: Component
  component: Component
}

const props = defineProps<{
  menuItems: MenuItem[]
  activeMenuKey: SETTINGS_MENU_KEY
  showActiveIndicator?: boolean
  twoColumnsAvailable: boolean
}>()

const emit = defineEmits<{
  (e: 'menu-click', key: SETTINGS_MENU_KEY): void
}>()

const showVisibilityDialog = ref(false)

const menuRef = ref<HTMLDivElement>()
const menuItemRefs = ref<HTMLLIElement[]>([])
const { width } = useElementSize(menuRef)
const activeLeft = ref(0)
const activeWidth = ref(0)
const activeStyle = computed(() => {
  return {
    transform: `translateX(${activeLeft.value}px)`,
    width: `${activeWidth.value}px`,
  }
})

useCtrlsBar()

const updateActiveMenuLeft = async () => {
  await nextTick()
  const itemRef = menuItemRefs.value.find((el) => el.dataset.key === props.activeMenuKey)
  if (itemRef) {
    activeLeft.value = itemRef.offsetLeft
  }
}

const updateActiveMenuWidth = async () => {
  await nextTick()
  const itemRef = menuItemRefs.value.find((el) => el.dataset.key === props.activeMenuKey)
  if (itemRef) {
    activeWidth.value = itemRef.offsetWidth
  }
}

const { isSwiping } = useSwipe(menuRef, {
  passive: false,
  onSwipe(e: TouchEvent) {
    if (!menuRef.value) return
    const menuRect = menuRef.value.getBoundingClientRect()
    const relativeX = e.touches[0].clientX - menuRect.left

    activeLeft.value = Math.max(
      0,
      Math.min(
        relativeX - activeWidth.value / 2,
        menuRef.value.offsetWidth - activeWidth.value - 16,
      ),
    )
    const targetKey = getMenuItemAtPosition(e.touches[0].clientX)
    if (targetKey && targetKey !== props.activeMenuKey) {
      emit('menu-click', targetKey)
    }
  },
  onSwipeEnd() {
    updateActiveMenuLeft()
  },
})

const handleMenuClick = (key: SETTINGS_MENU_KEY) => {
  if (isSwiping.value) return
  emit('menu-click', key)
}

const getMenuItemAtPosition = (x: number): SETTINGS_MENU_KEY | null => {
  if (!menuRef.value) return null

  const menuRect = menuRef.value.getBoundingClientRect()
  const relativeX = x - menuRect.left

  // 找到触摸位置对应的菜单项
  for (const itemEl of menuItemRefs.value) {
    const itemRect = itemEl.getBoundingClientRect()
    const itemRelativeX = itemRect.left - menuRect.left
    const itemWidth = itemRect.width

    if (relativeX >= itemRelativeX && relativeX <= itemRelativeX + itemWidth) {
      return itemEl.dataset.key as SETTINGS_MENU_KEY
    }
  }

  return null
}

watch(
  () => props.activeMenuKey,
  () => {
    if (isSwiping.value) return
    updateActiveMenuLeft()
  },
  {
    immediate: true,
  },
)

watch(
  () => [width.value, props.menuItems],
  () => {
    updateActiveMenuWidth()
    updateActiveMenuLeft()
  },
  { immediate: true },
)
</script>
