import { CommonModule } from '@angular/common'
import { Component, OnChanges, SimpleChanges } from '@angular/core'

import Map from 'ol/Map.js'
import View from 'ol/View.js'
import Tile from 'ol/layer/Tile.js'
import StadiaMaps from 'ol/source/StadiaMaps.js'
import Feature from 'ol/Feature'
import VectorSource from 'ol/source/Vector'
import VectorLayer from 'ol/layer/Vector'

import { StopDto } from '../../shared/models/StopDto'
import { ShapeDto } from '../../shared/models/ShapeDto'
import { fromLonLat } from 'ol/proj'
import { LineString, Point } from 'ol/geom'

@Component({
  selector: 'app-metro-stations',
  imports: [CommonModule],
  templateUrl: './metro-stations.component.html',
  styleUrl: './metro-stations.component.css'
})
export class MetroStationsComponent {
  count = 0
  
  stations: StopDto[] = []
  platforms: StopDto[] = []

  shapes: ShapeDto[] = []
  shapeIds: number[] = []
  currentShapeIndex: number = 0
  
  map!: Map

  nextShapeIndex() {
    this.map.getLayers().getArray()
      .filter(layer => layer.get('name') === this.shapeIds[this.currentShapeIndex].toString())
      .forEach(layer => this.map.removeLayer(layer));
    
    this.currentShapeIndex++
    if (this.currentShapeIndex == this.shapeIds.length) this.currentShapeIndex = 0

    this.populateMap()
  }

  async getMetroStations() {
    try {
      const res = await fetch('https://localhost:7062/api/sydney/metro/stops')
      if (!res.ok) throw new Error(`HTTP ${res.status}`)

      const data: StopDto[] = await res.json()
      this.stations = data.filter((stop) => stop.locationType === "Station")
      this.platforms = data.filter((stop) => stop.locationType === "Platform")
    } catch (error) {
      console.error('Fetch failed:', error)
    }
  }

  async getMetroShapes() {
    try {
      const res = await fetch('https://localhost:7062/api/sydney/metro/shapes')
      if (!res.ok) throw new Error(`HTTP ${res.status}`)

      const data: ShapeDto[] = await res.json()
      this.shapes = data
      this.shapeIds = Array.from(new Set(this.shapes.map(shape => shape.id)))
      
      this.createMap()
      this.populateMap()
    } catch (error) {
      console.error('Fetch failed:', error)
    }
  }

  createMap() {
    this.map = new Map({
      view: new View({
        center: fromLonLat([151.05, -33.82]),
        zoom: 11.8,
      }),
      layers: [
        new Tile({
          source: new StadiaMaps({
            layer: 'alidade_smooth',
            retina: true,
          }),
        }),
      ],
      target: 'map',
      controls: []
    });
  }

  populateMap() {
    const shapeSource = new VectorSource({
      // features: shapeFeatures
    })

    const sorted = [...this.shapes].sort((a, b) => {
      if (a.id !== b.id) {
        return a.id - b.id
      }

      return a.sequence - b.sequence
    })
    
    const lineCoords = sorted.filter((item) => item.id === this.shapeIds[this.currentShapeIndex]).map(s => fromLonLat([s.longitude, s.latitude]))

    const lineFeature = new Feature({
      geometry: new LineString(lineCoords)
    })

    shapeSource.addFeature(lineFeature)

    const shapeLayer = new VectorLayer({
      source: shapeSource,
    })
    shapeLayer.set('name', this.shapeIds[this.currentShapeIndex].toString())
    this.map.addLayer(shapeLayer)
  }

  ngOnInit(): void {
    this.getMetroStations()
    this.getMetroShapes()
  }
}
