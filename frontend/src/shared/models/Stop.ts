export interface Stop {
    id: number
    name: string
    latitude: number
    longitude: number
}

export interface StopPlatformDto {
    id: number
    name: string
    latitude: number
    longitude: number
    parentStationId: number
    wheelchairBoarding: boolean
    platformCode: number
}

export interface StopStationDto {
    id: number
    name: string
    latitude: number
    longitude: number
}
