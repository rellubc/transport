<script lang="ts">
  import type { PageProps } from './$types';

  let { data }: PageProps = $props()

  const shapes = $derived(data.shapes)
  const stops = $derived(data.stops)
  const vehicles = $derived(data.vehicles)
</script>

<div class="main">
  <h2>Shapes</h2>

  <h2>Stops</h2>
  {#each stops as stop, index}
    <p>{index + 1}: {stop.name}</p>
  {/each}
  <h2>Vehicles</h2>
</div>

<style>
  .main {
    width: 1200px;
    margin: auto;
  }
</style>