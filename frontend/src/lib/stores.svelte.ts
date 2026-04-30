import type { Stops } from "$lib/types/stops.types"
import type { Vehicles } from "./types/vehicles.types"

let stops = $state<Stops>({})
let vehicles = $state<Vehicles>({})

export const getStops = () => stops
export const setStops = (data: Stops) => stops = data

export const getVehicles = () => vehicles
export const setVehicles = (data: Vehicles) => vehicles = data