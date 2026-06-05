<script lang="ts">
  import { onMount } from "svelte";
  import { refreshVehicles, transportDataStore } from "$lib/stores/map-data.store.svelte";
  import Map from '$lib/components/Map/Map.svelte'

  onMount(() => {
    console.log("Initial vehicles: ", $state.snapshot(transportDataStore.vehicles))

    const interval = setInterval(refreshVehicles, 10000)
    return () => clearInterval(interval)
  })

  $effect(() => {
    transportDataStore.refreshId

    console.log("Refreshed vehicles: ", $state.snapshot(transportDataStore.vehicles))
  })
</script>

<Map />