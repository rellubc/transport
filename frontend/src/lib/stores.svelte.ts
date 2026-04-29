import type { Stops } from "$lib/types/stops.types"

let stops = $state<Stops>({})

export const getStops = () => stops
export const setStops = (data: Stops) => stops = data