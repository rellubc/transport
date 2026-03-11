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

    console.log(this.shapes)

    const metroLines = Object.keys(metroShapes).reduce<Record<string, string[]>>((acc, shapeId) => {
      if (!acc['M1']) acc['M1'] = []
      acc['M1'].push(shapeId)
      return acc
    }, {})

    const trainLines = Object.keys(trainShapes).reduce<Record<string, string[]>>((acc, shapeId) => {
      const line = shapeId.split('_')[0]
      if (!acc[line]) acc[line] = []
      acc[line].push(shapeId)
      return acc
    }, {})

    const lines = {
      ...metroLines,
      ...trainLines,
    }

    for (const [routeId, shapeIds] of Object.entries(lines)) {
      this.map.addShape(routeId, shapeIds)
    }

    this.map.addStops('metro', true)
    this.map.addStops('sydneytrains', true)

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
