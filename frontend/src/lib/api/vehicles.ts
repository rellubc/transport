import type { Vehicle, Vehicles } from "$lib/types/vehicles.types";
import { request } from "./client.api";

export const vehiclesApi = {
  getAll: () => request<Vehicles>(`/api/sydney/vehicles`),
  getById: (vehicleId: string) => request<Vehicle>(`/api/sydney/vehicles/${vehicleId}`)
}