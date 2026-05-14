import { BASE_URL } from "$lib/constants";
import type { Stop } from "$lib/types/stops.types";
import type { StopStopTime } from "$lib/types/stoptimes.types";
import type { Vehicle, Vehicles } from "$lib/types/vehicles.types";

export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options
  });

  if (!res.ok) throw new Error(`${res.status} ${path}`);

  return await res.json();
}

export const stopsApi = {
  getById: (stopId: string) => request<Stop>(`/api/sydney/stops/${stopId}`),
}

export const stopTimesApi = {
  getForStop: (stopId: string, direction: string, time: number) => request<StopStopTime[]>(`/api/sydney/stops/${stopId}/stop_times?direction=${direction}&time=${time}`),
  getForTrip: (tripId: string, lon: number, lat: number) => request<StopStopTime[]>(`/api/sydney/trips/${tripId}/stop_times?vehicle_lon=${lon}&vehicle_lat=${lat}`),
  getForVehicle: (vehicleId: string, lon: number, lat: number) => request<StopStopTime[]>(`/api/sydney/vehicles/${vehicleId}/stop_times?vehicle_lon=${lon}&vehicle_lat=${lat}`),
}

export const vehiclesApi = {
  getAll: () => request<Vehicles>(`/api/sydney/vehicles`),
  getByTrip: (tripId: string) => request<Vehicle>(`/api/sydney/vehicle?trip_id=${tripId}`),
  getById: (vehicleId: string) => request<Vehicle>(`/api/sydney/vehicle?vehicle_id=${vehicleId}`)
}