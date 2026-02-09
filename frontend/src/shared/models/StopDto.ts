export interface StopDto {
    id: number
    name: string
    latitude: number
    longitude: number
    locationType: string
    parentStationId?: number
    wheelchairBoarding: boolean
    platformCode?: number
}