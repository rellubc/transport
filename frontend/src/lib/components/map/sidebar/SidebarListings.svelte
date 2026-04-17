<script lang="ts">
  import type { RealtimeStopTime } from "$lib/types/stopTime";
  import SidebarListing from "./SidebarListing.svelte";

  const { colour, realtimeStopTimes }: { colour: string, realtimeStopTimes: RealtimeStopTime[] } = $props()

  let lastStop: number = $state(0)
  let offset: number = $state(20)
  
  $effect(() => {
    for (let i = realtimeStopTimes.length - 1; i > 0; i--) {
      if (realtimeStopTimes[i].status === 'stop') {
        lastStop = realtimeStopTimes.length - i - 1
        break
      }
    }

    let currentStop = 0
    let nextStop = 0
    let skips = 0
    for (let i = 0; i < realtimeStopTimes.length; i++) {
      if (!realtimeStopTimes[i].progress) {
        continue
      }
      if (realtimeStopTimes[i].progress === 'skipped') skips++ 
      if (realtimeStopTimes[i].progress === 'passed') skips = 0
      if (realtimeStopTimes[i].progress === 'not_passed') {
        currentStop = i - 1 - skips
        nextStop = i
        break
      }
    }

    let newOffset = 0
    skips = 0
    let passes = 0
    for (let i = 0; i <= currentStop; i++) {
      if (realtimeStopTimes[i].progress === 'passed') {
        if (realtimeStopTimes[i].consist.length === 0) newOffset += 76
        else newOffset += 94
        passes++
      }

      if (passes === 1) {
        newOffset += 60 * skips
        passes--
        skips = 0
      }

      if (realtimeStopTimes[i].progress === 'skipped') skips++
    }

    offset = newOffset + 20 - 94
    getProgress(currentStop, nextStop)
  })

  const getProgress = (currentStop: number, nextStop: number) => {
    // progress between stops
  }
</script>

<div class="h-full overflow-y-scroll">
  <div class="relative">
    <div
      style={`background-color: #d3d3d3; top: 20px; bottom: ${lastStop * 60 + 69}px`}
      class={`absolute left-4.25 w-1.5 z-20`}
    ></div>
    <div
      style={`background-color: ${colour}; top: ${offset}px; bottom: ${lastStop * 60 + 69}px`}
      class={`absolute left-4.25 w-1.5 z-30`}
    ></div>
    {#each realtimeStopTimes as stopTime, i}
      <SidebarListing colour={colour} stopTime={stopTime} last={i === realtimeStopTimes.length - 1} />
    {/each}
  </div>
</div>
