import type { PageLoad } from './$types' 

import { getSydneyShapes } from "$lib/api/sydney"
import { getSydneyStops } from "$lib/api/sydney"
import { getSydneyVehiclePositions } from '$lib/api/sydney'

export const load: PageLoad = async ({ fetch }) => {
  const shapes = await getSydneyShapes(fetch)
  const stops = await getSydneyStops(fetch, 'sydneytrains')
  const vehicles = await getSydneyVehiclePositions(fetch, undefined, 'sydneytrains')

  return { shapes, stops, vehicles }
}
