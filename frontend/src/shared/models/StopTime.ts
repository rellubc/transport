export interface StopTimeDto {
  tripId: string
  arrivalTime: string
  departureTime: string
  stopId: number
  stopSequence: number
  stopHeadSign: string
  pickupType: number
  dropOffType: number
  shapeDistTravelled: number
  timepoint: number
  stopNote: string
}