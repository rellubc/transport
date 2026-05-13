<script lang="ts">
  import { getSydneyNowSeconds, secondsToTime, stopDelayColour, stopDelayText, timeFromNow } from "$lib/helpers";
  import type { Stop } from "$lib/types/stops.types";
  import type { StopStopTime } from "$lib/types/stoptimes.types";

  const BUFFER_PX = 32

  let { listElement = $bindable(), activeStop, stopTimes, getVehicleInfo }: {
    listElement: HTMLElement | null
    activeStop: Stop
    stopTimes: StopStopTime[]
    getVehicleInfo: (tripId: string) => void
  } = $props();

  let page = $state<string>("overview")

  const overviewMappings = $derived.by(() => {
    const mappings: Record<string, StopStopTime[]> = {}

    for (const stopTime of stopTimes) {
      if (stopTime.stopType !== "depart" && stopTime.stopType !== "stop") continue

      if (!mappings[stopTime.tripHeadsign]) mappings[stopTime.tripHeadsign] = []
      mappings[stopTime.tripHeadsign].push(stopTime)
    }

    return mappings
  })

</script>

{#if page === "overview"}
  <div style:scrollbar-width="none" class="flex flex-col gap-4 overflow-y-scroll">
    <div>
      <p class="font-bold">Overview</p>
      {#if Object.keys(overviewMappings).length === 0}
        <p>This {activeStop.stopParentStation ? "platform" : "station"} has no upcoming departures for the rest of today.</p>
      {:else}
        <p>This {activeStop.stopParentStation ? "platform" : "station"} has upcoming departures terminating at:</p>
        <ul class="list-disc pl-4">
          {#each Object.keys(overviewMappings) as headsign}
            <li>{headsign}</li>
          {/each}
        </ul>
      {/if}
      <button onclick={() => { page = "schedule" }} class="cursor-pointer">View all services &#8594;</button>
    </div>
    <div class="flex flex-col gap-2">
      {#each Object.entries(overviewMappings) as [headsign, stopTimes]}
        <div class="flex flex-col gap-2 pb-2">
          <p class="font-bold">{headsign}</p>
          <ul class="flex flex-col gap-2">
            {#each stopTimes.slice(0, 5) as stopTime}
              <li class="flex flex-row gap-2">
                <div style:background-color={`#${stopTime.routeColour}`} class="w-10 h-6 flex flex-row justify-center items-center rounded-md">
                  <p class="text-white text-xs font-bold">{stopTime.routeShortName ? stopTime.routeShortName : "NR"}</p>
                </div>
                {secondsToTime(stopTime.displayTime)} ({timeFromNow(stopTime.displayTime)})
              </li>
            {/each}
          </ul>
        </div>
      {/each}
    </div>
  </div>
{:else if page === "schedule"}
  <button onclick={() => { page = "overview" }} class="absolute top-2 text-black z-50 cursor-pointer">&#x2039; Back</button>
  <div bind:this={listElement} style:scrollbar-width="none" class="relative flex flex-col overflow-y-scroll overflow-visible">
    <div class="min-h-screen">
      <div style:height={`${BUFFER_PX}px`} class="w-full shrink-0"></div>
      {#each stopTimes as stopTime, index}
        <button onclick={() => { getVehicleInfo(stopTime.tripId); page = "vehicle" }} style:opacity={stopTime.stopType === 'pass' || getSydneyNowSeconds() > stopTime.effectiveDepartureTime ? 0.5 : 1} class="w-full flex flex-row justify-between items-center py-2 cursor-pointer">
          <div class="flex flex-row items-center gap-4">
            <div style:background-color={`#${stopTime.routeColour}`} class="w-12 h-6 flex flex-row justify-center items-center rounded-md">
              <p class="text-white font-bold">{stopTime.routeShortName ? stopTime.routeShortName : "NR"}</p>
            </div>
            <div class="flex flex-col items-start">
              {#if !activeStop.stopParentStation}
                {#if stopTime.routeShortName && ["L1", "L2", "L3", "LX"].includes(stopTime.routeShortName[0])}
                  <p class="text-sm font-light">Lighrail {stopTime.stopName.split(', ')[1]}</p>
                {:else}
                  <p class="text-sm font-light">{stopTime.stopName.split(', ')[1]}</p>
                {/if}
              {/if}
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
        </button>
        {#if stopTime.stopType === 'pass'}
          <p>Does not stop</p>
        {:else if stopTime.stopType === 'terminate'}
          <p>Terminates</p>
        {:else if stopTime.stopType === 'continues'}
          <p>Terminates, continues</p>
        {/if}
        {#if index !== stopTimes.length - 1}
          <div class="border-b border-slate-200"></div>
        {/if}
      {/each}
      <div style:height={`${BUFFER_PX}px`} class="w-full shrink-0"></div>
    </div>
  </div>
{/if}