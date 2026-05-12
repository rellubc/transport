import type { PageLoad } from "../$types";
import { BASE_URL } from "$lib/constants";

export const load: PageLoad = async ({ fetch }) => {
  const routeShapesRes = await fetch(`${BASE_URL}/api/sydney/shapes?shape_type=ROUTE`)
  const displayShapesRes = await fetch(`${BASE_URL}/api/sydney/shapes?shape_type=DISPLAY`)
  const stopsRes = await fetch(`${BASE_URL}/api/sydney/stops`)
  const vehiclesRes = await fetch(`${BASE_URL}/api/sydney/vehicles`)

  return { 
    routeShapes: await routeShapesRes.json(),
    displayShapes: await displayShapesRes.json(),
    stops: await stopsRes.json(),
    vehicles: await vehiclesRes.json()
  }
}