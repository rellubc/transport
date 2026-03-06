export interface TripUpdate {
  trip: TripDescriptor
  vehicle?: VehicleDescriptor
  stopTimeUpdate: StopTimeUpdate[]
  timestamp?: Date
  delay?: number
}

export interface VehiclePosition {
  vehicle?: VehicleDescriptor
  position?: Position
  trip?: TripDescriptor
  currentStopSequence?: number
  stopId?: string
  currentStatus?: number
  timestamp?: Date | number | string
  congestionLevel?: number
  occupancyStatus?: number
  consist?: CarriageDescriptor[]
}

export interface Alert {
  activePeriod: TimeRange[]
  informedEntity: EntitySelector[]
  cause?: number
  effect?: number
  url?: TranslatedString
  headerText?: TranslatedString
  descriptionText?: TranslatedString
  ttsHeader?: TranslatedString
  ttsDescription?: TranslatedString
  severityLevel?: number
}

export interface UpdateBundle {
  gtfsStaticBundle: string
  updateSequence: number
  cancelledTrip: string[]
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

export interface StopTimeUpdate {
  stopSequence?: number
  stopName?: string
  stopId?: string
  arrival?: StopTimeEvent
  departure?: StopTimeEvent
  scheduleRelationship?: number
  departureOccupancyStatus?: number
  carriage?: CarriageDescriptor
}

export interface StopTimeEvent {
  delay?: number
  time?: Date
  uncertainty?: number
}

export interface TimeRange {
  start?: number
  end?: number
}

export interface EntitySelector {
  agencyId?: string
  routeId?: string
  routeType?: number
  trip?: TripDescriptor
  stopId?: string
  directionId?: number
}

export interface TranslatedString {
  translation: Translation[]
}

export interface Translation {
  text: string
  language?: string
}