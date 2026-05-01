<script lang="ts">
  import { onMount, tick } from "svelte";
  import type { PageData } from "./$types";
  import type { Stop } from "$lib/types/stops.types";
  import { getStops, getVehicles, setStops, setVehicles } from "$lib/stores.svelte";
  import { getStopStopTimes, getVehicleStopTimes } from "$lib/api/stoptimes.api";
  import { BASE_URL, LineColours, LineRoutes, ModeLabels } from "$lib/constants";
  import type { StopStopTime } from "$lib/types/stoptimes.types";
  import { getSydneyNow, secondsToTime, stopDelayColour, stopDelayText } from "$lib/helpers";
  import type { Vehicle } from "$lib/types/vehicles.types";
  
  let { data }: { data: PageData } = $props()

  const BUFFER_PX = 32

  let refreshInterval: ReturnType<typeof setInterval> | null = null
  let vehicleRefreshInterval: ReturnType<typeof setInterval> | null = null

  let stopQuery = $state<string>('')
  let activeStop = $state<Stop | null>(null)
  let activeVehicle = $state<Vehicle | null>(null)

  let stopTimes = $state<StopStopTime[]>([])
  let listElement = $state<HTMLElement | null>(null)
  let sidebarElement = $state<HTMLElement | null>(null)
  let fetching = $state<boolean>(false)
  let disableRefresh = $state<boolean>(false)

  onMount(() => {
    const savedStopQuery = localStorage.getItem('stopQuery')
    if (savedStopQuery !== null) stopQuery = savedStopQuery
    setStops(data.stops)
    setVehicles(data.vehicles)

    console.log("Initial stops: ", $state.snapshot(data.stops))
    console.log("Initial vehicles: ", $state.snapshot(data.vehicles))

    vehicleRefreshInterval = setInterval(async () => {
      const vehicleRes = await fetch(`${BASE_URL}/api/sydney/vehicles`)
      const vehicles = await vehicleRes.json()
      setVehicles(vehicles)
      console.log("Refreshed vehicles: ", $state.snapshot(vehicles))
    }, 10000)

    return () => {
      if (refreshInterval) clearInterval(refreshInterval)
      if (vehicleRefreshInterval) clearInterval(vehicleRefreshInterval)
    }
  })

  $effect(() => {
    localStorage.setItem('stopQuery', stopQuery)
  })

  $effect(() => {
    if (!listElement) return
    if (listElement.scrollTop === 0) listElement.scrollTop = BUFFER_PX
    const list = listElement

    // surely can do something more cleaner when there are less than 20 stop times
    const onScroll = async () => {
      console.log(list.scrollTop)
      if (fetching || stopTimes.length === 0) return
      if (list.scrollTop === 0) {
        fetching = true
        try {
          const newTimes = await getStopStopTimes(activeStop!.stopId, "prev", stopTimes[0].displayTime)
          if (newTimes.length === 0) return
          stopTimes = [...newTimes, ...stopTimes]

          await tick()
          let additions = newTimes.filter((stopTime) => stopTime.stopType === 'pass' || stopTime.stopType === 'terminate').length * 24
          list.scrollTop = newTimes.length * 60 + newTimes.length + additions
        } catch (error) {
          console.error(error)
        } finally {
          fetching = false
          disableRefresh = true
        }
      } else if (list.scrollTop + list.clientHeight === list.scrollHeight) {
        fetching = true
        try {
          const newTimes = await getStopStopTimes(activeStop!.stopId, "next", stopTimes[stopTimes.length - 1].displayTime)
          if (newTimes.length === 0) return
          stopTimes = [...stopTimes, ...newTimes]
        } catch (error) {
          console.error(error)
        } finally {
          fetching = false
          disableRefresh = true
        }
      }
    }

    console.log("New stop times: ", $state.snapshot(stopTimes))

    list.addEventListener('scroll', onScroll)
    return () => list.removeEventListener('scroll', onScroll)
  })

  // todo: add refreshing when scrolled
  // todo: for regional trains, remove duplicated entries on sydney trains
  const stopStopTimes = async (stop: Stop) => {
    try {
      stopTimes = await getStopStopTimes(stop.stopId, "initial", getSydneyNow())
      console.log("Stop times: ", $state.snapshot(stopTimes))
      activeStop = stop
      await tick()

      if (refreshInterval) clearInterval(refreshInterval)

      refreshInterval = setInterval(async () => {
        if (!listElement || disableRefresh) return

        stopTimes = await getStopStopTimes(stop.stopId, "initial", getSydneyNow())
        console.log("Refreshed stop times: ", $state.snapshot(stopTimes))
      }, 10000)
    } catch (err) {
      console.error(err)
    }
  }

  const vehicleStopTimes = async (vehicle: Vehicle) => {
    console.log(vehicle)
  }
