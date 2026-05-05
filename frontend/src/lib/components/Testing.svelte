<script lang="ts">
  import { ModeLabels } from "$lib/constants";
  import { getStops, getVehicles } from "$lib/stores.svelte";
  import { onMount } from "svelte";

  const { type, setStopTimes } = $props()

  let stopQuery = $state<string>('')
  let vehicleQuery = $state<string>('')

  onMount(() => {
    if (type === 'stops') {
      const savedStopQuery = localStorage.getItem('stopQuery')
      if (savedStopQuery !== null) stopQuery = savedStopQuery
    } else if (type === 'vehicles') {
      const savedVehicleQuery = localStorage.getItem('vehicleQuery')
      if (savedVehicleQuery !== null) vehicleQuery = savedVehicleQuery
    }
  })

  $effect(() => {
    localStorage.setItem('stopQuery', stopQuery)
  })

  $effect(() => {
    localStorage.setItem('vehicleQuery', vehicleQuery)
  })
</script>

<div class="h-[calc(100vh-1rem)] my-20 overflow-y-scroll">
  {#if type === 'stops'}
    <p>{type}</p>
    <input
      bind:value={stopQuery}
      placeholder={`Search ${type}...`}
    />
    {#each Object.entries(getStops()) as [mode, modeStops]}
      <p class="font-bold">{mode} - {ModeLabels[Number(mode)]}</p>
      <div class="flex flex-col items-start">
        {#each modeStops.filter((stop) => stop.stopName.toLowerCase().includes(stopQuery.toLowerCase())).slice(0, 20) as stop}
          <button class="cursor-pointer" onclick={() => setStopTimes(stop)}>{stop.stopName} - {stop.stopId}</button>
        {/each}
      </div>
    {/each}
  {:else if type === 'vehicles'}
    <p>{type}</p>
    <input
      bind:value={vehicleQuery}
      placeholder={`Search ${type}...`}
    />
    {#each Object.entries(getVehicles()) as [mode, modeVehicles]}
      <p class="font-bold">{mode} - {ModeLabels[Number(mode)]}</p>
      <div class="flex flex-col items-start">
        {#each modeVehicles.filter((vehicle) => vehicle.vehicleId.toLowerCase().includes(stopQuery.toLowerCase())).slice(0, 20) as vehicle}
          <button class="cursor-pointer" onclick={() => setStopTimes(vehicle)}>{vehicle.vehicleModel} - {vehicle.vehicleId}</button>
        {/each}
      </div>
    {/each}
  {/if}
</div>