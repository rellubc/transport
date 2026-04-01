export interface Shape {
    id: string
    latitude: number
    longitude: number
    sequence: number
    distanceTravelled: number
}

export interface ShapeDetails {
    latitude: number
    longitude: number
    sequence: number
    distanceTravelled: number
    mode: string
}

export type Shapes = Record<string, ShapeDetails[]>
