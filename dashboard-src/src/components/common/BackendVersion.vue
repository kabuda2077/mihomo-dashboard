<template>
  <div class="flex items-center gap-1 overflow-hidden">
    <img
      :src="isSingBox ? SingBoxLogo : MetacubexLogo"
      class="h-4 w-4 rounded-xs"
    />
    <span
      class="text-base-content/60 truncate"
      @mouseenter="checkTruncation"
    >
      {{ version }}
    </span>
  </div>
</template>

<script setup lang="ts">
import { isSingBox, version } from '@/api'
import MetacubexLogo from '@/assets/images/metacubex.jpg'
import SingBoxLogo from '@/assets/images/sing-box.svg'
import { checkTruncation } from '@/helper/tooltip'
</script>
