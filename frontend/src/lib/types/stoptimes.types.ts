import type { Consist } from "./vehicles.types"

export interface VehicleStopTime {
  stopId: string
  stopName: string
  stopSequence: number
  stopHeadsign: string
  arrivalTime: number
  departureTime: number
  arrivalDelay: number
  departureDelay: number
  status: string
  progress: string
  consist: Consist[]
}

export interface StopStopTime {
  tripId: string
  tripHeadsign: string
  serviceId: string
  routeId: string
  routeShortName: string
  routeType: number
  stopId: string
  stopName: string
  arrivalTime: number
  arrivalDelay: number
  departureTime: number
  departureDelay: number
  effectiveArrivalTime: number
  effectiveDepartureTime: number
  pickupType: number
  dropOffType: number
  stopType: string
  isRealtime: boolean
  displayTime: number
  hasContinuation: boolean
}