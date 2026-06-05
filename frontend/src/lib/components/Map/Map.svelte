<script lang="ts">
  import maplibregl from "maplibre-gl";
  import { onMount, tick } from "svelte";
  import { refreshStopStopTimes, refreshVehicleStopTimes, selectionStore, selectStop, selectVehicleByVehicleId } from "$lib/stores/map-selection.store.svelte";
  import { transportDataStore } from "$lib/stores/map-data.store.svelte";
  import { addShapes, addStops, addVehicles, closeSidebar, getStoredView, isStop, isStopStopTime, isVehicle, isVehicleStopTime, loadIcons, saveView, updateVehicleHighlight } from "./map.helper";
  import Search from "./Search.svelte";
  import StopSidebarHeader from "../Sidebar/StopSidebarHeader.svelte";
  import StopSidebarBody from "../Sidebar/StopSidebarBody.svelte";
  import VehicleSidebarHeader from "../Sidebar/VehicleSidebarHeader.svelte";
  import VehicleSidebarBody from "../Sidebar/VehicleSidebarBody.svelte";
  import type { StopStopTime } from "$lib/types/stoptimes.types";
  import { stopTimesApi } from "$lib/api/client.api";
  import type { Vehicle } from "$lib/types/vehicles.types";

  let map!: maplibregl.Map
  let mapContainer: HTMLElement

  const BUFFER_PX = 32

  const selection = selectionStore

  const activeItem = $derived(selection.activeItem)
  const activeStopTimes = $derived(selection.activeStopTimes)
  const loading = $derived(selection.loading)

  let listElement = $state<HTMLElement | null>(null)
  let searchElement = $state<HTMLElement | null>(null)
  let sidebarElement = $state<HTMLElement | null>(null)
  let fetching = $state<boolean>(false)
  let disableRefresh = $state<boolean>(false)

  onMount(() => {
    const { center, zoom } = getStoredView()
    map = new maplibregl.Map({
      container: mapContainer,
      style: `https://api.maptiler.com/maps/019dfbef-34ee-7a66-a158-87b7c0aba3a3/style.json?key=${import.meta.env.VITE_MAPTILER_KEY}#1.0/0.00000/0.00000`,
      center,
      zoom
    })

    map.addControl(new maplibregl.FullscreenControl())
    map.addControl(new maplibregl.NavigationControl())

    map.on('moveend', () => saveView(map))
    map.on('zoomend', () => saveView(map))

    map.on('load', async () => {
      loadIcons(map)
      addShapes(map, $state.snapshot(transportDataStore.displayShapes))
      addStops(map, $state.snapshot(transportDataStore.stops))
      addVehicles(map, $state.snapshot(transportDataStore.vehicles), $state.snapshot(transportDataStore.modes))
    })

    map.on('click', (e: maplibregl.MapMouseEvent & Object) => {
      const features = map.queryRenderedFeatures(e.point)
      if (features.length === 0) return
      const feature = features[0]

      updateVehicleHighlight(map, feature.properties.vehicleId)

      if (feature.properties.type === 'stop') {
        selectStop(feature.properties.stopId)        
      } else if (feature.properties.type === 'vehicle') {
        selectVehicleByVehicleId(feature.properties.vehicleId)
      }
    })

    return () => {
      map?.remove()
    }
  })

  $effect(() => {
    transportDataStore.refreshId

    if (!map || !map.isStyleLoaded()) return
    addVehicles(map, $state.snapshot(transportDataStore.vehicles), $state.snapshot(transportDataStore.modes))

    const selection = $state.snapshot(selectionStore)
    
    if (selection.activeItem && isStop(selection.activeItem) && !disableRefresh) {
      refreshStopStopTimes(selection.activeItem.stopId)
      console.log("Refreshed stop stop times: ", $state.snapshot(selectionStore.activeStopTimes))
    } else if (selection.activeItem && isVehicle(selection.activeItem)) {
      refreshVehicleStopTimes(selection.activeItem.vehicleId)
      console.log("Refreshed vehicle info: ", $state.snapshot(selectionStore.activeItem))
      console.log("Refreshed vehicle stop times: ", $state.snapshot(selectionStore.activeStopTimes))
    }
  })

  $effect(() => {
    selectionStore.activeItem

    if (!map || !map.isStyleLoaded()) return
    if (!selectionStore.activeItem) return
    console.log("activeItem", selectionStore.activeItem)
    updateVehicleHighlight(map, (selectionStore.activeItem as Vehicle).vehicleId)
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
      selectionStore.activeStopTimes = activeStopTimes as StopStopTime[]

      const atTop = list.scrollTop === 0
      const atBottom = Math.abs(list.scrollTop + list.clientHeight - list.scrollHeight) <= 1 / window.devicePixelRatio

      if (atTop) {
        fetching = true
        try {
          const newTimes = await stopTimesApi.getForStop(activeItem.stopId, "prev", activeStopTimes[0].displayTime)
          if (newTimes.length === 0) return
          selectionStore.activeStopTimes = [...newTimes, ...activeStopTimes as StopStopTime[]]

          await tick()
          let additions = newTimes.filter((stopTime) => stopTime.stopType === 'pass' || stopTime.stopType === 'terminate' || stopTime.stopType === 'continues').length * 24
          let platformAdditions = 0
          if (activeItem.stopParentStation) {
            platformAdditions = newTimes.filter((stopTime) => stopTime.stopType === 'stop' || 'depart').length * 60
          } else {
            let viaAdditions = newTimes.filter((stopTime) => stopTime.tripHeadsign.includes('via')).length * 80
            let nonViaAdditions = newTimes.filter((stopTime) => !stopTime.tripHeadsign.includes('via')).length * 60
            platformAdditions = viaAdditions + nonViaAdditions
          }
          list.scrollTop = platformAdditions + newTimes.length + additions
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
          selectionStore.activeStopTimes = [...activeStopTimes as StopStopTime[], ...newTimes]
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
</script>

<svelte:window onclick={async (e: MouseEvent) => {
  const path = e.composedPath();
  if (sidebarElement && !path.includes(sidebarElement)) {
    if (searchElement && !path.includes(searchElement)) {
      closeSidebar()
    }
  }
}}/>

<div class="relative w-screen h-screen">
  <div bind:this={mapContainer} id="map" class="w-full h-full opacity-100"></div>
  <div class="absolute top-4 left-4 flex flex-col">
    <Search bind:searchElement />
    <div class="mt-16">
      {#if activeItem && isStop(activeItem) && isStopStopTime(activeStopTimes) && !loading}
        <div bind:this={sidebarElement} class="bg-white w-md h-[calc(100vh-6rem)] flex flex-col p-8 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)]">
          <StopSidebarHeader title={activeItem.stopName} id={activeItem.stopId} />
          <StopSidebarBody bind:listElement activeStop={activeItem} stopTimes={activeStopTimes} />
        </div>
      {:else if activeItem && isVehicle(activeItem) && isVehicleStopTime(activeStopTimes) && !loading}
        <div bind:this={sidebarElement} class="bg-white w-md h-[calc(100vh-6rem)] flex flex-col p-8 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)]">
          {#if [...activeStopTimes].find((stopTime) => stopTime.progress === "passed")}
            <VehicleSidebarHeader title={[...activeStopTimes].reverse().find((stopTime) => stopTime.progress === "passed")?.tripHeadsign} id={activeItem.vehicleId} routeShortName={[...activeStopTimes].reverse().find((stopTime) => stopTime.progress === "passed")?.routeShortName} routeColour={[...activeStopTimes].reverse().find((stopTime) => stopTime.progress === "passed")?.routeColour} />
          {:else}
            <VehicleSidebarHeader title={activeStopTimes[0].tripHeadsign} id={activeItem.vehicleId} routeShortName={activeStopTimes[0].routeShortName} routeColour={activeStopTimes[0].routeColour} />
          {/if}
          <VehicleSidebarBody stopTimes={activeStopTimes} />
        </div>
      {/if}
    </div>
  </div>
</div>
