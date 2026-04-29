import type { Stops } from "$lib/types/stops.types";
import type { PageLoad } from "./$types";

const BASE_URL = import.meta.env.VITE_API_BASE_URL

export const load: PageLoad = async ({ fetch }) => {
  const stops = await fetch(`${BASE_URL}/api/sydney/stops`)

  return {
    stops: await stops.json() as Stops
  }
}