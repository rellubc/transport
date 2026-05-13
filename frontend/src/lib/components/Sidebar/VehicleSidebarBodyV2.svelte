<script lang="ts">
  import { getSydneyNowSeconds, secondsToTime, stopDelayColour, stopDelayText, timeFromNow } from "$lib/helpers";
  import type { VehicleStopTime } from "$lib/types/stoptimes.types";
  import type { Vehicle } from "$lib/types/vehicles.types";

  const BUFFER_PX = 32

  let { stopTimes, getStopInfo }: {
    stopTimes: VehicleStopTime[]
    getStopInfo: (stopId: string) => void
  } = $props();

  let barSegmentHeight = $state<number>(60)

</script>

<div style:scrollbar-width="none" class="w-full h-full flex flex-col overflow-y-scroll">
  <div class="min-h-screen w-full">
    {#each stopTimes as stopTime, index}
      <button onclick={() => getStopInfo(stopTime.stopId)} class="w-full flex flex-row gap-2 cursor-pointer">
        <div class="relative w-4 flex flex-col justify-center items-center">
          {#if index !== stopTimes.length - 1}
            <div style:height={`${barSegmentHeight}px`} style:background-color={`#${stopTimes[0].routeColour}`} class="absolute top-7.5 left-1.5 w-1 z-10"></div>
            <div style:height={`${stopTimes[index + 1].stopProgress / 100 * barSegmentHeight }px`} class="absolute top-7.5 left-1.5 w-1 bg-gray-300 z-20"></div>
          {/if}
          <div style:background-color={stopTime.progress === "passed" ? undefined : `#${stopTime.routeColour}`} class="h-4 w-4 flex flex-row justify-center items-center rounded-full z-30 {stopTime.progress === 'passed' ? 'bg-gray-300' : ''}">
            <div class="h-2 w-2 bg-white rounded-full z-40"></div>
          </div>
        </div>

        <div class="w-full flex flex-row justify-between items-center py-2">
          <div style:opacity={stopTime.progress === 'passed' ? 0.5 : 1} class="flex flex-col items-start">
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
      </button>
    {/each}
  </div>
</div>