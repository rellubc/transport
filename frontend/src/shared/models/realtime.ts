export interface RealtimeStopTimeUpdateDto {
  trip: TripDescriptor
  vehicle?: VehicleDescriptor
  stopTimeUpdate: StopTimeUpdate[]
  timestamp?: Date
  delay?: number
}

export interface RealtimeVehicle {
  vehicle?: VehicleDescriptor
  position?: Position
  trip?: TripDescriptor
  currentStopSequence?: number
  stopId?: string
  currentStatus?: number
  timestamp?: Date
  congestionLevel?: number
  occupancyStatus?: number
  consist?: CarriageDescriptor
}

export interface TripDescriptor {
  tripId?: string
  routeId?: string
  directionId?: number
  startTime?: string
  startDate?: string
  scheduleRelationship?: number
}

export interface VehicleDescriptor {
  id?: string
  label?: string
  licensePlate?: string
  tfnswVehicleDescriptor: TfnswVehicleDescriptor
}

export interface Position {
  latitude?: number
  longitude?: number
  bearing?: number
  odometer?: number
  speed?: number
  trackDirection?: number
}

export interface CarriageDescriptor {
  name?: string
  positionInConsist: number
  occupancyStatus?: number
  quietCarriage: boolean
  toilet?: number
  luggageRack: boolean
  departureOccupancyStatus?: number
}

export interface TfnswVehicleDescriptor {
  airConditioned?: boolean
  wheelchairAccessible?: number
  vehicleModel?: string
  performingPriorTrip?: boolean
  specialVehicleAttributes?: number
}

export class StopTimeUpdate {
  stopSequence?: number
  stopName?: string
  stopId?: string
  arrival?: StopTimeEvent
  departure?: StopTimeEvent
  scheduleRelationship?: number
  departureOccupancyStatus?: number
  carriage?: CarriageDescriptor
}

export class StopTimeEvent {
  delay?: number
  time?: Date
  uncertainty?: number
}
