import type { PageLoad } from "../$types";
import { BASE_URL } from "$lib/constants";

export const load: PageLoad = async ({ fetch }) => {
  const stopsRes = await fetch(`${BASE_URL}/api/sydney/stops`)
  const vehiclesRes = await fetch(`${BASE_URL}/api/sydney/vehicles`)

  return { 
    stops: await stopsRes.json(),
    vehicles: await vehiclesRes.json()
  }
}