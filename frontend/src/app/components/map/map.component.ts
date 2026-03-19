import { Component, ElementRef, Input, ViewChild } from '@angular/core'
import { CommonModule } from '@angular/common'

import Map from 'ol/Map.js'
import View from 'ol/View'
import Tile from 'ol/layer/Tile'
import StadiaMaps from 'ol/source/StadiaMaps'

import Feature from 'ol/Feature'
import VectorSource from 'ol/source/Vector'
import VectorLayer from 'ol/layer/Vector'

import Style from 'ol/style/Style'
import Stroke from 'ol/style/Stroke'

import { LineString, Point } from 'ol/geom'
import { fromLonLat } from 'ol/proj'

import { Shape } from '../../../shared/models/shape'
import { Stop } from '../../../shared/models/stop'
import CircleStyle from 'ol/style/Circle'
import { Fill, Icon, Text } from 'ol/style'
import { getSydneyMetroStopsPlatforms, getSydneyMetroStopTimes, getSydneyMetroTrip, getSydneyMetroTripUpdates } from '../../sydney-metro/sydney-metro-helpers'
import { TripUpdate, VehiclePosition } from '../../../shared/models/realtime'
import { Trip } from '../../../shared/models/trip'
import { StopTime } from '../../../shared/models/stopTime'
import { getSydneyTrainsStops, getSydneyTrainsStopsPlatforms, getSydneyTrainsStopTimes, getSydneyTrainsTrip, getSydneyTrainsTripUpdates } from '../../sydney-trains/sydney-trains-helpers'
import { coloursMap, ROUTE_TYPE_METRO, ROUTE_TYPE_RAIL, routesMap, routeTypeMap } from '../../../shared/models/constants'
import { getDepartures } from './map-helpers'
import { MapSidebarComponent } from '../map-sidebar/map-sidebar.component'
import { Vector } from 'ol/source'

@Component({
  selector: 'app-map',
  imports: [CommonModule, MapSidebarComponent],
  templateUrl: './map.component.html',
  styleUrl: './map.component.css',
  standalone: true
})

export class MapComponent {
  @ViewChild(MapSidebarComponent) mapSidebar!: MapSidebarComponent

  @Input() stops: Stop[] = []
  @Input() vehicles: VehiclePosition[] = []
  @Input() shapes: Shape = {}
  
  map!: Map

  routeTypes: Record<number, Set<string>> = {}
  shapeSource: Record<string, VectorSource> = {}
  shapeLayer: Record<string, VectorLayer> = {}
  stopSource: Record<string, VectorSource> = {}
  stopLayer: Record<string, VectorLayer> = {}
  vehicleSource: Record<string, VectorSource> = {}
  vehicleLayer: Record<string, VectorLayer> = {}

