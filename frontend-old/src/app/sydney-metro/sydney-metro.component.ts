import { Component, ViewChild } from '@angular/core';
import { MapComponent } from '../components/map/map.component';

import { Stop } from '../../shared/models/stop';
import { Shapes } from '../../shared/models/shape';
import { VehiclePosition } from '../../shared/models/realtime';
import { ROUTE_TYPE_METRO, routeTypeMap } from '../../shared/models/constants';
import { getSydneyStops } from '../api/sydney-stops';
import { getSydneyShapes } from '../api/sydney-shapes';
import { getSydneyRealtimeVehicles } from '../api/sydney-realtime';

@Component({
  selector: 'app-sydney-metro',
  imports: [MapComponent],
  templateUrl: './sydney-metro.component.html',
  styleUrl: './sydney-metro.component.css'
})
export class SydneyMetroComponent {
  @ViewChild(MapComponent) map!: MapComponent

  stops: Stop[] = []
  shapes: Shapes = {}
  vehicles: VehiclePosition[] = []

  async ngOnInit(): Promise<void> {
    this.stops = await getSydneyStops(routeTypeMap[ROUTE_TYPE_METRO])
    this.map.stops = this.stops

    this.shapes = await getSydneyShapes(routeTypeMap[ROUTE_TYPE_METRO])
    this.map.shapes = this.shapes

    this.map.routeTypes[ROUTE_TYPE_METRO] = new Set()
    this.map.routeTypes[ROUTE_TYPE_METRO].add('M1')
    this.map.addShapeSource(ROUTE_TYPE_METRO)
    this.map.addStopSource(ROUTE_TYPE_METRO)
    this.map.addVehicleSource(ROUTE_TYPE_METRO)

    this.map.addShapes()
    this.map.addStops()

    this.vehicles = await getSydneyRealtimeVehicles(routeTypeMap[ROUTE_TYPE_METRO])
    this.map.vehicles = this.vehicles
    this.map.refresh()

    setInterval(async () => {
      this.vehicles = await getSydneyRealtimeVehicles(routeTypeMap[ROUTE_TYPE_METRO])
      this.map.vehicles = this.vehicles
      this.map.refresh()
    }, 15000)
  }
}
