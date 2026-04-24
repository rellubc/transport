<script lang="ts">
  import Map from '$lib/components/map/Map.svelte';
  import { ModeType } from '$lib/constants';
  import { addModes, routes, shapes, stops, vehicles } from '$lib/stores';
  import { onMount } from 'svelte';
  import { getSydneyVehiclePositions } from '$lib/api/sydney';
  import type { PageProps } from './$types';

  let { data }: PageProps = $props()
  let loaded = $state(false)

  onMount(() => {
    shapes.set(data.shapes)
    stops.set(data.stops)
    vehicles.set(data.vehicles)
    routes.set(data.routes)
    
    loaded = true

    const fetchVehicles = async () => {
      console.log("Updating vehicle positions...")
      vehicles.set(await getSydneyVehiclePositions(fetch))
    }

    const newModes: Partial<Record<number, string[]>> = {}
    Object.keys(data.routes).forEach((routeId) => {
      const line = routeId.split('_')[1]

      let modeType: number | null = null
      if (/^T[0-9]$/.test(line)) modeType = ModeType.RAIL
      if (/^M[0-9]$/.test(line)) modeType = ModeType.METRO
      if (/^L[0-9]$/.test(line)) modeType = ModeType.LIGHT_RAIL

      if (modeType !== null) {
        newModes[modeType] = [...(newModes[modeType] ?? []), line]
      }
    })

    addModes(newModes)

    const interval = setInterval(fetchVehicles, 15000)
    return () => clearInterval(interval)
  })
</script>

{#if loaded}
  <Map />
{/if}