import { StopTime } from "../../shared/models/stopTime"

export const getSydneyStopScheduledStopTimes = async (mode: string, stopName: string, timeString: string, before: boolean): Promise<StopTime[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/stop/scheduled/stop-times?mode=${mode}&stopName=${stopName}&timeString=${timeString}&before=${before}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StopTime[] = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getSydneyTripScheduledStopTimes = async (mode: string, tripId: string, timeString: string): Promise<StopTime[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/trip/scheduled/stop-times?mode=${mode}&tripId=${tripId}&timeString=${timeString}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StopTime[] = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getSydneyTripRealtimeStopTimes = async (mode: string, tripId: string, timeString: string): Promise<StopTime[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/trip/realtime/stop-times?mode=${mode}&tripId=${tripId}&timeString=${timeString}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StopTime[] = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}