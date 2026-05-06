import type { Stops } from "$lib/types/stops.types"
import type { Shapes } from "$lib/types/shapes.types"
import type { Vehicles } from "$lib/types/vehicles.types"

export const transportDataStore = $state<{ routeShapes: Shapes, displayShapes: Shapes, stops: Stops, vehicles: Vehicles, modes: Set<string> }>({
  routeShapes: {},
  displayShapes: {},
  stops: {},
  vehicles: {},
  modes: new Set()
})
