<script lang="ts">
  import X from "lucide-svelte/icons/x";

  const { tripHeadsign, route, colour, onClose }: { tripHeadsign: string, route: string, colour: string, onClose?: () => void } = $props()

  const headsign = $derived(tripHeadsign.split('via')[0].trim())
  const via = $derived(tripHeadsign.includes('via') ? `via ${tripHeadsign.split('via')[1].trim()}` : "")
</script>

<div class="sticky top-0 z-10 bg-white w-[calc(100% - 4rem)] p-8 rounded-t-2xl">
  <div class="flex flex-row items-center gap-4">
    <div
      style={`background-color: ${colour}`} 
      class={`w-12 h-12 flex flex-row justify-center items-center rounded-lg`}
    >
      <p class="text-white text-2xl font-bold">{route}</p>
    </div>
    <div class="flex flex-col">
      <p class="text-2xl font-bold">{headsign}</p>
      <p class="text-lg/4">{via}</p>
    </div>
    <button class="ml-auto cursor-pointer" onclick={() => onClose?.()}>
      <X />
    </button>
  </div>
</div>