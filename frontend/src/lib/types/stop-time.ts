export interface StaticStopTime {
  tripId: string
  stopId: string
  stopName: string
  arrivalTime: number
  departureTime: number
  stopSequence: number
  stopHeadsign: string
  pickupType: number
  dropOffType: number
  shapeDistTravelled: number
  timepoint: number
  stopNote: string
  mode: string
}

export interface RealtimeStopTime {
  stopId: string
  stopName: string
  stopPtSequence: number
  vehiclePtSequence: number
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

export interface Consist {
  positionInConsist: number
  occupancyStatus: string
}