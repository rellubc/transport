<script lang="ts">
  import { onMount, tick } from "svelte";
  import type { PageData } from "./$types";
  import type { Stop } from "$lib/types/stops.types";
  import { BASE_URL } from "$lib/constants";
  import type { StopStopTime, VehicleStopTime } from "$lib/types/stoptimes.types";
  import { getSydneyNow } from "$lib/helpers";
  import type { Vehicle } from "$lib/types/vehicles.types";
  import Testing from "$lib/components/Testing.svelte";
  import StopSidebarBody from "$lib/components/Sidebar/StopSidebarBody.svelte";
  import VehicleSidebarBody from "$lib/components/Sidebar/VehicleSidebarBody.svelte";
  import StopSidebarHeader from "$lib/components/Sidebar/StopSidebarHeader.svelte";
  import VehicleSidebarHeader from "$lib/components/Sidebar/VehicleSidebarHeader.svelte";
  import Map from '$lib/components/Map/Map.svelte';
  import { stopTimesApi } from "$lib/api/stoptimes";
  import { vehiclesApi } from "$lib/api/vehicles";
  import { transportDataStore } from "$lib/stores.svelte";

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
  let loaded = $state<boolean>(false)

  onMount(() => {
    transportDataStore.routeShapes = data.routeShapes
    transportDataStore.displayShapes = data.displayShapes
    transportDataStore.stops = data.stops
    transportDataStore.vehicles = data.vehicles
    Object.keys(transportDataStore.routeShapes).forEach((shapeId) => {
      transportDataStore.modes.add(shapeId.split('_')[1])
    })
    loaded = true

    console.log("Route shapes: ", $state.snapshot(transportDataStore.routeShapes))
    console.log("Display shapes: ", $state.snapshot(transportDataStore.displayShapes))
    console.log("Stops: ", $state.snapshot(transportDataStore.stops))
    console.log("Initial vehicles: ", $state.snapshot(transportDataStore.vehicles))

    const interval = setInterval(async () => {
      transportDataStore.vehicles = await vehiclesApi.getAll()
      console.log("Refreshed vehicles: ", $state.snapshot(transportDataStore.vehicles))
    }, 10000)

    return () => clearInterval(interval)
  })

  // todo: add refreshing when scrolled
  // todo: for regional trains, remove duplicated entries on sydney trains
  const stopStopTimes = async (stop: Stop) => {
    try {
      stopTimes = await stopTimesApi.getForStop(stop.stopId, "initial", getSydneyNow())
      activeStop = stop
      
      console.log("Stop times: ", $state.snapshot(stopTimes))
      console.log("Active stop: ", $state.snapshot(activeStop))
    } catch (err) {
      console.error(err)
    }
  }

  const vehicleStopTimes = async (vehicle: Vehicle) => {
    try {
      stopTimes = await stopTimesApi.getForTrip(vehicle.tripId, vehicle.positionLongitude, vehicle.positionLatitude)
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
