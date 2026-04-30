import type { StopStopTime } from "$lib/types/stoptimes.types"
import { client } from "./client.api"

export const getStopStopTimes = (stopId: string, direction: string, time: number) => client<StopStopTime[]>(`/api/sydney/stop/${stopId}/stop_times?direction=${direction}&time=${time}`)
export const getVehicleStopTimes = (vehicleId: string) => client<StopStopTime[]>(`/api/sydney/vehicle/${vehicleId}/stop_times`)