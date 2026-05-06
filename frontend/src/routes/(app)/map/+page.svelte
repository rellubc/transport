<script lang="ts">
  import { onMount } from "svelte";
  import type { PageData } from "./$types";
  import { transportDataStore } from "$lib/stores.svelte";
  import Map from '$lib/components/Map/Map.svelte';
  import { vehiclesApi } from "$lib/api/vehicles";

  let { data }: { data: PageData } = $props()

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
</script>

{#if loaded}
  <Map />
{/if}