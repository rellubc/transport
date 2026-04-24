import type { PageLoad } from './$types' 

import { getSydneyShapes } from "$lib/api/sydney"
import { getSydneyStops } from "$lib/api/sydney"
import { getSydneyVehiclePositions } from '$lib/api/sydney'

export const load: PageLoad = async ({ fetch }) => {
  const routes = await getSydneyShapes(fetch, "ROUTE")
  const shapes = await getSydneyShapes(fetch, "DISPLAY")
  const stops = await getSydneyStops(fetch)
  const vehicles = await getSydneyVehiclePositions(fetch)

  return { routes, shapes, stops, vehicles }
}
