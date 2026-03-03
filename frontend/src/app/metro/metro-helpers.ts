import { RealtimeStopTimeUpdateDto, RealtimeVehiclePositionDto } from "../../shared/models/Realtime"
import { Shape } from "../../shared/models/Shape"
import { StopPlatformDto, StopStationDto } from "../../shared/models/Stop"
import { StopTimeDto } from "../../shared/models/StopTime"

export const getMetroPlatforms = async (): Promise<StopPlatformDto[]>=> {
  try {
    const res = await fetch('https://localhost:7284/api/sydney/metro/platforms')
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StopPlatformDto[] = await res.json()
    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getMetroStations = async (): Promise<StopStationDto[]>=> {
  try {
    const res = await fetch('https://localhost:7284/api/sydney/metro/stations')
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StopStationDto[] = await res.json()
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

    console.log(data)

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return {}
  }
}

export const getStationStopTimes = async (stopId: number): Promise<StopTimeDto[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/metro/stop-times?stopId=${stopId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StopTimeDto[] = await res.json()

    console.log(data)

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getRealTimeStopTimes = async (): Promise<RealtimeStopTimeUpdateDto[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/metro/realtime-stop-times`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: RealtimeStopTimeUpdateDto[] = await res.json()

    console.log(data)

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getRealTimeVehiclePositions = async (): Promise<RealtimeVehiclePositionDto[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/metro/realtime-vehicle-positions`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: RealtimeVehiclePositionDto[] = await res.json()

    console.log(data)

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}
