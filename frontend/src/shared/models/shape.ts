export interface ShapeDto {
    id: string
    latitude: number
    longitude: number
    sequence: number
    distanceTravelled: number
}

interface ShapeDetails {
    latitude: number
    longitude: number
    sequence: number
    distanceTravelled: number
    mode: string
}

export type Shape = Record<string, ShapeDetails[]>
