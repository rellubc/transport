export interface Stop {
    id: string
    code?: string
    name: string
    description?: string
    latitude: number
    longitude: number
    zoneId?: string
    url?: string
    locationType: number
    parentStationId?: string
    timezone?: string
    wheelchairBoarding: boolean
    platformCode: number
    mode: string
    network: string
}

// export interface StopPlatformDto {
//     id: number
//     name: string
//     latitude: number
//     longitude: number
//     locationType: string
//     parentStationId: number
//     wheelchairBoarding: boolean
//     platformCode: number
// }

// export interface StopStationDto {
//     id: number
//     name: string
//     latitude: number
//     longitude: number
//     locationType: string
// }
