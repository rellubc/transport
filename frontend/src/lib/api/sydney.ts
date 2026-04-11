import type { Vehicles } from "$lib/types/realtime"
import type { Shapes } from "$lib/types/shape"
import type { Stops } from "$lib/types/stop"

export const getSydneyStops = async (fetchFn: typeof fetch, mode?: string): Promise<Stops>=> {
  let url = `http://localhost:8080/api/sydney/stops`
  if (mode) url = url.concat(`?mode=${mode}`)

  try {
    const res = await fetchFn(url)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Stops = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return {}
  }
}

export const getSydneyShapes = async (fetchFn: typeof fetch, mode?: string): Promise<Shapes> => {
  const url = `http://localhost:8080/api/sydney/shapes`
  if (mode) url.concat(`?mode=${mode}`)

  try {
    const res = await fetchFn(url)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Shapes = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return {}
  }
}

export const getSydneyVehiclePositions = async (fetchFn: typeof fetch, routeId?: string, mode?: string): Promise<Vehicles> => {
  let url = `http://localhost:8080/api/sydney/vehicle_positions?`
  if (routeId) url = url.concat(`route_id=${routeId}&`)
  if (mode) url = url.concat(`mode=${mode}`)

  try {
    const res = await fetchFn(url)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Vehicles = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return {}
  }
}