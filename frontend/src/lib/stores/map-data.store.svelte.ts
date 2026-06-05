import type { Stops } from "$lib/types/stops.types"
import type { Shapes } from "$lib/types/shapes.types"
import type { Vehicles } from "$lib/types/vehicles.types"
import { vehiclesApi } from "../api/client.api"
import type { PageData } from "../../routes/(app)/map/$types"

export interface DataStore {
  routeShapes: Shapes
  displayShapes: Shapes
  stops: Stops
  vehicles: Vehicles
  modes: Set<string>
  refreshId: number
}

export const transportDataStore = $state<DataStore>({
  routeShapes: {},
  displayShapes: {},
  stops: {},
  vehicles: {},
  modes: new Set(),
  refreshId: 0
})

export const initialiseDataStore = (data: PageData) => {
  transportDataStore.routeShapes = data.routeShapes
  transportDataStore.displayShapes = data.displayShapes
  transportDataStore.stops = data.stops
  transportDataStore.vehicles = data.vehicles

  Object.keys(data.routeShapes).forEach((shapeId) => {
    transportDataStore.modes.add(shapeId.split('_')[1])
  })
}

export const refreshVehicles = async () => {
  transportDataStore.vehicles = await vehiclesApi.getAll()
  transportDataStore.refreshId++
}
