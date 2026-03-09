import { Component, ViewChild } from '@angular/core';
import { MapComponent } from '../components/map/map.component';

import { getMetroShapes, getMetroStops, getVehiclePositions } from './metro-helpers';
import { Stop } from '../../shared/models/stop';
import { Shape } from '../../shared/models/shape';
import { VehiclePosition } from '../../shared/models/realtime';

@Component({
  selector: 'app-metro',
  imports: [MapComponent],
  templateUrl: './metro.component.html',
  styleUrl: './metro.component.css'
})
export class MetroComponent {
  @ViewChild(MapComponent) map!: MapComponent

  stops: Stop[] = []
  shapes: Shape = {}
  shapeIds: string[] = []
  vehicles: VehiclePosition[] = []

  METRO_FIRST_SHAPE: string = "3722"
  METRO_SECOND_SHAPE: string = "16714"

  async ngOnInit(): Promise<void> {
    this.stops = await getMetroStops()
    this.map.stops = this.stops
    this.shapes = await getMetroShapes()
    this.map.shapes = this.shapes

    this.shapeIds = Object.keys(this.shapes).map(id => id)

    this.map.addShape(this.shapes, this.METRO_FIRST_SHAPE)
    this.map.addShape(this.shapes, this.METRO_SECOND_SHAPE)
    this.map.addStops()

    this.vehicles = await getVehiclePositions()
    this.map.vehicles = this.vehicles
    this.map.refresh()

    setInterval(async () => {
      this.vehicles = await getVehiclePositions();
      this.map.vehicles = this.vehicles
      this.map.refresh()
    }, 15000)
  }
}
