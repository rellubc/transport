<script lang="ts">
  import { onMount, tick } from "svelte";
  import type { PageData } from "./$types";
  import type { Stop } from "$lib/types/stops.types";
  import { getStops, setStops } from "$lib/stores.svelte";
  import { getStopStopTimes } from "$lib/api/stoptimes.api";
  import { ModeType } from "$lib/constants";
  import type { StopStopTime } from "$lib/types/stoptimes.types";
  import { getSydneyNow, secondsToTime, stopDelayColour, stopDelayText } from "$lib/helpers";
  
  let { data }: { data: PageData } = $props()

  const BUFFER_PX = 32

  let stopQuery = $state<string>('')
  let activeStop = $state<Stop | null>(null)

  let stopTimes = $state<StopStopTime[]>([])
  let listElement = $state<HTMLElement | null>(null)
  let fetching = $state<boolean>(false)

  onMount(() => {
    const savedStopQuery = localStorage.getItem('stopQuery')
    if (savedStopQuery !== null) stopQuery = savedStopQuery
    setStops(data.stops)

    console.log(data.stops)
  })

  $effect(() => {
    localStorage.setItem('stopQuery', stopQuery)
  })

  $effect(() => {
    if (!listElement) return
    const list = listElement

    const onScroll = async () => {
      if (fetching || stopTimes.length === 0) return

      if (list.scrollTop === 0) {
        fetching = true
        try {
          const newTimes = await getStopStopTimes(activeStop!.stopId, "prev", stopTimes[0].effectiveDepartureTime)
          if (newTimes.length === 0) return
          stopTimes = [...newTimes, ...stopTimes]

          await tick()
          list.scrollTop = newTimes.length * 44
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

      console.log($state.snapshot(stopTimes))
    }

    list.addEventListener('scroll', onScroll)
    return () => list.removeEventListener('scroll', onScroll)
  })

  const getStopTimes = async (stop: Stop) => {
    try {
      stopTimes = await getStopStopTimes(stop.stopId, "initial", getSydneyNow())
      activeStop = stop

      await tick()
      if (listElement) listElement.scrollTop = BUFFER_PX
    } catch (err) {
      console.error(err)
    }
  }
</script>

<div class="pl-150 pt-50">
  <p>Stops</p>
  <input
    bind:value={stopQuery}
    placeholder="Search stops..."
  />
  {#each Object.entries(getStops()) as [mode, modeStops]}
    <p class="font-bold">{ModeType[Number(mode)]}</p>
    <div class="flex flex-col items-start">
      {#each modeStops.filter((stop) => stop.stopName.toLowerCase().includes(stopQuery.toLowerCase())).slice(0, 20) as stop}
        <button class="cursor-pointer" onclick={() => getStopTimes(stop)}>{stop.stopName}</button>
      {/each}
    </div>
  {/each}
</div>

{#if activeStop}
  <div class="absolute top-4 left-4 bg-white w-md h-[calc(100vh-2rem)] flex flex-col p-8 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)]">
    <div class="sticky top-0 z-10 bg-white">
      <p class="text-2xl font-bold">{activeStop.stopName}</p>
      <p class="text-xs font-light">{activeStop.stopId}</p>
    </div>
    <div bind:this={listElement} /*style:scrollbar-width="none"*/ class="flex flex-col overflow-y-scroll">
    <!-- <div bind:this={listElement} style:scrollbar-width="none" class="flex flex-col overflow-y-scroll"> -->
      <div style:height={`${BUFFER_PX}px`} class="w-full shrink-0"></div>
        {#each stopTimes as stopTime}
          <div class="flex flex-row justify-between items-center">
            <div>
              <p>{stopTime.tripHeadsign}</p>
            </div>
            <div class="flex flex-col justify-center w-26">
              <p class="text-center">{secondsToTime(stopTime.effectiveDepartureTime)}</p>
              {#if stopTime.isRealtime}
                <p style:color={stopDelayColour(stopTime.departureDelay)} class="text-sm text-center">{stopDelayText(stopTime.departureDelay)}</p>
              {:else}
                <p class="text-sm text-center">No realtime data</p>
              {/if}
              </div>
          </div>
        {/each}
      <div style:height={`${BUFFER_PX}px`} class="w-full shrink-0"></div>
    </div>
  </div>
{/if}