</script>

<svelte:window onclick={(e: MouseEvent) => {
  if (!activeStop && !activeVehicle) return
  if (sidebarElement && !sidebarElement.contains(e.target as Node)) {
    activeStop = null
    activeVehicle = null
    if (refreshInterval) clearInterval(refreshInterval)
  }
}}/>

<div class="absolute top-4 h-[calc(100vh-1rem)] overflow-y-scroll">
  <div class="pl-150">
    <p>Stops</p>
    <input
      bind:value={stopQuery}
      placeholder="Search stops..."
    />
    {#each Object.entries(getStops()) as [mode, modeStops]}
      <p class="font-bold">{mode} - {ModeLabels[Number(mode)]}</p>
      <div class="flex flex-col items-start">
        {#each modeStops.filter((stop) => stop.stopName.toLowerCase().includes(stopQuery.toLowerCase())).slice(0, 20) as stop}
          <button class="cursor-pointer" onclick={() => stopStopTimes(stop)}>{stop.stopName} - {stop.stopId}</button>
        {/each}
      </div>
    {/each}
  </div>

  <div class="pl-150 pt-50">
    <p>Vehicles</p>
    {#each Object.entries(getVehicles()) as [mode, modeVehicles]}
      <p class="font-bold">{mode} - {ModeLabels[Number(mode)]}</p>
      <div class="flex flex-col items-start">
        {#each modeVehicles as vehicle}
          <button class="cursor-pointer" onclick={() => vehicleStopTimes(vehicle)}>{vehicle.vehicleModel} - {vehicle.vehicleId}</button>
        {/each}
      </div>
    {/each}
  </div>
</div>

{#if activeStop}
  <div bind:this={sidebarElement} class="absolute top-4 left-4 bg-white w-md h-[calc(100vh-2rem)] flex flex-col p-8 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)]">
    <div class="sticky top-0 z-10 bg-white">
      <p class="text-xl font-bold">{activeStop.stopName}</p>
      <p class="text-xs font-light">{activeStop.stopId}</p>
    </div>

    <div bind:this={listElement} style:scrollbar-width="none" class="flex flex-col overflow-y-scroll">
      <div class="min-h-screen">
        <div style:height={`${BUFFER_PX}px`} class="w-full shrink-0"></div>
          {#each stopTimes as stopTime, index}
            <div style:opacity={stopTime.stopType === 'pass' ? 0.5 : 1} class="flex flex-row justify-between items-center py-2">
              <div class="flex flex-row items-center gap-4">
                {#if stopTime.routeShortName}
                  <div style:background-color={`#${stopTime.routeColour}`} class="w-12 h-6 flex flex-row justify-center items-center rounded-md">
                    <p class="text-white font-bold">{stopTime.routeShortName}</p>
                  </div>
                {:else}
                  <div class="w-12 h-6 bg-[#888] flex flex-row justify-center items-center rounded-md">
                    <p class="text-white font-bold">NR</p>
                  </div>
                {/if}
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
  </div>
{:else if activeVehicle}
  <div bind:this={sidebarElement} class="absolute top-4 left-4 bg-white w-md h-[calc(100vh-2rem)] flex flex-col p-8 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)]">
    <div class="sticky top-0 z-10 bg-white">
      <p class="text-2xl font-bold">{activeVehicle.vehicleModel}</p>
      <p class="text-xs font-light">{activeVehicle.vehicleId}</p>
    </div>

    <div bind:this={listElement} style:scrollbar-width="none" class="flex flex-col overflow-y-scroll">
      <div class="min-h-screen">
        <div style:height={`${BUFFER_PX}px`} class="w-full shrink-0"></div>
          {#each stopTimes as stopTime, index}
            <div style:opacity={stopTime.stopType === 'pass' ? 0.5 : 1} class="flex flex-row justify-between items-center py-2">
              <div class="flex flex-row items-center gap-4">
                <div style:background-color={stopTime.routeType === 401 ? LineColours[stopTime.routeId.split('_')[1]] : LineColours[stopTime.routeId.split('_')[0]]} class="w-12 h-6 flex flex-row justify-center items-center rounded-md">
                  <p class="text-white font-bold">
                    {stopTime.routeType === 401 ? LineRoutes[stopTime.routeId.split('_')[1]] : LineRoutes[stopTime.routeId.split('_')[0]]}
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
  </div>
{/if}
