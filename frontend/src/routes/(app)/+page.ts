import type { PageLoad } from "./$types";
import { BASE_URL } from "$lib/constants";

export const load: PageLoad = async ({ fetch }) => {
  const [stopsRes, vehiclesRes] = await Promise.all([
    fetch(`${BASE_URL}/api/sydney/stops`),
    fetch(`${BASE_URL}/api/sydney/vehicles`)
  ])

  const [stops, vehicles] = await Promise.all([
    stopsRes.json(),
    vehiclesRes.json()
  ])

  console.log(stops, vehicles)

  return { stops, vehicles }
}