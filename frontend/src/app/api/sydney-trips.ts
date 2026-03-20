import { StopTime } from "../../shared/models/stopTime"
import { Trip } from "../../shared/models/trip"

export const getSydneyTrip = async (tripId: string): Promise<Trip> => {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/trip/${tripId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Trip = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return { id: '', routeId: '', serviceId: '', shapeId: -1, headSign: '', directionId: -1, shortName: '', wheelchairAccessible: 0, routeDirection: '' }
  }
}

export const getSydneyTripStopTimes = async (mode: string, tripId: string, timeString: string): Promise<StopTime[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/trip-stop-times?mode=${mode}&tripId=${tripId}&timeString=${timeString}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StopTime[] = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}