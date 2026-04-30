export interface Vehicle {
  tripId: string
  tripRouteId: string
  tripScheduleRelationship: string
  vehicleId: string
  vehicleLabel: string
  vehicleModel: string
  positionLatitude: number
  positionLongitude: number
  stopId: string
  timestamp: number
  congestionLevel: string
  occupancyStatus: string
  routeType: number
}

export interface Consist {
  positionInConsist: number
  occupancyStatus: string
}

export type Vehicles = Record<string, Vehicle[]>