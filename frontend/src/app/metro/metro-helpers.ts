import { RealtimeStopTimeUpdateDto, RealtimeVehicle } from "../../shared/models/realtime"
import { Shape } from "../../shared/models/shape"
import { Stop } from "../../shared/models/stop"
import { StopTimeDto } from "../../shared/models/stopTime"
import { Trip } from "../../shared/models/trip"


export const getMetroStops = async (): Promise<Stop[]>=> {
  try {
    const res = await fetch('https://localhost:7284/api/sydney/metro/stops')
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Stop[] = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getMetroShapes = async (): Promise<Shape>=> {
  try {
    const res = await fetch('https://localhost:7284/api/sydney/metro/shapes')
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Shape = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return {}
  }
}

export const getMetroTrip = async (tripId: string): Promise<Trip>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/metro/trips?tripId=${tripId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Trip = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return { id: '', routeId: '', serviceId: -1, shapeId: -1, headSign: '', directionId: -1, shortName: '', wheelchairAccessible: 0, routeDirection: '' }
  }
}

export const getStationStopTimes = async (stopId: number): Promise<StopTimeDto[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/metro/stop-times?stopId=${stopId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StopTimeDto[] = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getRealTimeStopTimes = async (tripId: string): Promise<RealtimeStopTimeUpdateDto>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/metro/realtime-stop-times?tripId=${tripId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: RealtimeStopTimeUpdateDto = await res.json()

    data.stopTimeUpdate.map((stu) => {
      if (stu.arrival?.time) {
        stu.arrival.time = new Date(stu.arrival?.time)
      }

      if (stu.departure?.time) {
        stu.departure.time = new Date(stu.departure?.time)
      }
    })

    if (data.timestamp) {
      data.timestamp = new Date(data.timestamp)
    }

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return { trip: {}, stopTimeUpdate: [] }
  }
}

export const getRealTimeVehiclePositions = async (): Promise<RealtimeVehicle[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/metro/realtime-vehicle-positions`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: RealtimeVehicle[] = await res.json()

    data.map((vehicle) => {
      if (vehicle.timestamp) {
        vehicle.timestamp = new Date(vehicle.timestamp)
      }
    })

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}
