export interface StopMapIcon {
    id: number
    name: string
    latitude: number
    longitude: number
}

export interface Stop {
    id: number
    name: string
    latitude: number
    longitude: number
    locationType: string
    parentStationId: number
    wheelchairBoarding: boolean
    platformCode: number
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
