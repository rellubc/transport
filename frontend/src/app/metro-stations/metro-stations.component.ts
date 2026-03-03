import { CommonModule } from '@angular/common'
import { Component, provideExperimentalZonelessChangeDetection } from '@angular/core'

import Map from 'ol/Map.js'
import View from 'ol/View.js'
import Tile from 'ol/layer/Tile.js'
import StadiaMaps from 'ol/source/StadiaMaps.js'
import Feature from 'ol/Feature'
import VectorSource from 'ol/source/Vector'
import VectorLayer from 'ol/layer/Vector'

import Style from 'ol/style/Style'
import CircleStyle from 'ol/style/Circle'
import Fill from 'ol/style/Fill'
import Stroke from 'ol/style/Stroke'

import { StopPlatformDto, StopStationDto } from '../../shared/models/Stop'
import { Shape, ShapeDto } from '../../shared/models/Shape'
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
  
  stations: StopStationDto[] = []
  platforms: StopPlatformDto[] = []
  shapes: Shape = {}
  shapeIds: number[] = []

  // index 0/2/6 is tallawong --> sydnenham
  // index 1/5/7 is sydnenham --> tallawong
  // index 3 is tallwong --> chatswood
  // index 4 is tallwong --> epping
  currentShapeIndex: number = 0 // testing
  fromTallawong: number = 0  
  fromSydnenham: number = 1

  map!: Map

  sidebarOpen: boolean = false
  selectedType: 'station' | 'platform' | null = null
  selectedProps: any = null

  nextShapeIndex() {
    this.map.getLayers().getArray()
      .filter(layer => layer.get('name') === this.shapeIds[this.currentShapeIndex].toString())
      .forEach(layer => this.map.removeLayer(layer));
  }

  async getMetroPlatforms() {
    try {
      const res = await fetch('https://localhost:7284/api/sydney/metro/platforms')
      if (!res.ok) throw new Error(`HTTP ${res.status}`)

      const data: StopPlatformDto[] = await res.json()
      this.platforms = data
    } catch (error) {
      console.error('Fetch failed:', error)
    }
  }

  async getMetroStations() {
    try {
      const res = await fetch('https://localhost:7284/api/sydney/metro/stations')
      if (!res.ok) throw new Error(`HTTP ${res.status}`)

      const data: StopStationDto[] = await res.json()
      this.stations = data
    } catch (error) {
      console.error('Fetch failed:', error)
    }
  }

  async getMetroShapes() {
    try {
      const res = await fetch('https://localhost:7284/api/sydney/metro/shapes')
      if (!res.ok) throw new Error(`HTTP ${res.status}`)

      const data: Shape = await res.json()

      this.shapes = data
      this.shapeIds = Object.keys(this.shapes).map(id => parseInt(id))
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

    // Handle clicks on features to open sidebar with details
    this.map.on('singleclick', (evt) => {
      const feature = this.map.forEachFeatureAtPixel(evt.pixel, f => f)

      console.log(feature)

      if (feature) {
        const props = feature.getProperties()
        
        console.log(props)

        if (props['stationId']) {
          this.openSidebar('station', props)
        } else if (props['platformId']) {
          this.openSidebar('platform', props)
        }
      } else {
        this.closeSidebar()
      }
    })
  }

  openSidebar(type: 'station' | 'platform', props: any) {
    this.selectedType = type
    this.selectedProps = props
    this.sidebarOpen = true
  }

  closeSidebar() {
    this.sidebarOpen = false
    this.selectedType = null
    this.selectedProps = null
  }

  addShape() {
    const lineCoordsTallawongStart = this.shapes[this.shapeIds[this.fromTallawong]].map(s => fromLonLat([s.longitude, s.latitude]))
    const lineCoordsSydenhamStart = this.shapes[this.shapeIds[this.fromSydnenham]].reverse().map(s => fromLonLat([s.longitude, s.latitude]))

    const lineFeatureTallawongStart = new Feature({
      geometry: new LineString(lineCoordsTallawongStart)
    })
    
    const shapeSourceTallawongStart = new VectorSource({
      features: [lineFeatureTallawongStart]
    })

    const shapeLayerTallawongStart = new VectorLayer({
      source: shapeSourceTallawongStart,
      style: new Style({
        stroke: new Stroke({
          // color: '#168388',
          color: '#FF00FF', // temp colour for testing
          width: 2,
        }),
      }),
    })


    const lineFeatureSydenhamStart = new Feature({
      geometry: new LineString(lineCoordsSydenhamStart)
    })
    
    const shapeSourceSydenhamStart = new VectorSource({
      features: [lineFeatureSydenhamStart]
    })

    const shapeLayerSydenhamStart = new VectorLayer({
      source: shapeSourceSydenhamStart,
      style: new Style({
        stroke: new Stroke({
          color: '#168388',
          width: 2,
        }),
      }),
    })

    shapeLayerTallawongStart.set('name', this.shapeIds[this.fromTallawong].toString())
    shapeLayerSydenhamStart.set('name', this.shapeIds[this.fromSydnenham].toString())
    this.map.addLayer(shapeLayerTallawongStart)
    this.map.addLayer(shapeLayerSydenhamStart)
  }

  addStations() {
    const stationSource = new VectorSource({})

    this.stations.forEach(station => {
      const coord = fromLonLat([station.longitude, station.latitude]);

      const feature = new Feature({
        geometry: new Point(coord),
        stationId: station.id,
        name: station.name,
      });

      stationSource.addFeature(feature);
    });

    const stationLayer = new VectorLayer({
      source: stationSource,
      style: new Style({
        image: new CircleStyle({
          radius: 6,
          fill: new Fill({ color: '#168388' }),
          stroke: new Stroke({ color: '#ffffff', width: 2 }),
        }),
      }),
    });

    const platformSource = new VectorSource({})

    this.platforms.forEach(platform => {
      const coord = fromLonLat([platform.longitude, platform.latitude]);

      const feature = new Feature({
        geometry: new Point(coord),
        platformId: platform.id,
        name: platform.name,
      });

      platformSource.addFeature(feature);
    });

    const platformLayer = new VectorLayer({
      source: platformSource,
      style: new Style({
        image: new CircleStyle({
          radius: 6,
          fill: new Fill({ color: '#168388' }),
          stroke: new Stroke({ color: '#ffffff', width: 2 }),
        }),
      }),
    });

    stationLayer.set('name', 'stations')
    platformLayer.set('name', 'platforms')
    this.map.addLayer(stationLayer)
    this.map.addLayer(platformLayer)
  }

  async ngOnInit(): Promise<void> {
    await this.getMetroPlatforms()
    await this.getMetroStations()
    await this.getMetroShapes()
    this.createMap()
    this.addShape()
    this.addStations()
  }
}
