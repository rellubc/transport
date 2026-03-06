import { Component, Input } from '@angular/core';
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
import { LucideAngularModule, X } from 'lucide-angular';
import { getMetroTrip, getStationStopTimes, getTripUpdates } from '../../metro/metro-helpers';
import { TripUpdate, VehiclePosition } from '../../../shared/models/realtime';
import { Trip } from '../../../shared/models/trip';

@Component({
  selector: 'app-map',
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './map.component.html',
  styleUrl: './map.component.css',
  standalone: true
})

export class MapComponent {
  @Input() stops: Stop[] = [];
  @Input() vehicles: VehiclePosition[] = [];
  @Input() shapes: Shape = {};

  readonly XIcon = X
  
  map!: Map

  stopSource: VectorSource | null = null;
  stopLayer: VectorLayer | null = null;
  vehicleSource: VectorSource | null = null;
  vehicleLayer: VectorLayer | null = null;

  sidebarOpen: boolean = false
  selectedProps: any = null
  propType: string = ''

  currentVehicleInfo: VehiclePosition | null = null
  currentVehicleSchedule: TripUpdate | null = null
  currentTripInfo: Trip | null = null
  currentTripEnd: number | null = null

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
        const props = feature.getProperties()

        this.openSidebar(props)
      } else {
        this.closeSidebar()
      }
    })

    this.map.getView().on('change:resolution', (evt) => {
      const center = this.map.getView().getCenter()!
      const zoom = this.map.getView().getZoom()!
      localStorage.setItem('zoom', zoom.toString())
      localStorage.setItem('center', center.toString())

      this.stopSource?.clear()

      if (zoom && zoom > 18) {
        this.updateStops('Platform');
      } else if (zoom && zoom <= 18) {
        this.updateStops('Station');
      }
    });
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
    this.stopSource = new VectorSource({})
    this.stopLayer = new VectorLayer({
      source: this.stopSource,
      updateWhileAnimating: true,
      updateWhileInteracting: true,
      style: function (feature) {
        const layer = feature.get('stops')
        const map = layer ? layer.getMapInternal() : null
        const zoom = map ? map.getView().getZoom() : 11.8
        const scale = Math.max(0.1, 0.1 + (zoom - 11.8) * 0.02)
        return new Style({
          image: new Icon({
            src: 'metro.png',
            scale: scale,
          }),
        });
      },
    });
    
    this.stopLayer.set('name', 'stops')
    this.map.addLayer(this.stopLayer)

    const zoom = this.map.getView().getZoom();
    if (zoom && zoom > 18) {
      this.updateStops('Platform');
    } else if (zoom && zoom <= 18) {
      this.updateStops('Station');
    }
  }

  updateStops(type: string) {
    this.stops.forEach((stop, index) => {
      if (stop.locationType !== type) return

      const coord = fromLonLat([stop.longitude, stop.latitude])

      const feature = new Feature({
        geometry: new Point(coord),
        name: stop.name,
        index
      });

      if (type === 'Platform') {
        feature.set('platformId', stop.id)
      } else if (type === 'Station') {
        feature.set('stationId', stop.id)
      }

      this.stopSource!.addFeature(feature);
    });
  }

  async updateVehiclePositions() {
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
            text: feature.get('id') || '',
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

    this.vehicles.forEach((vehicle, index) => {
      if (vehicle.position?.latitude == null || vehicle.position?.longitude == null) return
      if (vehicle.vehicle?.id == null) return
      if (vehicle.trip?.tripId == null) return

      const coord = fromLonLat([vehicle.position.longitude, vehicle.position.latitude]);

      const feature = new Feature({
        geometry: new Point(coord),
        id: vehicle.vehicle.id,
        tripId: vehicle.trip.tripId,
      });

      this.vehicleSource!.addFeature(feature);
    });
  }

  async refresh() {
    console.log('Updating vehicle positions...')
    this.updateVehiclePositions()
    if (this.sidebarOpen) {
      console.log('Updating current schedule...')

      this.currentVehicleInfo = this.vehicles.find((vehicle) => vehicle.vehicle?.id === this.selectedProps.id)!
      this.currentVehicleSchedule = await getTripUpdates(this.selectedProps.tripId)

      this.updateBar()
    }
  }

  async openSidebar(props: any) {
    if (this.sidebarOpen && this.currentVehicleInfo?.vehicle?.id === props.id) return

    console.log(props)
    
    this.selectedProps = props
    this.sidebarOpen = true

    if (props['id']) {
      this.propType = 'vehicle'
      this.currentVehicleInfo = this.vehicles.find((vehicle) => vehicle.vehicle?.id === this.selectedProps.id)!
      this.currentVehicleInfo.consist = this.currentVehicleInfo.trip?.directionId === 1 ? this.currentVehicleInfo.consist?.reverse() : this.currentVehicleInfo.consist
      this.currentVehicleSchedule = await getTripUpdates(props.tripId)
      this.currentTripInfo = await getMetroTrip(this.selectedProps.tripId)
      this.currentTripEnd = this.shapes[this.currentTripInfo!.shapeId][0].sequence === 0 ? this.shapes[this.currentTripInfo!.shapeId][this.shapes[this.currentTripInfo!.shapeId].length - 1].distanceTravelled : this.shapes[this.currentTripInfo!.shapeId][0].distanceTravelled
      this.updateBar()
    } else if (props['stopId']) {

    }
  }

  closeSidebar() {
    this.sidebarOpen = false
    this.selectedProps = null
    this.currentVehicleInfo = null
    this.currentTripInfo = null
    this.currentTripEnd = null
  }

  updateBar() {
    const bar = document.querySelector('.sidebar-body-bar')! as HTMLElement;
    const barCover = document.querySelector('.sidebar-body-bar-cover')! as HTMLElement;
    
    const currentTripStartSegment = this.stops.find((stop) => stop.id === Number(this.currentVehicleInfo!.stopId))?.id!
    const nextIndex = Number(this.currentVehicleSchedule?.stopTimeUpdate.findIndex((stop) => stop.stopId === currentTripStartSegment.toString())) + 1
    const currentTripEndSegment = this.stops.find((stop) => stop.id === Number(this.currentVehicleSchedule?.stopTimeUpdate[nextIndex].stopId))?.id!

    const start = this.stops.find((stop) => stop.id === currentTripStartSegment!)!
    const end = this.stops.find((stop) => stop.id === currentTripEndSegment!)!

    let startBestIndex = 0;
    let startBestDiff = [Math.abs(this.shapes[this.currentTripInfo!.shapeId][0].latitude - start.latitude!), Math.abs(this.shapes[this.currentTripInfo!.shapeId][0].longitude - start.longitude!)];
    let endBestIndex = 0;
    let endBestDiff = [Math.abs(this.shapes[this.currentTripInfo!.shapeId][0].latitude - end.latitude!), Math.abs(this.shapes[this.currentTripInfo!.shapeId][0].longitude - end.longitude!)];

    for (let i = 1; i < this.shapes[this.currentTripInfo!.shapeId].length; i++) {
      const startDiff = [Math.abs(this.shapes[this.currentTripInfo!.shapeId][i].latitude - start.latitude!), Math.abs(this.shapes[this.currentTripInfo!.shapeId][i].longitude - start.longitude!)];
      if (startDiff[0] < startBestDiff[0] && startDiff[1] < startBestDiff[1]) {
        startBestIndex = i;
        startBestDiff = startDiff;
      }

      const endDiff = [Math.abs(this.shapes[this.currentTripInfo!.shapeId][i].latitude - end.latitude!), Math.abs(this.shapes[this.currentTripInfo!.shapeId][i].longitude - end.longitude!)];
      if (endDiff[0] < endBestDiff[0] && endDiff[1] < endBestDiff[1]) {
        endBestIndex = i;
        endBestDiff = endDiff;
      }
    }

    if (this.currentTripInfo?.directionId === 0) {
      const tempBestIndex = startBestIndex;
      startBestIndex = endBestIndex;
      endBestIndex = tempBestIndex;
      const tempBestDiff = startBestDiff;
      startBestDiff = endBestDiff;
      endBestDiff = tempBestDiff;
    }

    let currentBestIndex = startBestIndex;
    let currentBestDiff = [Math.abs(this.shapes[this.currentTripInfo!.shapeId][startBestIndex].latitude - this.currentVehicleInfo!.position!.latitude!), Math.abs(this.shapes[this.currentTripInfo!.shapeId][startBestIndex].longitude - this.currentVehicleInfo!.position!.longitude!)];

    for (let i = startBestIndex; i < endBestIndex; i++) {
      const currentDiff = [Math.abs(this.shapes[this.currentTripInfo!.shapeId][i].latitude - this.currentVehicleInfo!.position!.latitude!), Math.abs(this.shapes[this.currentTripInfo!.shapeId][i].longitude - this.currentVehicleInfo!.position!.longitude!)];
      if (currentDiff[0] < currentBestDiff[0] && currentDiff[1] < currentBestDiff[1]) {
        currentBestIndex = i;
        currentBestDiff = currentDiff;
      }
    }

    if (this.currentTripInfo?.directionId === 0) {
      const tempBestIndex = startBestIndex;
      startBestIndex = endBestIndex;
      endBestIndex = tempBestIndex;
      const tempBestDiff = startBestDiff;
      startBestDiff = endBestDiff;
      endBestDiff = tempBestDiff;
    }

    const progress = (this.shapes[this.currentTripInfo!.shapeId][currentBestIndex].distanceTravelled - this.shapes[this.currentTripInfo!.shapeId][startBestIndex].distanceTravelled) / (this.shapes[this.currentTripInfo!.shapeId][endBestIndex].distanceTravelled - this.shapes[this.currentTripInfo!.shapeId][startBestIndex].distanceTravelled);
    const percent = progress * 100;
    const increment = parseInt(getComputedStyle(bar).getPropertyValue('height')) / 20
    const currentProgress = increment * (nextIndex - 1) + increment * (percent / 100)

    barCover.style.height = `calc(${currentProgress}px)`
  }

  getOccupancyColour(status: number): string {
    if (status === 2 || status === 3) {
      return 'yellow'
    } else if (status === 4 || status === 5) {
      return 'red'
    } else if (status === 6) {
      return 'black'
    } else {
      return 'green'
    }
  }

  ngOnInit(): void {
    this.createMap()
  }
}
