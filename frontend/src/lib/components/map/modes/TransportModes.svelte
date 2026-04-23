<script lang="ts">
  import { MODEICONS, ModeLabels, ModeType } from "$lib/constants";
  import { addModes, modes, shapes } from "$lib/stores";
  import type { ModeIcon } from "$lib/types/general";
  import { get } from "svelte/store";

  let disabledModes = $state<Set<string>>(new Set())

  const updateModes = (icon: ModeIcon) => {
    const modeName = icon.name.replace('-icon', '')
    const modeEntry = Object.entries(ModeLabels).find(([_, label]) => label === modeName)
    if (!modeEntry) return

    const modeKey = Number(modeEntry[0])
    if (disabledModes.has(modeName)) {
      disabledModes.delete(modeName)
      disabledModes = new Set(disabledModes)

      const newModes: Partial<Record<number, string[]>> = {}
      Object.keys(get(shapes)).forEach((shapeId) => {
        for (let i = 1; i <  shapeId.split('_')[0].length; i++) {
          const line = shapeId[0] + shapeId[i]

          let modeType: number | null = null
          if (/^T[0-9]$/.test(line) && modeKey === ModeType.RAIL) modeType = ModeType.RAIL
          if (/^M[0-9]$/.test(line) && modeKey === ModeType.METRO) modeType = ModeType.METRO
          if (/^L[0-9]$/.test(line) && modeKey === ModeType.LIGHT_RAIL) modeType = ModeType.LIGHT_RAIL

          if (modeType !== null) {
            newModes[modeType] = [...(newModes[modeType] ?? []), line]
          }
        }
      })

      addModes(newModes)
    } else {
      disabledModes.add(modeName)
      disabledModes = new Set(disabledModes)
      modes.update((m) => {
        m[modeKey] = new Set()
        return m
      })
    }
  }
</script>

<div class="absolute top-4 right-14 bg-white h-12 w-60 flex flex-col rounded-full shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)] overflow-hidden">
  <div class="flex flex-row">
    {#each MODEICONS as mode}
      <button onclick={() => updateModes(mode)}>
        <img src={mode.url} alt={mode.name} class={`w-12 h-12 cursor-pointer transition-opacity ${disabledModes.has(mode.name.replace('-icon', '')) ? 'opacity-50' : 'opacity-100'} `} />
      </button>
    {/each}
  </div>
</div>