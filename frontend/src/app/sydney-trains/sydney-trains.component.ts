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

    for (const shapeId of Object.keys(this.shapes)) {
      this.map.addShape(shapeId, 'sydneytrains')
    }

    this.map.addStops('sydneytrains')

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
