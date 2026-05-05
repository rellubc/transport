import type { StopStopTime } from "$lib/types/stoptimes.types"
import type { Vehicle } from "$lib/types/vehicles.types"
import { client } from "./client.api"

export const getStopStopTimes = (stopId: string, direction: string, time: number) => client<StopStopTime[]>(`/api/sydney/stops/${stopId}/stop_times?direction=${direction}&time=${time}`)
export const getTripStopTimes = (tripId: string, lon: number, lat: number) => client<StopStopTime[]>(`/api/sydney/trips/${tripId}/stop_times?vehicle_lon=${lon}&vehicle_lat=${lat}`)

export const getVehicle = (vehicleId: string) => client<Vehicle>(`/api/sydney/vehicles/${vehicleId}`)