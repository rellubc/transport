import { writable, type Writable } from "svelte/store";
import type { Shapes } from "./types/shape";
import type { Stops } from "./types/stop";
import type { Vehicles } from "./types/realtime";

export const modes: Writable<Record<number, Set<string>>> = writable<Record<number, Set<string>>>({})

export const shapes: Writable<Shapes> = writable<Shapes>({})
export const stops: Writable<Stops> = writable<Stops>({})
export const vehicles: Writable<Vehicles> = writable<Vehicles>({})

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