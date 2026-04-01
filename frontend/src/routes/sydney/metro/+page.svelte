<script lang="ts">
  import Map from '$lib/components/map/Map.svelte';
  import { MODE_TYPE_METRO } from '$lib/constants';
  import { addMode, lineShapes, shapes, stops, vehicles } from '$lib/stores';
  import type { PageProps } from './$types';

  let { data }: PageProps = $props()

  let loaded = $state(false)

  $effect(() => {
    const metroLines = Object.keys(data.shapes).reduce<Record<string, string[]>>((acc, shapeId) => {
      if (!acc['M1']) acc['M1'] = []
      acc['M1'].push(shapeId)
      return acc
    }, {})
  
    shapes.set(data.shapes)
    lineShapes.set(metroLines)
    stops.set(data.stops)
    vehicles.set(data.vehicles)
    
    addMode(MODE_TYPE_METRO, 'M1')

    loaded = true
  })
</script>

{#if loaded}
  <Map />
{/if}