  createMap() {    
    const center = localStorage.getItem('center') ? [Number(localStorage.getItem('center')!.split(',')[0]), Number(localStorage.getItem('center')!.split(',')[1])] : fromLonLat([151.05, -33.82])
    const zoom = localStorage.getItem('zoom') ? Number(localStorage.getItem('zoom')!) : 11.8
    this.map = new Map({
      view: new View({
        center,
        zoom,
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
    })

    this.map.on('singleclick', (evt) => {
      const feature = this.map.forEachFeatureAtPixel(evt.pixel, f => f)

      if (feature) {
        this.mapSidebar.openSidebar(feature.getProperties())
      } else {
        this.mapSidebar.closeSidebar()
      }
    })

    this.map.getView().on('change:resolution', () => {
      const center = this.map.getView().getCenter()!
      const zoom = this.map.getView().getZoom()!
      localStorage.setItem('zoom', zoom.toString())
      localStorage.setItem('center', center.toString())

      if (this.stopSource) Object.entries(this.stopSource!).forEach(([_mode, source]) => source.clear())

      this.addStops()
    })
  }

  addShapeSource(routeType: number) {
    this.shapeSource[routeTypeMap[routeType]] = new VectorSource({})
    this.shapeLayer[routeTypeMap[routeType]] = new VectorLayer({
      source: this.shapeSource[routeTypeMap[routeType]],
      style: (feature) => {
        const routeId: string = feature.get('routeId').split('_')[0]

        return new Style({
          stroke: new Stroke({
            color: coloursMap[routeId] || '#0000000',
            width: 2,
          }),
        })
      },
    })

    this.shapeLayer[routeTypeMap[routeType]].set('name', routeTypeMap[routeType])
    this.map.addLayer(this.shapeLayer[routeTypeMap[routeType]])
  }

  addStopSource(routeType: number) {
    this.stopSource[routeTypeMap[routeType]] = new VectorSource({})
    this.stopLayer[routeTypeMap[routeType]] = new VectorLayer({
      source: this.stopSource[routeTypeMap[routeType]],
      updateWhileAnimating: true,
      updateWhileInteracting: true,
      style: function (feature) {
        const layer = feature.get('stops')
        const map = layer ? layer.getMapInternal() : null
        const zoom = map ? map.getView().getZoom() : 11.8
        const scale = Math.max(0.05, 0.05 + (zoom - 11.8) * 0.02)
        return new Style({
          image: new Icon({
            src: `${routeTypeMap[routeType].toLowerCase()}.png`,
            scale: scale,
          }),
        })
      },
    })

    this.stopLayer[routeTypeMap[routeType]].set('name', 'stops')
    this.map.addLayer(this.stopLayer[routeTypeMap[routeType]])
  }

  addVehicleSource(routeType: number) {
    this.vehicleSource[routeTypeMap[routeType]] = new VectorSource({})
    this.vehicleLayer[routeTypeMap[routeType]] = new VectorLayer({
      source: this.vehicleSource[routeTypeMap[routeType]],
      zIndex: 9999,
      style: (feature) => {
        let routeId: string = ''

        if (routeType === ROUTE_TYPE_METRO) {
          routeId = feature.get('routeId').split('_')[1]
        } else if (routeType === ROUTE_TYPE_RAIL) {
          routeId = feature.get('routeId').split('_')[0]
        }

        let colourHex: string = coloursMap[routeId]

        return new Style({
          image: new CircleStyle({
            radius: 6,
            fill: new Fill({ color: colourHex }),
            stroke: new Stroke({ color: '#ffffff', width: 2 }),
          }),
        })
      },
    })

    this.vehicleLayer[routeTypeMap[routeType]].set('name', 'vehicles')
    this.map.addLayer(this.vehicleLayer[routeTypeMap[routeType]])
  }

  addShapes() {
    for (const routeType of Object.keys(this.routeTypes)) {
      Object.keys(this.shapes).forEach((shapeId) => {
        const lineCoords = this.shapes[shapeId].map(s => fromLonLat([s.longitude, s.latitude]))
        const feature = new Feature({
          geometry: new LineString(lineCoords),
          type: 'route',
          routeId: shapeId,
        })

        this.shapeSource[routeTypeMap[Number(routeType)]].addFeature(feature)
      })
    }
  }

  addStops() {
    for (const routeType of Object.keys(this.routeTypes)) {
      const zoom = this.map.getView().getZoom()
      let type: string
      let regional: boolean = zoom && zoom < 11 ? true : false
      if (zoom && zoom > 18) {
        type = 'Platform'
      } else if (zoom && zoom <= 18 && zoom >= 11) {
        type = 'Station'
      } else if (zoom && zoom < 18) {
        // for regional major stations?
      }

      const filteredStops = this.stops.filter((stop) => {
        if (type === 'Platform' && !stop.parentStationId) return false
        if (type === 'Station' && stop.parentStationId) return false
        return true
      })

      filteredStops.forEach((stop) => {
        const feature = new Feature({
          geometry: new Point(fromLonLat([stop.longitude, stop.latitude])),
          name: stop.name,
          id: stop.id,
          type: 'stop',
          mode: routeTypeMap[Number(routeType)],
        })

        this.stopSource[routeTypeMap[Number(routeType)]].addFeature(feature)
      })
    }
  }

  updateVehicles() {
    for (const routeType of Object.keys(this.routeTypes)) {
      this.vehicleSource[routeTypeMap[Number(routeType)]].clear()

      this.vehicles.forEach((vehicle, index) => {
        if (vehicle.position?.latitude == null || vehicle.position?.longitude == null) return
        if (vehicle.vehicle?.id == null) return
        if (vehicle.trip?.tripId == null) return

        const feature = new Feature({
          geometry: new Point(fromLonLat([vehicle.position.longitude, vehicle.position.latitude])),
          id: vehicle.vehicle.id,
          tripId: vehicle.trip.tripId,
          routeId: vehicle.trip.routeId,
          mode: routeTypeMap[Number(routeType)],
          type: 'vehicle',
        })

        this.vehicleSource[routeTypeMap[Number(routeType)]].addFeature(feature)
      })
    }
  }

  async refresh() {
    console.log('Updating vehicle positions...')
    this.updateVehicles()
    this.mapSidebar.refresh()
  }

  ngOnInit(): void {
    this.createMap()
  }
}
