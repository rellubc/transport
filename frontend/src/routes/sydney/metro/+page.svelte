<script lang="ts">
  import Map from '$lib/components/map/Map.svelte';
  import { MODE_TYPE_METRO } from '$lib/constants';
  import { addMode, shapes, stops, vehicles } from '$lib/stores';
  import type { ShapeCoord } from '$lib/types/shape';
  import type { PageProps } from './$types';

  let { data }: PageProps = $props()

  let loaded = $state(false)

  $effect(() => {
    const lines: Record<string, ShapeCoord[]> = {}

    // combine when using custom map
    lines['M1_INNER'] = data.shapes['3722']
    lines['M1_OUTER'] = data.shapes['16714']
    
    addMode(MODE_TYPE_METRO, 'M1')
    shapes.set(lines)
    stops.set(data.stops)
    vehicles.set(data.vehicles)

    loaded = true
  })
</script>

{#if loaded}
  <Map />
{/if}