import { Component, ViewChild } from '@angular/core';
import { getSydneyMetroShapes, getSydneyMetroStops, getSydneyMetroVehiclePositions } from '../sydney-metro/sydney-metro-helpers';
import { getSydneyTrainsShapes, getSydneyTrainsStops, getSydneyTrainsVehiclePositions } from '../sydney-trains/sydney-trains-helpers';
import { Stop } from '../../shared/models/stop';
import { Shape } from '../../shared/models/shape';
import { VehiclePosition } from '../../shared/models/realtime';
import { MapComponent } from '../components/map/map.component';

@Component({
  selector: 'app-sydney',
  imports: [MapComponent],
  templateUrl: './sydney.component.html',
  styleUrl: './sydney.component.css'
})
export class SydneyComponent {
  @ViewChild(MapComponent) map!: MapComponent

  stops: Stop[] = []
  shapes: Shape = {}
  vehicles: VehiclePosition[] = []

  METRO_FIRST_SHAPE: string = "3722"
  METRO_SECOND_SHAPE: string = "16714"

  async ngOnInit(): Promise<void> {
    const metroStops = await getSydneyMetroStops()
    const trainStops = await getSydneyTrainsStops()
    this.stops = metroStops.concat(trainStops)
    this.map.stops = this.stops

    const metroShapes = await getSydneyMetroShapes()
    const trainShapes = await getSydneyTrainsShapes()
    this.shapes = { ...metroShapes, ...trainShapes }
    this.map.shapes = this.shapes

    for (const [shapeId, shape] of Object.entries(this.shapes)) {
      this.map.addShape(shapeId, this.shapes[shapeId][0].mode)
    }

    console.log(this.stops)

    this.map.addStops('metro')
    this.map.addStops('sydneytrains')

    const metroVehicles = await getSydneyMetroVehiclePositions()
    const trainVehicles = await getSydneyTrainsVehiclePositions()
    this.vehicles = metroVehicles.concat(trainVehicles)
    this.map.vehicles = this.vehicles
    this.map.refresh()

    setInterval(async () => {
      const metroVehicles = await getSydneyMetroVehiclePositions()
      const trainVehicles = await getSydneyTrainsVehiclePositions()
      this.vehicles = metroVehicles.concat(trainVehicles)
      this.map.vehicles = this.vehicles
      this.map.refresh()
    }, 15000)
  }
}
