import { TripUpdate, VehiclePosition } from "../../shared/models/realtime"
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

export const getTripUpdates = async (tripId: string): Promise<TripUpdate>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/metro/realtime-trip-updates?tripId=${tripId}`)
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

    console.log(data)

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return { trip: {}, stopTimeUpdate: [] }
  }
}

export const getVehiclePositions = async (): Promise<VehiclePosition[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/metro/realtime-vehicle-positions`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: VehiclePosition[] = await res.json()

    data.map((vehicle) => {
      if (vehicle.timestamp) {
        vehicle.timestamp = new Date(Number(vehicle.timestamp) * 1000)
      }
    })

    console.log(data)

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}
