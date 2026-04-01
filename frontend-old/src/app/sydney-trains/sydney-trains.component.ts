import { Component, ViewChild } from '@angular/core';
import { MapComponent } from '../components/map/map.component';

import { Stop } from '../../shared/models/stop';
import { Shapes } from '../../shared/models/shape';
import { VehiclePosition } from '../../shared/models/realtime';
import { ROUTE_TYPE_RAIL, routeTypeMap } from '../../shared/models/constants';
import { getSydneyStops } from '../api/sydney-stops';
import { getSydneyShapes } from '../api/sydney-shapes';
import { getSydneyRealtimeVehicles } from '../api/sydney-realtime';

@Component({
  selector: 'app-sydney-trains',
  imports: [MapComponent],
  templateUrl: './sydney-trains.component.html',
  styleUrl: './sydney-trains.component.css'
})
export class SydneyTrainsComponent {
  @ViewChild(MapComponent) map!: MapComponent

  stops: Stop[] = []
  shapes: Shapes = {}
  vehicles: VehiclePosition[] = []

  async ngOnInit(): Promise<void> {
    this.stops = await getSydneyStops(routeTypeMap[ROUTE_TYPE_RAIL])
    this.map.stops = this.stops

    this.shapes = await getSydneyShapes(routeTypeMap[ROUTE_TYPE_RAIL])
    this.map.shapes = this.shapes

    this.map.routeTypes[ROUTE_TYPE_RAIL] = new Set()
    const routeSet = new Set(Object.keys(this.shapes).map(shapeId => shapeId.split('_')[0]))
    for (const routeId of routeSet) {
      this.map.routeTypes[ROUTE_TYPE_RAIL].add(routeId)
      this.map.addShapeSource(ROUTE_TYPE_RAIL)
    }
    this.map.addStopSource(ROUTE_TYPE_RAIL)
    this.map.addVehicleSource(ROUTE_TYPE_RAIL)

    this.map.addShapes()
    this.map.addStops()

    this.vehicles = await getSydneyRealtimeVehicles(routeTypeMap[ROUTE_TYPE_RAIL])
    this.map.vehicles = this.vehicles
    this.map.refresh()

    setInterval(async () => {
      this.vehicles = await getSydneyRealtimeVehicles(routeTypeMap[ROUTE_TYPE_RAIL]);
      this.map.vehicles = this.vehicles
      this.map.refresh()
    }, 15000)
  }
}
