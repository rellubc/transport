<script lang="ts">
  import { secondsToTime, stopDelayColour, stopDelayText } from "$lib/helpers";
  import type { StopStopTime } from "$lib/types/stoptimes.types";

  const BUFFER_PX = 32

  let { listElement = $bindable(), stopTimes }: {
    listElement: HTMLElement | null
    stopTimes: StopStopTime[]
  } = $props();

</script>

<div bind:this={listElement} style:scrollbar-width="none" class="flex flex-col overflow-y-scroll">
  <div class="min-h-screen">
    <div style:height={`${BUFFER_PX}px`} class="w-full shrink-0"></div>
    {#each stopTimes as stopTime, index}
      <div style:opacity={stopTime.stopType === 'pass' ? 0.5 : 1} class="flex flex-row justify-between items-center py-2">
        <div class="flex flex-row items-center gap-4">
          <div style:background-color={`#${stopTime.routeColour}`} class="w-12 h-6 flex flex-row justify-center items-center rounded-md">
            <p class="text-white font-bold">{stopTime.routeShortName}</p>
          </div>
          <div>
            <p class="font-bold">{stopTime.tripHeadsign.split('via')[0]}</p>
            {#if stopTime.tripHeadsign.split('via').length > 1}
              <p class="text-sm">via {stopTime.tripHeadsign.split('via')[1]}</p>
            {/if}  
          </div>
        </div>
        <div class="flex flex-col justify-center w-20">
          <p class="text-center">{secondsToTime(stopTime.displayTime)}</p>
          {#if stopTime.isRealtime}
            <p style:color={stopDelayColour(stopTime.departureDelay)} class="text-sm text-center">{stopDelayText(stopTime.departureDelay)}</p>
          {:else}
            <p class="text-sm text-center">Scheduled</p>
          {/if}
          </div>
      </div>
      {#if stopTime.stopType === 'pass'}
        <p>Does not stop</p>
      {:else if stopTime.stopType === 'terminate'}
        {#if stopTime.hasContinuation}
          <p>Terminates, continues</p>
        {:else}
          <p>Terminates</p>
        {/if}
      {/if}
      {#if index !== stopTimes.length - 1}
        <div class="border-b border-slate-200"></div>
      {/if}
    {/each}
    <div style:height={`${BUFFER_PX}px`} class="w-full shrink-0"></div>
  </div>
</div>