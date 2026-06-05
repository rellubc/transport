import type { PageLoad } from "../$types";
import { BASE_URL } from "$lib/constants";
import { initialiseDataStore } from "$lib/stores/map-data.store.svelte";

export const load: PageLoad = async ({ fetch }) => {
  const routeShapesRes = await fetch(`${BASE_URL}/api/sydney/shapes?shape_type=ROUTE`)
  const displayShapesRes = await fetch(`${BASE_URL}/api/sydney/shapes?shape_type=DISPLAY`)
  const stopsRes = await fetch(`${BASE_URL}/api/sydney/stops`)
  const vehiclesRes = await fetch(`${BASE_URL}/api/sydney/vehicles`)

  const routeShapes = await routeShapesRes.json()
  const displayShapes = await displayShapesRes.json()
  const stops = await stopsRes.json()
  const vehicles = await vehiclesRes.json()

  initialiseDataStore({ routeShapes, displayShapes, stops, vehicles })
}