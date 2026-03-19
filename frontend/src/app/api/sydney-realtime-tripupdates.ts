import { TripUpdate } from "../../shared/models/realtime"
import { StopTime } from "../../shared/models/stopTime"

export const getSydneyTripUpdates = async (mode: string, tripId: string): Promise<TripUpdate>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/realtime-trip-updates?mode=${mode}&tripId=${tripId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: TripUpdate = await res.json()
    
    console.log(data)

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return { trip: {}, stopTimeUpdate: [] }
  }
}

export const getSydneyRealtimeTripStopTimes = async (mode: string, tripId: string, timeString: string): Promise<StopTime[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/realtime-trip-stop-times?mode=${mode}&tripId=${tripId}&timeString=${timeString}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StopTime[] = await res.json()

    console.log(data)

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}