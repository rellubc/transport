export interface Shape {
    shapeId: string
    shapePtLat: number
    shapePtLon: number
    shapePtSequence: number
    shapePtDistanceTravelled: number
}

export interface ShapeDetails {
    shapePtLat: number
    shapePtLon: number
    shapePtSequence: number
    shapePtDistanceTravelled: number
    mode: string
}

export interface ShapeCoord {
    shapePtLat: number
    shapePtLon: number
}

export type Shapes = Record<string, ShapeCoord[]>
