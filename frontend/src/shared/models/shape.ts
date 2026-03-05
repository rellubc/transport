export interface ShapeDto {
    id: number
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
}

export type Shape = Record<number, ShapeDetails[]>
