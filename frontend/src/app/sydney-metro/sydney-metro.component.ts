import { Component, ViewChild } from '@angular/core';
import { MapComponent } from '../components/map/map.component';

import { getSydneyMetroShapes, getSydneyMetroStops, getSydneyMetroVehiclePositions } from './sydney-metro-helpers';
import { Stop } from '../../shared/models/stop';
import { Shape } from '../../shared/models/shape';
import { VehiclePosition } from '../../shared/models/realtime';
import { ROUTE_TYPE_METRO } from '../../shared/models/constants';

@Component({
  selector: 'app-sydney-metro',
  imports: [MapComponent],
  templateUrl: './sydney-metro.component.html',
  styleUrl: './sydney-metro.component.css'
})
export class SydneyMetroComponent {
  @ViewChild(MapComponent) map!: MapComponent

  stops: Stop[] = []
  shapes: Shape = {}
  vehicles: VehiclePosition[] = []

  async ngOnInit(): Promise<void> {
    this.stops = await getSydneyMetroStops()
    this.map.stops = this.stops

    this.shapes = await getSydneyMetroShapes()
    this.map.shapes = this.shapes

    this.map.routeTypes[ROUTE_TYPE_METRO] = new Set()
    this.map.routeTypes[ROUTE_TYPE_METRO].add('M1')
    this.map.addShapeSource(ROUTE_TYPE_METRO)
    this.map.addStopSource(ROUTE_TYPE_METRO)
    this.map.addVehicleSource(ROUTE_TYPE_METRO)

    this.map.addShapes()
    this.map.addStops()

    this.vehicles = await getSydneyMetroVehiclePositions()
    this.map.vehicles = this.vehicles
    this.map.refresh()
    console.log(this.vehicles)

    setInterval(async () => {
      this.vehicles = await getSydneyMetroVehiclePositions();
      this.map.vehicles = this.vehicles
      this.map.refresh()
      console.log(this.vehicles)
    }, 15000)
  }
}
