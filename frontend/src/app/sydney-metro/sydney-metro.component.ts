import { Component, ViewChild } from '@angular/core';
import { MapComponent } from '../components/map/map.component';

import { getSydneyMetroShapes, getSydneyMetroStops, getSydneyMetroVehiclePositions } from './sydney-metro-helpers';
import { Stop } from '../../shared/models/stop';
import { Shape } from '../../shared/models/shape';
import { VehiclePosition } from '../../shared/models/realtime';

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

  METRO_FIRST_SHAPE: string = "3722"
  METRO_SECOND_SHAPE: string = "16714"

  async ngOnInit(): Promise<void> {
    this.stops = await getSydneyMetroStops()
    this.map.stops = this.stops
    this.shapes = await getSydneyMetroShapes()
    this.map.shapes = this.shapes

    this.map.addShape(this.METRO_FIRST_SHAPE, 'metro')
    this.map.addShape(this.METRO_SECOND_SHAPE, 'metro')
    this.map.addStops('metro')

    this.vehicles = await getSydneyMetroVehiclePositions()
    this.map.vehicles = this.vehicles
    this.map.refresh()

    setInterval(async () => {
      this.vehicles = await getSydneyMetroVehiclePositions();
      this.map.vehicles = this.vehicles
      this.map.refresh()
    }, 15000)
  }
}
