<script lang="ts">
  import { onMount, tick } from "svelte";
  import type { PageData } from "./$types";
  import type { Stop } from "$lib/types/stops.types";
  import { setStops, setVehicles } from "$lib/stores.svelte";
  import { getStopStopTimes, getTripStopTimes, getVehicle } from "$lib/api/stoptimes.api";
  import { BASE_URL } from "$lib/constants";
  import type { StopStopTime, VehicleStopTime } from "$lib/types/stoptimes.types";
  import { getSydneyNow } from "$lib/helpers";
  import type { Vehicle } from "$lib/types/vehicles.types";
  import Testing from "$lib/components/Testing.svelte";
  import StopSidebarBody from "$lib/components/Sidebar/StopSidebarBody.svelte";
  import VehicleSidebarBody from "$lib/components/Sidebar/VehicleSidebarBody.svelte";
  import StopSidebarHeader from "$lib/components/Sidebar/StopSidebarHeader.svelte";
  import VehicleSidebarHeader from "$lib/components/Sidebar/VehicleSidebarHeader.svelte";

  let { data }: { data: PageData } = $props()

  const BUFFER_PX = 32

  let refreshInterval: ReturnType<typeof setInterval> | null = null
  let vehicleRefreshInterval: ReturnType<typeof setInterval> | null = null

  let activeStop = $state<Stop | null>(null)
  let activeVehicle = $state<Vehicle | null>(null)

  let stopTimes = $state<StopStopTime[] | VehicleStopTime[]>([])

  let listElement = $state<HTMLElement | null>(null)
  let sidebarElement = $state<HTMLElement | null>(null)
  let fetching = $state<boolean>(false)
  let disableRefresh = $state<boolean>(false)

  onMount(() => {
    setStops(data.stops)
    setVehicles(data.vehicles)

    console.log("Initial stops: ", $state.snapshot(data.stops))
    console.log("Initial vehicles: ", $state.snapshot(data.vehicles))

    const interval = setInterval(async () => {
      const vehicleRes = await fetch(`${BASE_URL}/api/sydney/vehicles`)
      const vehicles = await vehicleRes.json()
      setVehicles(vehicles)
      console.log("Refreshed vehicles: ", $state.snapshot(vehicles))

      if (!listElement || disableRefresh) return

      if (activeStop) {
        stopTimes = await getStopStopTimes(activeStop.stopId, "initial", getSydneyNow())
        console.log("Refreshed stop stop times: ", $state.snapshot(stopTimes))
      } else if (activeVehicle) {
        activeVehicle = await getVehicle(activeVehicle.vehicleId)
        stopTimes = await getTripStopTimes(activeVehicle.tripId, activeVehicle.positionLongitude, activeVehicle.positionLatitude)
        console.log("Refreshed vehicle info: ", $state.snapshot(activeVehicle))
        console.log("Refreshed vehicle stop times: ", $state.snapshot(stopTimes))
      }
    }, 10000)

    return () => clearInterval(interval)
  })

  $effect(() => {
    if (!activeStop) return
    if (!listElement) return
    if (listElement.scrollTop === 0) listElement.scrollTop = BUFFER_PX
    const list = listElement

    // surely can do something more cleaner when there are less than 20 stop times
    const onScroll = async () => {
      const atTop = list.scrollTop === 0
      const atBottom = Math.abs(list.scrollTop + list.clientHeight - list.scrollHeight) <= 1 / window.devicePixelRatio

      let currentStopTimes = $state.snapshot(stopTimes) as StopStopTime[]

      if (fetching || stopTimes.length === 0) return


      if (atTop) {
        fetching = true
        try {
          const newTimes = await getStopStopTimes(activeStop!.stopId, "prev", currentStopTimes[0].displayTime)
          if (newTimes.length === 0) return
          currentStopTimes = [...newTimes, ...currentStopTimes]

          await tick()
          let additions = newTimes.filter((stopTime) => stopTime.stopType === 'pass' || stopTime.stopType === 'terminate').length * 24
          list.scrollTop = newTimes.length * 60 + newTimes.length + additions
        } catch (error) {
          console.error(error)
        } finally {
          fetching = false
          disableRefresh = true
        }
      } else if (atBottom) {
        fetching = true
        try {
          const newTimes = await getStopStopTimes(activeStop!.stopId, "next", currentStopTimes[currentStopTimes.length - 1].displayTime)
          if (newTimes.length === 0) return
          currentStopTimes = [...currentStopTimes, ...newTimes]
        } catch (error) {
          console.error(error)
        } finally {
          fetching = false
          disableRefresh = true
        }
      }
    }

    list.addEventListener('scroll', onScroll)
    return () => list.removeEventListener('scroll', onScroll)
  })

  // todo: add refreshing when scrolled
  // todo: for regional trains, remove duplicated entries on sydney trains
  const stopStopTimes = async (stop: Stop) => {
    try {
      stopTimes = await getStopStopTimes(stop.stopId, "initial", getSydneyNow())
      activeStop = stop
      
      console.log("Stop times: ", $state.snapshot(stopTimes))
      console.log("Active stop: ", $state.snapshot(activeStop))
    } catch (err) {
      console.error(err)
    }
  }

  const vehicleStopTimes = async (vehicle: Vehicle) => {
    try {
      stopTimes = await getTripStopTimes(vehicle.tripId, vehicle.positionLongitude, vehicle.positionLatitude)
      activeVehicle = vehicle

      console.log("Vehicle info: ", $state.snapshot(activeVehicle))
      console.log("Vehicle stop times: ", $state.snapshot(stopTimes))
    } catch (err) {
      console.error(err)
    }
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

<div class="flex flex-row gap-20 pl-120">
  <Testing
    type={"stops"}
    setStopTimes={stopStopTimes}
  />
  <Testing
    type={"vehicles"}
    setStopTimes={vehicleStopTimes}
  />
</div>

{#if activeStop}
  <div bind:this={sidebarElement} class="absolute top-4 left-4 bg-white w-md h-[calc(100vh-2rem)] flex flex-col p-8 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)]">
    <StopSidebarHeader title={activeStop.stopName} id={activeStop.stopId} />
    <StopSidebarBody bind:listElement stopTimes={stopTimes as StopStopTime[]} />
  </div>
{:else if activeVehicle}
  <div bind:this={sidebarElement} class="absolute top-4 left-4 bg-white w-md h-[calc(100vh-2rem)] flex flex-col p-8 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)]">
    <VehicleSidebarHeader stopTime={stopTimes[0] as VehicleStopTime} activeVehicle={activeVehicle} />
    <VehicleSidebarBody bind:listElement stopTimes={stopTimes as VehicleStopTime[]} />
  </div>
{/if}
