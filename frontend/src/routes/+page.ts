import type { PageLoad } from './$types' 

import { getSydneyShapes, getSydneyVehiclePositions } from "$lib/api/sydney"
import { getSydneyStops } from "$lib/api/sydney"
import { MODE_TYPE_METRO, modeTypeMap } from '$lib/constants'

export const load: PageLoad = async () => {
  const shapes = await getSydneyShapes(fetch, modeTypeMap[MODE_TYPE_METRO])
  const stops = await getSydneyStops(fetch, modeTypeMap[MODE_TYPE_METRO])
  const vehicles = await getSydneyVehiclePositions(fetch, "M1")

  return { shapes, stops, vehicles }
}
