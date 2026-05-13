export interface Stop {
  stopId: string;
  stopName: string;
  stopLat: number;
  stopLon: number;
  stopParentStation: string | null;
  stopWheelchairBoarding: number;
  routeType: string;
}

export type Stops = Record<string, Stop[]>