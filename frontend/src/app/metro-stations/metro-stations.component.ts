import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { StopDto } from '../../shared/models/StopDto';

@Component({
  selector: 'app-metro-stations',
  imports: [CommonModule],
  templateUrl: './metro-stations.component.html',
  styleUrl: './metro-stations.component.css'
})
export class MetroStationsComponent {
  count = 0;
  stations: StopDto[] = []
  platforms: StopDto[] = []

  async getMetroStations() {
    try {
      const res = await fetch('https://localhost:7062/api/sydney/metro/stops')
      console.log('Response status:', res.status)
      console.log('Response headers:', res.headers)

      if (!res.ok) throw new Error(`HTTP ${res.status}`)

      const data: StopDto[] = await res.json()
      this.stations = data.filter((stop) => stop.locationType === "Station")
      this.platforms = data.filter((stop) => stop.locationType === "Platform")
    } catch (error) {
      console.error('Fetch failed:', error)
    }
  }

  increaseCount() {
    this.count++
  }

  ngOnInit(): void {
    this.getMetroStations()
  }
}
