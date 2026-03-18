import { Component, ViewChild } from '@angular/core';
import { MapComponent } from '../components/map/map.component';
import { Stop } from '../../shared/models/stop';
import { Shape } from '../../shared/models/shape';
import { VehiclePosition } from '../../shared/models/realtime';
import { getSydneyTrainsShapes, getSydneyTrainsStops, getSydneyTrainsVehiclePositions } from './sydney-trains-helpers';

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

    console.log(this.stops)
    console.log(this.shapes)

    // const lines = Object.keys(this.shapes).reduce<Record<string, string[]>>((acc, shapeId) => {
    //   const line = shapeId.split('_')[0]
    //   if (!acc[line]) acc[line] = []
    //   acc[line].push(shapeId)
    //   return acc
    // }, {})

    // for (const [routeId, shapeIds] of Object.entries(lines)) {
    //   this.map.addShape(routeId, shapeIds)
    // }

    // this.map.addStops('sydneytrains')

    // this.vehicles = await getSydneyTrainsVehiclePositions()
    // this.map.vehicles = this.vehicles
    // this.map.refresh()

    // setInterval(async () => {
    //   this.vehicles = await getSydneyTrainsVehiclePositions();
    //   this.map.vehicles = this.vehicles
    //   this.map.refresh()
    // }, 15000)
  }
}
