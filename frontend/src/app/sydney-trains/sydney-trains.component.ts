import { Component, ViewChild } from '@angular/core';
import { MapComponent } from '../components/map/map.component';
import { Stop } from '../../shared/models/stop';
import { Shape } from '../../shared/models/shape';
import { VehiclePosition } from '../../shared/models/realtime';
import { getSydneyTrainsShapes, getSydneyTrainsStops, getSydneyTrainsVehiclePositions } from './sydney-trains-helpers';
import { ROUTE_TYPE_RAIL, routesMap } from '../../shared/models/constants';

@Component({
  selector: 'app-sydney-trains',
  imports: [MapComponent],
  templateUrl: './sydney-trains.component.html',
  styleUrl: './sydney-trains.component.css'
})
export class SydneyTrainsComponent {
  @ViewChild(MapComponent) map!: MapComponent

  stops: Stop[] = []
  shapes: Shape = {}
  vehicles: VehiclePosition[] = []

  async ngOnInit(): Promise<void> {
    this.stops = await getSydneyTrainsStops()
    this.map.stops = this.stops

    this.shapes = await getSydneyTrainsShapes()
    this.map.shapes = this.shapes

    const routeSet = new Set(Object.keys(this.shapes).map(shapeId => shapeId.split('_')[0]))
    this.map.routeTypes[ROUTE_TYPE_RAIL] = new Set()

    for (const routeId of routeSet) {
      this.map.routeTypes[ROUTE_TYPE_RAIL].add(routeId)
      this.map.addShapeSource(ROUTE_TYPE_RAIL)
    }
    this.map.addStopSource(ROUTE_TYPE_RAIL)
    this.map.addVehicleSource(ROUTE_TYPE_RAIL)

    this.map.addShapes()
    this.map.addStops()

    this.vehicles = await getSydneyTrainsVehiclePositions()
    this.map.vehicles = this.vehicles
    this.map.refresh()

    setInterval(async () => {
      this.vehicles = await getSydneyTrainsVehiclePositions();
      this.map.vehicles = this.vehicles
      this.map.refresh()
    }, 15000)
  }
}
