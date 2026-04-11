import type { PageLoad } from './$types' 

import { getSydneyShapes, getSydneyStops, getSydneyVehiclePositions } from "$lib/api/sydney"
import { MODE_TYPE_METRO, modeTypeMap } from '$lib/constants'

export const load: PageLoad = async ({ fetch }) => {
  const shapes = await getSydneyShapes(fetch, modeTypeMap[MODE_TYPE_METRO])
  const stops = await getSydneyStops(fetch, modeTypeMap[MODE_TYPE_METRO])
  const vehicles = await getSydneyVehiclePositions(fetch, "M1")

  return { shapes, stops, vehicles }
}
