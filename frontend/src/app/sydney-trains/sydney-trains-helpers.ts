import { Calendar } from "../../shared/models/calendar"
import { TripUpdate, VehiclePosition } from "../../shared/models/realtime"
import { Shape } from "../../shared/models/shape"
import { Stop } from "../../shared/models/stop"
import { StopTime } from "../../shared/models/stopTime"
import { Trip } from "../../shared/models/trip"

export const getSydneyTrainsCalendars = async (serviceId: string): Promise<Calendar>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/trains/calendars?serviceId=${serviceId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Calendar = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return { serviceId: '', monday: false, tuesday: false, wednesday: false, thursday: false, friday: false, saturday: false, sunday: false, startDate: '', endDate: '' }
  }
}

export const getSydneyTrainsShapes = async (): Promise<Shape>=> {
  try {
    const res = await fetch('https://localhost:7284/api/sydney/trains/shapes')
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Shape = await res.json()

    console.log(data)

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return {}
  }
}

export const getSydneyTrainsStops = async (): Promise<Stop[]>=> {
  try {
    const res = await fetch('https://localhost:7284/api/sydney/trains/stops')
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Stop[] = await res.json()

    const north = data.filter((stop) => stop.name.includes("Richmond")).sort((a, b) => b.latitude - a.latitude)
    const east = data.filter((stop) => stop.name.includes("Bondi Junction")).sort((a, b) => b.longitude - a.longitude)
    const south = data.filter((stop) => stop.name.includes("Waterfall")).sort((a, b) => a.latitude - b.latitude)
    const west = data.filter((stop) => stop.name.includes("Emu Plains")).sort((a, b) => a.longitude - b.longitude)

    const mostNorth = [north[0].latitude, north[0].longitude]
    const mostEast = [east[0].latitude, east[0].longitude]
    const mostSouth = [south[0].latitude, south[0].longitude]
    const mostWest = [west[0].latitude, west[0].longitude]
    
    return data.map((stop) => {
      if (stop.latitude <= mostNorth[0] &&
        stop.latitude >= mostSouth[0] &&
        stop.longitude <= mostEast[1] &&
        stop.longitude >= mostWest[1] &&
        !stop.name.includes("Menangle")) stop.network = 'local'
      else stop.network = 'regional'
      return stop
    })
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getSydneyTrainsStopsPlatforms = async (stopId: string): Promise<Stop[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/trains/stops-platforms?stopId=${stopId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Stop[] = await res.json()
    
    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getSydneyTrainsStopTimes = async (stopId: string): Promise<StopTime[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/trains/stop-times?stopId=${stopId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StopTime[] = await res.json()

    data.sort((a, b) => {
      const normalise = (time: string) => {
        let temp = parseInt(time.substring(0, 2))
        if (temp < 4) {
          temp += 24
          time = temp.toString() + time.substring(2, time.length - 1)
        }
        return time
      }
      return normalise(a.arrivalTime).localeCompare(normalise(b.arrivalTime))
    })

    // console.log(data)

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getSydneyTrainsTrip = async (tripId: string): Promise<Trip>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/trains/trips?tripId=${tripId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Trip = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return { id: '', routeId: '', serviceId: '', shapeId: -1, headSign: '', directionId: -1, shortName: '', wheelchairAccessible: 0, routeDirection: '' }
  }
}

export const getSydneyTrainsTripUpdates = async (tripId: string): Promise<TripUpdate>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/trains/realtime-trip-updates?tripId=${tripId}`)
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

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return { trip: {}, stopTimeUpdate: [] }
  }
}

export const getSydneyTrainsVehiclePositions = async (): Promise<VehiclePosition[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/trains/realtime-vehicle-positions`)
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
