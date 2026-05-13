import type { Consist } from "./vehicles.types"

export interface VehicleStopTime {
  stopId: string
  stopName: string
  stopSequence: number
  tripHeadsign: string
  routeShortName: string
  routeType: number
  routeColour: string
  arrivalTime: number
  departureTime: number
  arrivalDelay: number
  departureDelay: number
  stopType: string
  progress: string
  isRealtime: boolean
  consist: Consist[]
  displayTime: number
  stopProgress: number
}

export interface StopStopTime {
  tripId: string
  tripHeadsign: string
  routeShortName: string
  routeColour: string
  stopId: string
  stopName: string
  arrivalDelay: number
  departureDelay: number
  effectiveArrivalTime: number
  effectiveDepartureTime: number
  stopType: string
  isRealtime: boolean
  displayTime: number
}