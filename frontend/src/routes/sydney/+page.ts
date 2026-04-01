import type { PageLoad } from './$types' 

import { getSydneyShapes } from "$lib/api/sydney-shapes"
import { getSydneyStops } from "$lib/api/sydney-stops"
import type { VehiclePosition } from '$lib/types/realtime'
import { MODE_TYPE_METRO, modeTypeMap } from '$lib/constants'

export const load: PageLoad = async ({ fetch }) => {
  const shapes = await getSydneyShapes(fetch, modeTypeMap[MODE_TYPE_METRO])
  const stops = await getSydneyStops(fetch, modeTypeMap[MODE_TYPE_METRO])
  // for realtime vehicle positions
  const vehicles: VehiclePosition[] = []

  return { shapes, stops, vehicles }
}
