<script lang="ts">
  import { stopsApi, stopTimesApi, vehiclesApi } from "$lib/api/client.api.js";
  import StopSidebarBodyV2 from "$lib/components/Sidebar/StopSidebarBodyV2.svelte";
  import StopSidebarHeaderV2 from "$lib/components/Sidebar/StopSidebarHeaderV2.svelte";
  import VehicleSidebarBodyV2 from "$lib/components/Sidebar/VehicleSidebarBodyV2.svelte";
  import VehicleSidebarHeaderV2 from "$lib/components/Sidebar/VehicleSidebarHeaderV2.svelte";
  import { ModeLabels } from "$lib/constants";
  import { getSydneyNow, secondsToTime, stopDelayColour, stopDelayText } from "$lib/helpers";
  import { transportDataStore } from "$lib/stores.svelte";
  import type { Stop } from "$lib/types/stops.types";
  import type { StopStopTime, VehicleStopTime } from "$lib/types/stoptimes.types";
  import type { Vehicle } from "$lib/types/vehicles.types";
  import { onMount, tick } from "svelte";

  const BUFFER_PX = 32

  let { data } = $props()

  let stopQuery = $state<string>('')
  let vehicleQuery = $state<string>('')

  let activeItem = $state<Stop | Vehicle | null>(null)
  let activeStopTimes = $state<StopStopTime[] | VehicleStopTime[]>([])

  let listElement = $state<HTMLElement | null>(null)
  let sidebarElement = $state<HTMLElement | null>(null)
  let fetching = $state<boolean>(false)
  let disableRefresh = $state<boolean>(false)

  onMount(() => {
    // local storage for stop and vehicle queries
    const savedStopQuery = localStorage.getItem('stopQuery')
    if (savedStopQuery !== null) stopQuery = savedStopQuery

    const savedVehicleQuery = localStorage.getItem('vehicleQuery')
    if (savedVehicleQuery !== null) vehicleQuery = savedVehicleQuery

    // set initial data
    transportDataStore.stops = data.stops
    transportDataStore.vehicles = data.vehicles

    console.log("Stops: ", $state.snapshot(transportDataStore.stops))
    console.log("Initial vehicles: ", $state.snapshot(transportDataStore.vehicles))

    // refresh data
    const interval = setInterval(async () => {
      transportDataStore.vehicles = await vehiclesApi.getAll()
      console.log("Refreshed vehicles: ", $state.snapshot(transportDataStore.vehicles))

      if (activeItem && isStop(activeItem)) {
        activeStopTimes = await stopTimesApi.getForStop(activeItem.stopId, "initial", getSydneyNow())
        console.log("Refreshed stop stop times: ", $state.snapshot(activeStopTimes))
      } else if (activeItem && isVehicle(activeItem)) {
        activeItem = await vehiclesApi.getById(activeItem.vehicleId)
        activeStopTimes = await stopTimesApi.getForVehicle(activeItem.vehicleId, activeItem.positionLongitude, activeItem.positionLatitude)
        console.log("Refreshed vehicle info: ", $state.snapshot(activeItem))
        console.log("Refreshed vehicle stop times: ", $state.snapshot(activeStopTimes))
      }
      
    }, 10000)

    return () => clearInterval(interval)
  })

  $effect(() => {
    localStorage.setItem('stopQuery', stopQuery)
  })

  $effect(() => {
    localStorage.setItem('vehicleQuery', vehicleQuery)
  })

  $effect(() => {
    if (!activeItem) return
    if (!listElement) return
    if (listElement.scrollTop === 0) listElement.scrollTop = BUFFER_PX
    const list = listElement

    // surely can do something more cleaner when there are less than 20 stop times
    const onScroll = async () => {
      if (!activeItem) return
      if (!isStop(activeItem)) return
      if (fetching || activeStopTimes.length === 0) return
      activeStopTimes = activeStopTimes as StopStopTime[]

      const atTop = list.scrollTop === 0
      const atBottom = Math.abs(list.scrollTop + list.clientHeight - list.scrollHeight) <= 1 / window.devicePixelRatio

      if (atTop) {
        fetching = true
        try {
          const newTimes = await stopTimesApi.getForStop(activeItem.stopId, "prev", activeStopTimes[0].displayTime)
          if (newTimes.length === 0) return
          activeStopTimes = [...newTimes, ...activeStopTimes]

          await tick()
          let additions = newTimes.filter((stopTime) => stopTime.stopType === 'pass' || stopTime.stopType === 'terminate' || stopTime.stopType === 'continues').length * 24
          let viaAdditions = newTimes.filter((stopTime) => stopTime.tripHeadsign.includes('via')).length * 80
          let nonViaAdditions = newTimes.filter((stopTime) => !stopTime.tripHeadsign.includes('via')).length * 60
          list.scrollTop = viaAdditions + nonViaAdditions + newTimes.length + additions
        } catch (error) {
          console.error(error)
        } finally {
          fetching = false
          disableRefresh = true
        }
      } else if (atBottom) {
        fetching = true
        try {
          const newTimes = await stopTimesApi.getForStop(activeItem.stopId, "next", activeStopTimes[activeStopTimes.length - 1].displayTime)
          if (newTimes.length === 0) return
          activeStopTimes = [...activeStopTimes, ...newTimes]
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

  const getStopInfo = async (stopId: string) => {
    try {
      activeItem = await stopsApi.getById(stopId)
      activeStopTimes = await stopTimesApi.getForStop(stopId, "initial", getSydneyNow())
      
      console.log("Stop times: ", $state.snapshot(activeStopTimes))
      console.log("Active stop: ", $state.snapshot(activeItem))
    } catch (err) {
      console.error(err)
    }
  }

  const getVehicleInfoByTrip = async (tripId: string) => {
    try {
      activeItem = await vehiclesApi.getByTrip(tripId)
      activeStopTimes = await stopTimesApi.getForVehicle(activeItem.vehicleId, activeItem.positionLongitude, activeItem.positionLatitude)
      
      console.log("Stop times: ", $state.snapshot(activeStopTimes))
      console.log("Active vehicle: ", $state.snapshot(activeItem))
    } catch (err) {
      console.error(err)
    }
  }

  const getVehicleInfoByVehicle = async (vehicleId: string) => {
    try {
      activeItem = await vehiclesApi.getById(vehicleId)
      activeStopTimes = await stopTimesApi.getForVehicle(activeItem.vehicleId, activeItem.positionLongitude, activeItem.positionLatitude)
      
      console.log("Stop times: ", $state.snapshot(activeStopTimes))
      console.log("Active vehicle: ", $state.snapshot(activeItem))
    } catch (err) {
      console.error(err)
    }
  }

  const isStop = (item: Stop | Vehicle): item is Stop => {
    return "stopId" in item
  }

  const isVehicle = (item: Stop | Vehicle): item is Vehicle => {
    return "vehicleId" in item
  }

  const isStopStopTime = (stopTimes: StopStopTime[] | VehicleStopTime[]): stopTimes is StopStopTime[] => {
    return stopTimes.length === 0 || stopTimes.length > 0 && !("progress" in stopTimes[0]);
  }

  const isVehicleStopTime = (stopTimes: StopStopTime[] | VehicleStopTime[]): stopTimes is VehicleStopTime[] => {
    return stopTimes.length === 0 || stopTimes.length > 0 && "progress" in stopTimes[0];
  }

</script>

<svelte:window onclick={(e: MouseEvent) => {
  if (!activeItem) return

  const path = e.composedPath();
  if (sidebarElement && !path.includes(sidebarElement)) {
    activeItem = null;
  }
}}/>

<div class="pl-120">
  <h1 class="font-2xl font-bold">Transport Data</h1>
  <div class="flex flex-row">
    <!-- STOPS -->
    <div>
      <p class="font-bold">Stops</p>
      <input
        bind:value={stopQuery}
        placeholder={`Search stops...`}
      />
      {#each Object.entries($state.snapshot(transportDataStore.stops)) as [mode, modeStops]}
        <p class="font-bold">{mode} - {ModeLabels[Number(mode)]}</p>
        <div class="flex flex-col items-start">
          {#each modeStops.filter((stop) => stop.stopName.toLowerCase().includes(stopQuery.toLowerCase())).slice(0, 20) as stop}
            <button class="cursor-pointer" onclick={() => getStopInfo(stop.stopId)}>{stop.stopName} - {stop.stopId}</button>
          {/each}
        </div>
      {/each}
    </div>

    <!-- VEHICLES -->
    <div>
      <p class="font-bold">Vehicles</p>
      <input
        bind:value={vehicleQuery}
        placeholder={`Search vehicles...`}
      />
      {#each Object.entries($state.snapshot(transportDataStore.vehicles)) as [mode, modeVehicles]}
        <p class="font-bold">{mode} - {ModeLabels[Number(mode)]}</p>
        <div class="flex flex-col items-start">
          {#each modeVehicles.filter((vehicle) => vehicle.vehicleId.toLowerCase().includes(vehicleQuery.toLowerCase())).slice(0, 20) as vehicle}
            <button class="cursor-pointer" onclick={() => getVehicleInfoByVehicle(vehicle.vehicleId)}>{vehicle.vehicleModel} - {vehicle.vehicleId}</button>
          {/each}
        </div>
      {/each}
    </div>
  </div>

  <!-- SIDEBAR -->
  {#if activeItem && isStop(activeItem) && isStopStopTime(activeStopTimes)}
    <div bind:this={sidebarElement} class="absolute top-4 left-4 bg-white w-md h-[calc(100vh-2rem)] flex flex-col p-8 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)]">
      <StopSidebarHeaderV2 title={activeItem.stopName} id={activeItem.stopId} />
      <StopSidebarBodyV2 bind:listElement activeStop={activeItem} stopTimes={activeStopTimes} getVehicleInfo={getVehicleInfoByTrip}/>
    </div>
  {:else if activeItem && isVehicle(activeItem) && isVehicleStopTime(activeStopTimes)}
    <div bind:this={sidebarElement} class="absolute top-4 left-4 bg-white w-md h-[calc(100vh-2rem)] flex flex-col p-8 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)]">
      <VehicleSidebarHeaderV2 title={activeStopTimes.slice().reverse().find((stopTime) => stopTime.progress === "passed")?.tripHeadsign} id={activeItem.vehicleId} routeShortName={activeStopTimes.slice().reverse().find((stopTime) => stopTime.progress === "passed")?.routeShortName} routeColour={activeStopTimes.slice().reverse().find((stopTime) => stopTime.progress === "passed")?.routeColour} />
      <VehicleSidebarBodyV2 stopTimes={activeStopTimes} getStopInfo={getStopInfo} />
    </div>
  {/if}
</div>