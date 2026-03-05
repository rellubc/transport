import { Calendar } from "./calendar"

export interface Trip {
    id: string
    routeId: string
    serviceId: number
    shapeId: number
    headSign: string
    directionId: number
    shortName: string
    blockId?: number
    wheelchairAccessible?: number
    tripNote?: string
    routeDirection: string
    bikesAllowed?: number
}