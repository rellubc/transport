export interface Stop {
    stopId: string
    stopCode?: string
    stopName: string
    stopDescription?: string
    stopLat: number
    stopLon: number
    stopZoneId?: string
    stopUrl?: string
    stopLocationType: number
    stopParentStation?: string
    stopTimezone?: string
    stopWheelchairBoarding: boolean
    stopPlatformCode: number
    mode: string
}

export type Stops = Record<string, Stop[]>

// export interface StopPlatformDTO {
//     id: number
//     name: string
//     latitude: number
//     longitude: number
//     locationType: string
//     parentStationId: number
//     wheelchairBoarding: boolean
//     platformCode: number
// }

// export interface StopStationDTO {
//     id: number
//     name: string
//     latitude: number
//     longitude: number
//     locationType: string
// }
