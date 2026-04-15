<script lang="ts">
  import { secondsToTime, stopDelay } from "$lib/helpers";
  import { occupancyColours } from "$lib/types/constants";
  import type { RealtimeStopTime } from "$lib/types/stopTime";

  let { colour, stopTime, last }: { colour: string, stopTime: RealtimeStopTime, last: boolean } = $props()

  const delayTime = $derived(last ? stopTime.arrivalDelay : stopTime.departureDelay)
  const delay = $derived(stopDelay(last ? stopTime.arrivalDelay : stopTime.departureDelay))
  const time = $derived(secondsToTime((last ? stopTime.arrivalTime : stopTime.departureTime) + delayTime))

  const carriageOccupancies = $derived(stopTime.carriageOccupancies)
</script>

<div class='relative px-8 pl-9 pb-8 z-50'>
  <div
    style={`background-color: ${stopTime.status !== 'stop' ? '' : stopTime.progress === 'passed' ? '#d3d3d3' : colour}`}
    class={`absolute ${stopTime.status !== 'stop' ? 'top-1.5' : 'top-3.5'} left-3 w-4 h-4 z-30 rounded-full`}>
  </div>
  <div
    style="background-color: #ffffff" 
    class={`absolute ${stopTime.status !== 'stop' ? 'top-2.5' : 'top-4.5'} left-4 w-2 h-2 z-40 rounded-full`}>
  </div>
  <div class={`${stopTime.status !== 'stop' ? 'opacity-50' : stopTime.progress === 'passed' ? 'opacity-50' : ''}`}>
    <div class="flex flex-row justify-between items-center">
      <div>
        <p class="text-xl">{stopTime.stopName.split(',')[0]}</p>
      </div>
      {#if stopTime.status === 'stop'}
        <div class="flex flex-col items-center">
          <p class="text-base">{time}</p>
          <!-- <p>{delayTime}</p> -->
          <p class="text-sm">{delay}</p>
        </div>
      {/if}
    </div>
    <div class="flex flex-row justify-between items-center">
      <p class="text-base/3">{stopTime.stopName.split(',')[1]}</p>
      <div class="flex flex-row justify-end gap-1">
        {#each carriageOccupancies as occupancy}
          <div
            style={`background-color: ${occupancyColours[occupancy.departureOccupancyStatus]}`}
            class="w-4.5 h-4.5 flex flex-row justify-center items-center rounded-full"
          >
            <p class="text-sm">{occupancy.positionInConsist}</p>  
          </div>
        {/each}
      </div>
    </div>
  </div>
</div>