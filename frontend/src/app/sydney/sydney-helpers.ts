import { Calendar } from "../../shared/models/calendar"
import { TripUpdate, VehiclePosition } from "../../shared/models/realtime"
import { Shape } from "../../shared/models/shape"
import { Stop } from "../../shared/models/stop"
import { StopTime } from "../../shared/models/stopTime"
import { Trip } from "../../shared/models/trip"

export const getSydneyCombinedCalendars = async (serviceId: string): Promise<Calendar>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/combined/calendars?serviceId=${serviceId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Calendar = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return { serviceId: '', monday: false, tuesday: false, wednesday: false, thursday: false, friday: false, saturday: false, sunday: false, startDate: '', endDate: '' }
  }
}

export const getSydneyCombinedShapes = async (): Promise<Shape>=> {
  try {
    const res = await fetch('https://localhost:7284/api/sydney/combined/shapes')
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Shape = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return {}
  }
}

export const getSydneyCombinedStops = async (): Promise<Stop[]>=> {
  try {
    const res = await fetch('https://localhost:7284/api/sydney/combined/stops')
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Stop[] = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getSydneyCombinedStopsPlatforms = async (stopId: string): Promise<Stop[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/combined/stops-platforms?stopId=${stopId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Stop[] = await res.json()
    
    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getSydneyCombinedStopTimes = async (stopName: string, timeString: string, before: boolean): Promise<StopTime[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/combined/stop-times?stopName=${stopName}&timeString=${timeString}&before=${before}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StopTime[] = await res.json()

    // console.log(data)

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getSydneyCombinedTrip = async (tripId: string): Promise<Trip | null>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/combined/trips?tripId=${tripId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Trip = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return null
  }
}

export const getSydneyCombinedTripStopTimes = async (tripId: string, timeString: string): Promise<StopTime[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/combined/trip-stop-times?tripId=${tripId}&timeString=${timeString}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StopTime[] = await res.json()

    // console.log(data)

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}


export const getSydneyCombinedTripUpdates = async (tripId: string): Promise<TripUpdate | null>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/combined/realtime-trip-updates?tripId=${tripId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: TripUpdate = await res.json()

    data.stopTimeUpdate.map((stu) => {
      if (stu.arrival?.time) {
        stu.arrival.time = new Date(Number(stu.arrival?.time) * 1000)
      }

      if (stu.departure?.time) {
        stu.departure.time = new Date(Number(stu.departure?.time) * 1000)
      }
    })

    if (data.timestamp) {
      data.timestamp = new Date(Number(data.timestamp) * 1000)
    }

    // console.log(data)

    if (data.trip === null || data.stopTimeUpdate === null) return null

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return { trip: {}, stopTimeUpdate: [] }
  }
}

export const getSydneyCombinedVehiclePositions = async (): Promise<VehiclePosition[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/combined/realtime-vehicle-positions`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: VehiclePosition[] = await res.json()

    data.map((vehicle) => {
      if (vehicle.timestamp) {
        vehicle.timestamp = new Date(Number(vehicle.timestamp) * 1000)
      }
    })

    // console.log(data)

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}
