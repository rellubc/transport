import type { StopStopTime } from "$lib/types/stoptimes.types";
import { request } from "./client.api";

export const stopTimesApi = {
  getForStop: (stopId: string, direction: string, time: number) => request<StopStopTime[]>(`/api/sydney/stops/${stopId}/stop_times?direction=${direction}&time=${time}`),
  getForTrip: (tripId: string, lon: number, lat: number) => request<StopStopTime[]>(`/api/sydney/trips/${tripId}/stop_times?vehicle_lon=${lon}&vehicle_lat=${lat}`)
}