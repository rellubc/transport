<script lang="ts">
  import { onMount, tick } from "svelte";
  import type { PageData } from "./$types";
  import type { Stop } from "$lib/types/stops.types";
  import { getStops, setStops } from "$lib/stores.svelte";
  import { getStopStopTimes } from "$lib/api/stoptimes.api";
  import { LineColours, LineRoutes, ModeLabels } from "$lib/constants";
  import type { StopStopTime } from "$lib/types/stoptimes.types";
  import { getSydneyNow, secondsToTime, stopDelayColour, stopDelayText } from "$lib/helpers";
  
  let { data }: { data: PageData } = $props()

  const BUFFER_PX = 32

  let refreshInterval: ReturnType<typeof setInterval> | null = null

  let stopQuery = $state<string>('')
  let activeStop = $state<Stop | null>(null)

  let stopTimes = $state<StopStopTime[]>([])
  let listElement = $state<HTMLElement | null>(null)
  let sidebarElement = $state<HTMLElement | null>(null)
  let fetching = $state<boolean>(false)

  onMount(() => {
    const savedStopQuery = localStorage.getItem('stopQuery')
    if (savedStopQuery !== null) stopQuery = savedStopQuery
    setStops(data.stops)

    console.log(data.stops)
    return () => {
      if (refreshInterval) clearInterval(refreshInterval)
    }
  })

  $effect(() => {
    localStorage.setItem('stopQuery', stopQuery)
  })

  $effect(() => {
    console.log(listElement)
    if (!listElement) return
    const list = listElement
    list.scrollTop = BUFFER_PX

    const onScroll = async () => {
      if (fetching || stopTimes.length === 0) return
      if (list.scrollTop === 0) {
        fetching = true
        try {
          // make it look nicer - terminate and departures both appear --> attempt to make departures appear with terminate headsigns (departures dont have full headsigns for some reason -_-)
          const newTimes = await getStopStopTimes(activeStop!.stopId, "prev", stopTimes[0].effectiveDepartureTime)
          if (newTimes.length === 0) return
          stopTimes = [...newTimes, ...stopTimes]

          await tick()
          list.scrollTop = newTimes.length * 60 + newTimes.length
        } catch (error) {
          console.error(error)
        } finally {
          fetching = false
        }
      } else if (list.scrollTop + list.clientHeight === list.scrollHeight) {
        fetching = true
        try {
          const newTimes = await getStopStopTimes(activeStop!.stopId, "next", stopTimes[stopTimes.length - 1].effectiveDepartureTime)
          if (newTimes.length === 0) return
          stopTimes = [...stopTimes, ...newTimes]
        } catch (error) {
          console.error(error)
        } finally {
          fetching = false
        }
      }
    }

    list.addEventListener('scroll', onScroll)
    return () => list.removeEventListener('scroll', onScroll)
  })

  const getStopTimes = async (stop: Stop) => {
    try {
      stopTimes = await getStopStopTimes(stop.stopId, "initial", getSydneyNow())
      console.log("Stop times: ", $state.snapshot(stopTimes))
      activeStop = stop
      await tick()

      if (refreshInterval) clearInterval(refreshInterval)

      refreshInterval = setInterval(async () => {
        if (!listElement) return
        if (listElement.scrollHeight > BUFFER_PX * 2 + stopTimes.length * 60 + stopTimes.length - 1) return

        stopTimes = await getStopStopTimes(stop.stopId, "initial", getSydneyNow())
        console.log("Refreshed stop times: ", $state.snapshot(stopTimes))
      }, 10000)
    } catch (err) {
      console.error(err)
    }
  }
</script>

<svelte:window onclick={(e: MouseEvent) => {
  if (!activeStop) return
  if (sidebarElement && !sidebarElement.contains(e.target as Node)) {
    activeStop = null
    if (refreshInterval) clearInterval(refreshInterval)
  }
}}/>

<div class="pl-150 pt-50">
  <p>Stops</p>
  <input
    bind:value={stopQuery}
    placeholder="Search stops..."
  />
  {#each Object.entries(getStops()) as [mode, modeStops]}
    <p class="font-bold">{mode} - {ModeLabels[Number(mode)]}</p>
    <div class="flex flex-col items-start">
      {#each modeStops.filter((stop) => stop.stopName.toLowerCase().includes(stopQuery.toLowerCase())).slice(0, 20) as stop}
        <button class="cursor-pointer" onclick={() => getStopTimes(stop)}>{stop.stopName}</button>
      {/each}
    </div>
  {/each}
</div>

{#if activeStop}
  <div bind:this={sidebarElement} class="absolute top-4 left-4 bg-white w-md h-[calc(100vh-2rem)] flex flex-col p-8 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)]">
    <div class="sticky top-0 z-10 bg-white">
      <p class="text-2xl font-bold">{activeStop.stopName}</p>
      <p class="text-xs font-light">{activeStop.stopId}</p>
    </div>
    <!-- <div bind:this={listElement} /*style:scrollbar-width="none"*/ class="flex flex-col overflow-y-scroll"> -->
    <div bind:this={listElement} style:scrollbar-width="none" class="flex flex-col overflow-y-scroll">
      <div style:height={`${BUFFER_PX}px`} class="w-full shrink-0"></div>
        {#each stopTimes as stopTime, index}
          <div style:opacity={stopTime.stopType === 'pass' ? 0.5 : 1} class="flex flex-row justify-between items-center py-2">
            <div class="flex flex-row items-center gap-4">
              <div style:background-color={LineColours[stopTime.routeId.split('_')[0]]} class="w-12 h-6 flex flex-row justify-center items-center rounded-md">
                <p class="text-white font-bold">
                  {LineRoutes[stopTime.routeId.split('_')[0]]}
                </p>
              </div>
              <div>
                <p class="font-bold">{stopTime.tripHeadsign.split('via')[0]}</p>
                {#if stopTime.tripHeadsign.split('via').length > 1}
                  <p class="text-sm">via {stopTime.tripHeadsign.split('via')[1]}</p>
                {/if}  
              </div>
            </div>
            <div class="flex flex-col justify-center w-20">
              {#if stopTime.stopType === 'terminate'}
                <p class="text-center">{secondsToTime(stopTime.effectiveArrivalTime)}</p>
              {:else}
                <p class="text-center">{secondsToTime(stopTime.effectiveDepartureTime)}</p>
              {/if}
              {#if stopTime.isRealtime}
                <p style:color={stopDelayColour(stopTime.departureDelay)} class="text-sm text-center">{stopDelayText(stopTime.departureDelay)}</p>
              {:else}
                <p class="text-sm text-center">Scheduled</p>
              {/if}
              </div>
          </div>
          {#if index !== stopTimes.length - 1}
            <div class="border-b border-slate-200"></div>
          {/if}
        {/each}
      <div style:height={`${BUFFER_PX}px`} class="w-full shrink-0"></div>
    </div>
  </div>
{/if}