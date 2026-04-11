<script lang="ts">
  import Map from '$lib/components/map/Map.svelte';
  import { MODE_TYPE_RAIL } from '$lib/constants';
  import { addMode, shapes, stops, vehicles } from '$lib/stores';
  import { onMount } from 'svelte';
  import type { PageProps } from './$types';
  import { getSydneyVehiclePositions } from '$lib/api/sydney';

  let { data }: PageProps = $props()

  let loaded = $state(false)

  onMount(() => {
    shapes.set(data.shapes)
    stops.set(data.stops)
    vehicles.set(data.vehicles)
    
    loaded = true

    const fetchVehicles = async () => {
      console.log("Updating vehicle positions...", $vehicles)
      vehicles.set(await getSydneyVehiclePositions(fetch))
    }

    Object.keys($shapes).forEach((line) => {
      addMode(MODE_TYPE_RAIL, line.split("_")[0])
    })

    console.log($shapes, $stops, $vehicles)

    const interval = setInterval(fetchVehicles, 15000)

    return () => clearInterval(interval)
  })
</script>

{#if loaded}
  <Map />
{/if}