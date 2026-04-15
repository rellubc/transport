<script lang="ts">
  import type { RealtimeStopTime } from "$lib/types/stopTime";
  import SidebarListing from "./SidebarListing.svelte";

  const { colour, realtimeStopTimes }: { colour: string, realtimeStopTimes: RealtimeStopTime[] } = $props()

  let lastStop: number = $state(0)
  let offset: number = $state(17)
  
  $effect(() => {
    console.log('asdfasdf')
    console.log(realtimeStopTimes)
    for (let i = realtimeStopTimes.length - 1; i > 0; i--) {
      if (realtimeStopTimes[i].status === 'stop') {
        lastStop = realtimeStopTimes.length - i - 1
        break
      }
    }

    let index = 0
    for (let i = 0; i < realtimeStopTimes.length; i++) {
      if (!realtimeStopTimes[i].progress) {
        continue
      }

      console.log(realtimeStopTimes[i].stopName, realtimeStopTimes[i].progress)
      if (realtimeStopTimes[i].progress === 'not_passed') {
        index = i - 1
        break
      }
    }

    let newOffset = 0
    for (let i = 0; i < index; i++) {
      if (realtimeStopTimes[i].progress === 'passed') newOffset += 94
      else if (realtimeStopTimes[i].progress === 'skipped') newOffset += 72
    }

    offset = 17 + 94 * index
  })
</script>

<div class="h-full overflow-y-scroll">
  <div class="relative">
    <div
      style={`background-color: ${colour}; top: ${offset}px; bottom: ${lastStop * 72 + 69}px`}
      class={`absolute left-4.25 w-1.5 z-20`}
    ></div>
    {#each realtimeStopTimes as stopTime, i}
      <SidebarListing colour={colour} stopTime={stopTime} last={i === realtimeStopTimes.length - 1} />
    {/each}
  </div>
</div>
