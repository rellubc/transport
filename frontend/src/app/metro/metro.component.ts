import { Component, ViewChild } from '@angular/core';
import { MapComponent } from '../components/map/map.component';

import { getMetroPlatforms, getMetroShapes, getMetroStations, getRealTimeStopTimes, getRealTimeVehiclePositions } from './metro-helpers';
import { StopPlatformDto, StopStationDto } from '../../shared/models/Stop';
import { Shape } from '../../shared/models/Shape';
import { RealtimeVehiclePositionDto } from '../../shared/models/Realtime';

@Component({
  selector: 'app-metro',
  imports: [MapComponent],
  templateUrl: './metro.component.html',
  styleUrl: './metro.component.css'
})
export class MetroComponent {
  @ViewChild(MapComponent) map!: MapComponent

  stations: StopStationDto[] = []
  platforms: StopPlatformDto[] = []
  shapes: Shape = {}
  shapeIds: number[] = []
  vehicles: RealtimeVehiclePositionDto[] = []

  async ngOnInit(): Promise<void> {
    this.platforms = await getMetroPlatforms()
    this.stations = await getMetroStations()
    this.shapes = await getMetroShapes()
    this.vehicles = await getRealTimeVehiclePositions();

    this.shapeIds = Object.keys(this.shapes).map(id => parseInt(id))

    this.map.addShape(this.shapes, this.shapeIds, 0, 1)
    this.map.addStops(this.stations)
    this.map.addStops(this.platforms)
    
    this.map.updateVehiclePositions(this.vehicles)
    
    setInterval(async () => {
      console.log('Updating vehicle positions...')
      this.vehicles = await getRealTimeVehiclePositions();
      this.map.updateVehiclePositions(this.vehicles)
    }, 15000)
  }
}
