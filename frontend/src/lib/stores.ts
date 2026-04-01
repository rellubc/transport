import { derived, writable, type Writable } from "svelte/store";
import type { Shapes } from "./types/shape";
import type { Stop } from "./types/stop";
import type { VehiclePosition } from "./types/realtime";

export const modes: Writable<Record<number, Set<string>>> = writable<Record<number, Set<string>>>({})
export const lineShapes: Writable<Record<string, string[]>> = writable<Record<string, string[]>>({})

export const shapes: Writable<Shapes> = writable<Shapes>({})
export const stops: Writable<Stop[]> = writable<Stop[]>([])
export const vehicles: Writable<VehiclePosition[]> = writable<VehiclePosition[]>([])

export const addMode = (mode: number, line: string) => {
  modes.update((current) => {
    const set = new Set(current[mode]) ?? []

    set.add(line)

    return {
      ...current,
      [mode]: set
    }
  })
}