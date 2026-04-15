<script lang="ts">
  import { getSydneyRealtimeStopTimes, getSydneyScheduledStopTimes, getSydneyScheduledTrip } from "$lib/api/sydney";
  import { routesMap } from "$lib/constants";
  import { coloursMap } from "$lib/types/constants";
  import type { RealtimeStopTime, StaticStopTime } from "$lib/types/stopTime";
  import type { Trip } from "$lib/types/trip";
  import SidebarHeader from "./sidebar/SidebarHeader.svelte";
  import SidebarListings from "./sidebar/SidebarListings.svelte";

  let { selectedFeature } = $props()

  let currentFeature: any = $state(null)
  let loaded: boolean = $state(false)

  let scheduledTrip: Trip | null = $state(null)
  let scheduledStopTimes: StaticStopTime[] = $state([])
  let realtimeStopTimes: RealtimeStopTime[] = $state([])

  let route: string = $state('')
  let colour: string = $state('')

  $effect(() => {
    if (!selectedFeature?.id) return
    
    const feature = selectedFeature
    currentFeature = feature

    let interval: ReturnType<typeof setInterval>

    (async () => {
      if (feature.type === 'vehicle') {
        scheduledTrip = await getSydneyScheduledTrip(fetch, feature.tripId)
        if (scheduledTrip === null) return

        route = routesMap[scheduledTrip.routeId.split('_')[0]]
        colour = coloursMap[route]
        scheduledStopTimes = await getSydneyScheduledStopTimes(fetch, null, feature.tripId)
        realtimeStopTimes = await getSydneyRealtimeStopTimes(fetch, null, feature.tripId)
        console.log(realtimeStopTimes)
      }

      loaded = true

      const fetchStopTimes = async () => {
        console.log("Updating stop times...")
        realtimeStopTimes = await getSydneyRealtimeStopTimes(fetch, null, feature.tripId)
        console.log(realtimeStopTimes)
      }

      interval = setInterval(fetchStopTimes, 15000);
    })()

    return () => {
      if (interval) clearInterval(interval)
    }
  })
</script>

{#if currentFeature && loaded}
  <div class="absolute top-4 left-4 bg-white w-md h-[calc(100vh-2rem)] flex flex-col rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)] overflow-hidden">
    {#if scheduledTrip}
      <SidebarHeader
        tripHeadsign={scheduledStopTimes[0].stopHeadsign}
        route={route}
        colour={colour}
        onClose={() => {
          selectedFeature = null
          currentFeature = null
          loaded = false
          scheduledTrip = null
          scheduledStopTimes = []
          realtimeStopTimes = []
        }}/>

      <SidebarListings colour={colour} realtimeStopTimes={realtimeStopTimes} />
    {/if}
  </div>
{/if}
