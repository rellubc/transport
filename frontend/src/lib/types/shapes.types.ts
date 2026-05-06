export interface ShapeDetails {
  shapePtLat: number;
  shapePtLon: number;
  shapePtSequence: number;
  shapeDistTravelled: number;
  routeType: number;
}

export interface ShapeCoord {
  shapePtLat: number
  shapePtLon: number
}

export type Shapes = Record<string, ShapeDetails[]>