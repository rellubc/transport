export interface Stop {
    stop_id: string
    stop_code?: string
    stop_name: string
    stop_description?: string
    stop_lat: number
    stop_lon: number
    stop_zone_id?: string
    stop_url?: string
    stop_location_type: number
    stop_parent_station?: string
    stop_timezone?: string
    stop_wheelchair_boarding: boolean
    stop_platformCode: number
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
