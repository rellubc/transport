<script lang="ts">
  import Map from '$lib/components/map/Map.svelte';
  import { MODE_TYPE_METRO } from '$lib/constants';
  import { addMode, shapes, stops, vehicles } from '$lib/stores';
  import type { PageProps } from './$types';

  let { data }: PageProps = $props()

  $effect(() => {
    const metroLines = Object.keys(data.shapes).reduce<Record<string, string[]>>((acc, shapeId) => {
      if (!acc['M1']) acc['M1'] = []
      acc['M1'].push(shapeId)
      return acc
    }, {})

    console.log(metroLines)
    shapes.set(data.shapes)
    stops.set(data.stops)
    vehicles.set(data.vehicles)
  })

  addMode(MODE_TYPE_METRO, 'M1')
</script>
