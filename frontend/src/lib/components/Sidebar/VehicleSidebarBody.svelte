<script lang="ts">
  import { secondsToTime, stopDelayColour, stopDelayText } from "$lib/helpers";
  import type { VehicleStopTime } from "$lib/types/stoptimes.types";

  let { listElement = $bindable(), stopTimes }: { listElement: HTMLElement | null, stopTimes: VehicleStopTime[] } = $props();

  let baseHeight = $state<number>(0)
  let travelledHeight = $state<number>(0)

  $effect(() => {
    baseHeight = (stopTimes.length - 1) * 60 + stopTimes.length
    const passedStops = stopTimes.filter((stopTime) => stopTime.progress === "passed")
    const firstNotPassed = stopTimes.find((stopTime) => stopTime.progress !== "passed")
    travelledHeight = Math.max((passedStops.length - 1) * 60 + passedStops.length + (firstNotPassed ? (firstNotPassed.stopProgress / 100 * 60) : 0), 0)
  })

</script>

<div bind:this={listElement} style:scrollbar-width="none" class="relative h-full flex flex-col overflow-y-scroll">
  <div style:height={`${baseHeight}px`} style:background-color={`#${stopTimes[0].routeColour}`} class="absolute top-7.5 left-1.5 w-1 z-10"></div>
  <div style:height={`${travelledHeight}px`} class="absolute top-7.5 left-1.5 w-1 bg-gray-300 z-20"></div>
  <div class="min-h-screen">
    {#each stopTimes as stopTime, index}
      <div class="relative flex flex-row justify-between items-center py-2">
        <div class="absolute h-4 w-4 rounded-full z-30 {stopTime.progress === 'passed' ? 'bg-gray-300' : ''}" style:background-color={stopTime.progress === "passed" ? undefined : `#${stopTime.routeColour}`}></div>
        <div class="absolute left-1 h-2 w-2 bg-white rounded-full z-40"></div>
        <div style:opacity={stopTime.progress === 'passed' ? 0.5 : 1} class="flex flex-col pl-8">
          <p class="font-bold">{stopTime.stopName.split(',')[0]}</p>
          <p class="text-sm">{stopTime.stopName.split(',')[1]}</p>
        </div>
        <div style:opacity={stopTime.progress === 'passed' ? 0.5 : 1} class="flex flex-col justify-center w-20">
          <p class="text-center">{secondsToTime(stopTime.displayTime)}</p>
          {#if stopTime.isRealtime}
            <p style:color={stopDelayColour(stopTime.departureDelay)} class="text-sm text-center">{stopDelayText(stopTime.departureDelay)}</p>
          {:else}
            <p class="text-sm text-center">Scheduled</p>
          {/if}
        </div>
      </div>
      {#if index !== stopTimes.length - 1}
        <div class="flex flex-row justify-end">
          <div class="w-[calc(100%-1.5rem)] border-b border-slate-200"></div>
        </div>
      {/if}
    {/each}
  </div>
</div>