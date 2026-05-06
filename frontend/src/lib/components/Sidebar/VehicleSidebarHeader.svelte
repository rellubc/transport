<script lang="ts">
  import type { VehicleStopTime } from "$lib/types/stoptimes.types";
  import type { Vehicle } from "$lib/types/vehicles.types";

  const { stopTime, activeVehicle }: { stopTime: VehicleStopTime, activeVehicle: Vehicle } = $props()
</script>

<div class="sticky top-0 z-10 bg-white">
  <div class="flex flex-row items-center gap-4">
    <div style:background-color={`#${stopTime.routeColour}`} class="w-12 h-6 flex flex-row justify-center items-center rounded-md">
      <p class="text-white font-bold">{stopTime.routeShortName}</p>
    </div>
    <p class="font-bold text-xl">{stopTime.tripHeadsign.split('via')[0]}<span class="font-normal text-sm">{#if stopTime.tripHeadsign.split('via').length > 1} via {stopTime.tripHeadsign.split('via')[1]}{/if}</span></p> 
  </div>
  <p class="text-xs font-light">{activeVehicle.vehicleId}</p>
</div>