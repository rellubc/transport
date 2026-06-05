<script lang="ts">
  const { title, id, routeShortName, routeColour } = $props()
</script>

<div class="flex flex-row items-center gap-4">
  <div style:background-color={`#${routeColour}`} class="w-12 h-6 flex flex-row justify-center items-center rounded-md">
    <p class="text-white font-bold">{routeShortName ? routeShortName : "NR"}</p>
  </div>
  <div class="sticky top-0 z-10 bg-white">
    <p class="text-xl font-bold">{title}</p>
  </div>
</div>
<p class="text-xs font-light">{id}</p>