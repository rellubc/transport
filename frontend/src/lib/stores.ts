import { writable, type Writable } from "svelte/store";
import type { Shapes } from "./types/shape";
import type { Stops } from "./types/stop";
import type { Vehicles } from "./types/realtime";
import { ObjectFlags } from "typescript";

export const modes: Writable<Record<string, Set<string>>> = writable<Record<string, Set<string>>>({})

export const shapes: Writable<Shapes> = writable<Shapes>({})
export const stops: Writable<Stops> = writable<Stops>({})
export const vehicles: Writable<Vehicles> = writable<Vehicles>({})

export const addModes = (newModes: Partial<Record<number, string[]>>) => {
  modes.update((current) => {
    const updated = { ...current}
    Object.entries(newModes).forEach(([mode, lines]) => {
      const set = new Set(current[Number(mode)])
      lines?.forEach((line) => set.add(line))
      updated[Number(mode)] = set
    })

    return updated
  })
}