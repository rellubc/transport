export interface StopTime {
  tripId: string
  arrivalTime: string
  departureTime: string
  stopId: number
  stopName: string
  routeId: string
  stopSequence: number
  stopHeadSign?: string
  pickupType: number
  dropOffType: number
  shapeDistTravelled: number
  timepoint?: number
  stopNote?: string
  mode: string
}