import { Component, ElementRef, Input, ViewChild } from '@angular/core';
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

import { Shape } from '../../../shared/models/shape';
import { Stop } from '../../../shared/models/stop';
import CircleStyle from 'ol/style/Circle';
import { Fill, Icon, Text } from 'ol/style';
import { getSydneyMetroStopsPlatforms, getSydneyMetroStopTimes, getSydneyMetroTrip, getSydneyMetroTripUpdates } from '../../sydney-metro/sydney-metro-helpers';
import { TripUpdate, VehiclePosition } from '../../../shared/models/realtime';
import { Trip } from '../../../shared/models/trip';
import { StopTime } from '../../../shared/models/stopTime';
import { getSydneyTrainsStops, getSydneyTrainsStopsPlatforms, getSydneyTrainsStopTimes, getSydneyTrainsTrip, getSydneyTrainsTripUpdates } from '../../sydney-trains/sydney-trains-helpers';
import { coloursMap, routesMap } from '../../../shared/models/constants';
import { getDepartures } from './map-helpers';
import { MapSidebarComponent } from '../map-sidebar/map-sidebar.component';

@Component({
  selector: 'app-map',
  imports: [CommonModule, MapSidebarComponent],
  templateUrl: './map.component.html',
  styleUrl: './map.component.css',
  standalone: true
})

export class MapComponent {
  @ViewChild(MapSidebarComponent) mapSidebar!: MapSidebarComponent

  @Input() stops: Stop[] = [];
  @Input() vehicles: VehiclePosition[] = [];
  @Input() shapes: Shape = {};
  
  map!: Map

  stopSource: Record<string, VectorSource> | null = null;
  stopLayer: Record<string, VectorLayer> | null = null;
  vehicleSource: VectorSource | null = null;
  vehicleLayer: VectorLayer | null = null;

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
    });

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

      Object.entries(this.stopSource!).forEach(([mode, source]) => source.clear())

      if (zoom && zoom > 18) {
        this.updateStops('Platform', false);
      } else if (zoom && zoom <= 18 && zoom >= 11) {
        this.updateStops('Station', false);
      } else if (zoom && zoom < 11) {
        this.updateStops('Station', true)
      }
    });
  }

  addShape = (routeId: string, shapeIds: string[]) => {
    const features: Feature<LineString>[] = shapeIds.map((shapeId) => {
      const lineCoords = this.shapes[shapeId].map(s => fromLonLat([s.longitude, s.latitude]))
      return new Feature({
        geometry: new LineString(lineCoords),
        propType: 'route',
      })
    })

    const shapeSource = new VectorSource({
      features,
    })

    const shapeLayer = new VectorLayer({
      source: shapeSource,
      style: new Style({
        stroke: new Stroke({
          color: coloursMap[routeId] || '#0000000',
          width: 2,
        }),
      }),
    })

    shapeLayer.set('name', routesMap[routeId])
    this.map.addLayer(shapeLayer)
  }

  addStops(mode: string, combined?: boolean) {
    if (!this.stopSource || !this.stopLayer) {
      this.stopSource = {}
      this.stopLayer = {}
    }
    this.stopSource[mode] = new VectorSource({})
    this.stopLayer[mode] = new VectorLayer({
      source: this.stopSource[mode],
      updateWhileAnimating: true,
      updateWhileInteracting: true,
      style: function (feature) {
        const layer = feature.get('stops')
        const map = layer ? layer.getMapInternal() : null
        const zoom = map ? map.getView().getZoom() : 11.8
        const scale = Math.max(0.05, 0.05 + (zoom - 11.8) * 0.02)
        return new Style({
          image: new Icon({
            src: `${mode}.png`,
            scale: scale,
          }),
        });
      },
    });
    
    this.stopLayer[mode].set('name', 'stops')
    this.map.addLayer(this.stopLayer[mode])

    const zoom = this.map.getView().getZoom();
    if (zoom && zoom > 18) {
      this.updateStops('Platform', combined);
    } else if (zoom && zoom <= 18 && zoom >= 11) {
      this.updateStops('Station', combined);
    } else if (zoom && zoom < 11) {
      this.updateStops('Station', combined, true)
    }
  }

  updateStops(type: string, combined?: boolean, regional?: boolean) {
    this.stops.forEach((stop, index) => {
      if (type === 'Platform' && stop.parentStationId === "") return
      if (type === 'Station' && stop.parentStationId !== "") return
      if (regional && stop.network !== 'regional') return
      if (!regional && stop.network === 'regional') return
      if (this.stopSource && !this.stopSource[stop.mode]) return
      const coord = fromLonLat([stop.longitude, stop.latitude])

      const feature = new Feature({
        geometry: new Point(coord),
        name: stop.name,
        id: stop.id,
        mode: combined ? 'combined' : stop.mode,
        propType: 'stop',
        index,
      });

      if (stop.name.includes('Platform')) {
        feature.set('platform', stop.name)
      } else {
        feature.set('station', stop.name)
      }
      if (!this.stopSource) return
      this.stopSource[stop.mode].addFeature(feature);
    });
  }

  updateVehiclePositions() {
    if (!this.vehicleSource) {
      this.vehicleSource = new VectorSource({})
      this.vehicleLayer = new VectorLayer({
        source: this.vehicleSource,
        zIndex: 9999,
        style: (feature) => {
          const type = feature.get('type')

          let routeId: string = ''

          if (type === 'metro')
            routeId = feature.get('routeId').split('_')[1]
          else 
            routeId = feature.get('routeId').split('_')[0]

          let colourHex: string = coloursMap[routeId]

          return new Style({
            image: new CircleStyle({
              radius: 6,
              fill: new Fill({ color: colourHex }),
              stroke: new Stroke({ color: '#ffffff', width: 2 }),
            }),
          })
        },
      });
      this.vehicleLayer.set('name', 'vehicles')
      this.map.addLayer(this.vehicleLayer)
    }

    this.vehicleSource.clear()

    this.vehicles.forEach((vehicle, index) => {
      if (vehicle.position?.latitude == null || vehicle.position?.longitude == null) return
      if (vehicle.vehicle?.id == null) return
      if (vehicle.trip?.tripId == null) return

      let type: string = ''

      if (vehicle.trip.routeId?.includes('M1')) type = 'metro'
      else type = 'sydneytrains'

      const coord = fromLonLat([vehicle.position.longitude, vehicle.position.latitude]);

      const feature = new Feature({
        geometry: new Point(coord),
        id: vehicle.vehicle.id,
        tripId: vehicle.trip.tripId,
        routeId: vehicle.trip.routeId,
        type: type,
        propType: 'vehicle',
      });

      this.vehicleSource!.addFeature(feature);
    });
  }

  async refresh() {
    console.log('Updating vehicle positions...')
    this.updateVehiclePositions()
    this.mapSidebar.refresh()
  }

  ngOnInit(): void {
    this.createMap()
  }
}
