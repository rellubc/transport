export interface Shape {
    shape_id: string
    shape_pt_lat: number
    shape_pt_lon: number
    shape_pt_sequence: number
    shape_pt_distanceTravelled: number
}

export interface ShapeDetails {
    shape_pt_lat: number
    shape_pt_lon: number
    shape_pt_sequence: number
    shape_pt_distanceTravelled: number
    mode: string
}

export interface ShapeCoord {
    shape_pt_lat: number
    shape_pt_lon: number
}

export type Shapes = Record<string, ShapeCoord[]>
