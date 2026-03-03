import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

import Map from 'ol/Map.js'
import View from 'ol/View';
import Tile from 'ol/layer/Tile';
import StadiaMaps from 'ol/source/StadiaMaps';

import Feature from 'ol/Feature'
import VectorSource from 'ol/source/Vector'
import VectorLayer from 'ol/layer/Vector'

import Style from 'ol/style/Style'
import Stroke from 'ol/style/Stroke'

import { LineString, Point } from 'ol/geom'
import { fromLonLat } from 'ol/proj';

import { Shape } from '../../../shared/models/Shape';
import { Stop, StopPlatformDto, StopStationDto } from '../../../shared/models/Stop';
import CircleStyle from 'ol/style/Circle';
import { Fill, Text } from 'ol/style';
import { LucideAngularModule, X } from 'lucide-angular';
import { getRealTimeStopTimes, getStationStopTimes } from '../../metro/metro-helpers';
import { RealtimeVehiclePositionDto } from '../../../shared/models/Realtime';

@Component({
  selector: 'app-map',
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './map.component.html',
  styleUrl: './map.component.css',
  standalone: true
})

export class MapComponent {
  readonly XIcon = X
  
  map!: Map

  vehicleLayer: VectorLayer | null = null;
  vehicleSource: VectorSource | null = null;

  sidebarOpen: boolean = false
  selectedProps: any = null

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

    this.map.on('singleclick', (evt) => {
      const feature = this.map.forEachFeatureAtPixel(evt.pixel, f => f)

      if (feature) {
        const props = feature.getProperties()

        this.openSidebar(props)
      } else {
        this.closeSidebar()
      }
    })
  }

  addShape = (shapes: Shape, shapeIds: number[], fromTallawong: number, fromSydnenham: number) => {
    const lineCoordsTallawongStart = shapes[shapeIds[fromTallawong]].map(s => fromLonLat([s.longitude, s.latitude]))
    const lineCoordsSydenhamStart = shapes[shapeIds[fromSydnenham]].reverse().map(s => fromLonLat([s.longitude, s.latitude]))

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

    shapeLayerTallawongStart.set('name', shapeIds[fromTallawong].toString())
    shapeLayerSydenhamStart.set('name', shapeIds[fromSydnenham].toString())
    this.map.addLayer(shapeLayerTallawongStart)
    this.map.addLayer(shapeLayerSydenhamStart)
  }

  addStops(stops: Stop[]) {
    const stopSource = new VectorSource({})

    stops.forEach(stop => {
      const coord = fromLonLat([stop.longitude, stop.latitude]);

      const feature = new Feature({
        geometry: new Point(coord),
        stopId: stop.id,
        name: stop.name,
      });

      stopSource.addFeature(feature);
    });

    const stopLayer = new VectorLayer({
      source: stopSource,
      style: new Style({
        image: new CircleStyle({
          radius: 3,
          fill: new Fill({ color: '#168388' }),
          stroke: new Stroke({ color: '#ffffff', width: 1 }),
        }),
      }),
    });

    stopLayer.set('name', 'stops')
    this.map.addLayer(stopLayer)
  }

  updateVehiclePositions(vehicles: RealtimeVehiclePositionDto[]) {
    if (!this.vehicleSource) {
      this.vehicleSource = new VectorSource({})
      this.vehicleLayer = new VectorLayer({
        source: this.vehicleSource,
        zIndex: 9999,
        style: (feature) => new Style({
          image: new CircleStyle({
            radius: 6,
            fill: new Fill({ color: '#168388' }),
            stroke: new Stroke({ color: '#ffffff', width: 2 }),
          }),
          text: new Text({
            text: feature.get('vehicleId') || '',
            font: '12px Calibri,sans-serif',
            fill: new Fill({ color: '#000' }),
            stroke: new Stroke({ color: '#fff', width: 2 }),
            offsetY: -15
          }),
        }),
      });
      this.vehicleLayer.set('name', 'vehicles')
      this.map.addLayer(this.vehicleLayer)
    }

    this.vehicleSource.clear()

    vehicles.forEach((vehicle, index) => {
      const coord = fromLonLat([vehicle.longitude, vehicle.latitude]);

      const feature = new Feature({
        geometry: new Point(coord),
        vehicleId: vehicle.vehicleId,
        entityId: vehicle.entityId,
        tripId: vehicle.tripId,
      });

      this.vehicleSource!.addFeature(feature);
    });
  }

  async openSidebar(props: any) {
    console.log(props)
    this.selectedProps = props

    if (props["vehicleId"]) {
      console.log(props.tripId)
      const stopTimes = await getRealTimeStopTimes(props.tripId)
      console.log(stopTimes)
    } else if (props["stopId"]) {
      const stopTimes = await getStationStopTimes(props.stopId)
      console.log(stopTimes)
    }

    this.sidebarOpen = true
  }

  closeSidebar() {
    this.sidebarOpen = false
    this.selectedProps = null
  }

  ngOnInit(): void {
    this.createMap()
  }
}
