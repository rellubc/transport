import { VehiclePosition } from "../../shared/models/realtime"
import { TripUpdate } from "../../shared/models/realtime"
import { StopTime } from "../../shared/models/stopTime"

export const getSydneyRealtimeTripUpdate = async (mode: string, tripId: string): Promise<TripUpdate>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/realtime/trip-updates?mode=${mode}&tripId=${tripId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: TripUpdate = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return { trip: {}, stopTimeUpdate: [] }
  }
}

export const getSydneyRealtimeVehicles = async (mode: string): Promise<VehiclePosition[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/realtime/vehicles?mode=${mode}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: VehiclePosition[] = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}