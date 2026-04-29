export interface Stop {
  stopId: string;
  stopCode: string | null;
  stopName: string;
  stopLat: number;
  stopLon: number;
  stopGeom: GeoJSONPoint | null;
  stopZoneId: string | null;
  stopUrl: string | null;
  stopLocationType: number | null;
  stopParentStation: string | null;
  stopTimezone: string | null;
  stopWheelchairBoarding: number;
  stopPlatformCode: string | null;
  mode: string;
}

interface GeoJSONPoint {
  type: 'Point';
  coordinates: [number, number];
}

export type Stops = Record<string, Stop[]>