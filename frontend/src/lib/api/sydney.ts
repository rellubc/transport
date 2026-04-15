import type { Vehicles } from "$lib/types/realtime"
import type { Shapes } from "$lib/types/shape"
import type { Stops } from "$lib/types/stop"
import type { RealtimeStopTime, StaticStopTime } from "$lib/types/stopTime"
import type { Trip } from "$lib/types/trip"

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

export const getSydneyScheduledTrip = async (fetchFn: typeof fetch, tripId: string): Promise<Trip | null> => {
  const url = `http://localhost:8080/api/sydney/trip/${tripId}`
  
  try {
    const res = await fetchFn(url)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Trip = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return null
  }
}

export const getSydneyScheduledStopTimes = async (fetchFn: typeof fetch, stopId: string | null, tripId: string | null): Promise<StaticStopTime[]> => {
  let url = `http://localhost:8080/api/sydney/stop_times/static?`
  if (stopId) url = url.concat(`stop_id=${stopId}&`)
  if (tripId) url = url.concat(`trip_id=${tripId}`)

  try {
    const res = await fetchFn(url)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StaticStopTime[] = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getSydneyRealtimeStopTimes = async (fetchFn: typeof fetch, stopId: string | null, tripId: string | null): Promise<RealtimeStopTime[]> => {
  let url = `http://localhost:8080/api/sydney/stop_times/realtime?`
  if (stopId) url = url.concat(`stop_id=${stopId}&`)
  if (tripId) url = url.concat(`trip_id=${tripId}`)

  try {
    const res = await fetchFn(url)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: RealtimeStopTime[] = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}
