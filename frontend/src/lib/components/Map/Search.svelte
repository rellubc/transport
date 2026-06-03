<script lang="ts">
  import { transportDataStore } from "$lib/stores.svelte";
  import type { Stop } from "$lib/types/stops.types";
  import { Search } from "@lucide/svelte";
  import { onMount } from "svelte";
  import { handlers } from "svelte/legacy";

  let { searchElement = $bindable(), getStopInfo }: { 
    searchElement: HTMLElement | null
    getStopInfo: (stopId: string) => Promise<void>
  } = $props()

  let active = $state<boolean>(false)
  let stopQuery = $state<string>('')
  let vehicleQuery = $state<string>('')

  let stops = $state<Stop[]>([])
  let queriedStops = $state<Stop[]>([])

  $effect(() => {
    localStorage.setItem('stopQuery', stopQuery)
  })

  $effect(() => {
    localStorage.setItem('vehicleQuery', vehicleQuery)
  })

  onMount(() => {
    $state.snapshot(Object.keys(transportDataStore.stops)).forEach((mode) => {
      stops = stops.concat($state.snapshot(transportDataStore.stops[mode]))
    })

    stops.sort((a, b) => a.stopName.localeCompare(b.stopName))
  })

  $effect(() => {
    queriedStops = stops.filter((stop, index, self) => index === self.findIndex((s) => s.stopId === stop.stopId)).filter((stop) => stop.stopName.toLowerCase().includes(stopQuery.toLowerCase()))
  })

  const handleClick = (stopId: string, stopName: string) => {
    getStopInfo(stopId)
    stopQuery = stopName
    active = false
  }

</script>

<svelte:window onclick={(e: MouseEvent) => {
  if (!active) return

  const path = e.composedPath();
  if (searchElement && !path.includes(searchElement)) {
    active = false
  }
}}/>

<div bind:this={searchElement} class="absolute bg-white w-md h-fit flex flex-col p-2 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)] z-40">
  <div class="flex flex-row items-center gap-4 pl-1 pr-3">
    <Search />
    <input
      bind:value={stopQuery}
      onfocus={() => { active = true }}
      class="h-8 w-full outline-none"
      placeholder={`Search stops...`}
    />
  </div>
  {#if active}
    <div class="max-h-[calc(100vh-28rem)] overflow-y-scroll">
      {#each queriedStops.filter((stop) => stop.stopName.toLowerCase().includes(stopQuery.toLowerCase())).slice(0, 20) as stop}
        <div class="flex flex-col items-start">
          <button class="cursor-pointer" onclick={() => handleClick(stop.stopId, stop.stopName)}>{stop.stopName} - {stop.stopId}</button>
        </div>
      {/each}
    </div>
  {/if}
</div>