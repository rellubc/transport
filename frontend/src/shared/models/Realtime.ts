export interface RealtimeStopTimeUpdateDto {
  entityId: string
  tripId: string
  stopSequence: number
  stopId: number
  arrivalTime: string
  departureTime: string
  scheduleRelationship: string
  insertedAt: string
}

export interface RealtimeVehiclePositionDto {
  entityId: string
  vehicleId: string
  label: string
  licensePlate: string
  latitude: number
  longitude: number
  bearing: number
  speed: number
  tripId: string
  currentStopSequence: number
  stopId: number
  currentStatus: string
  congestionLevel: string
  occupancyStatus: string
  insertedAt: string
}