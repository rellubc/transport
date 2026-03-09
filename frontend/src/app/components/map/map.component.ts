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
import { LucideAngularModule, X } from 'lucide-angular';
import { getMetroStopTimes, getMetroTrip, getTripUpdates } from '../../metro/metro-helpers';
import { TripUpdate, VehiclePosition } from '../../../shared/models/realtime';
import { Trip } from '../../../shared/models/trip';
import { StopTime } from '../../../shared/models/stopTime';

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

  currentVehicle: VehiclePosition | null = null
  currentRealtimeTrip: TripUpdate | null = null
  currentScheduledTrip: Trip | null = null

  currentStop: Stop | null = null
  currentStopScheduledServices: StopTime[] = []

  initialScrollTop: number = 0

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
        this.openSidebar(feature.getProperties())
      } else {
        this.closeSidebar()
      }
    })

    this.map.getView().on('change:resolution', () => {
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

  addShape = (shapes: Shape, shapeId: string) => {
    const lineCoordsFirstStart = shapes[shapeId].map(s => fromLonLat([s.longitude, s.latitude]))

    const lineFeatureFirstStart = new Feature({
      geometry: new LineString(lineCoordsFirstStart)
    })
    
    const shapeSourceFirstStart = new VectorSource({
      features: [lineFeatureFirstStart]
    })

    const shapeLayerFirstStart = new VectorLayer({
      source: shapeSourceFirstStart,
      style: new Style({
        stroke: new Stroke({
          color: '#168388',
          width: 2,
        }),
      }),
    })

    shapeLayerFirstStart.set('name', shapeId.toString())
    this.map.addLayer(shapeLayerFirstStart)
  }

  addStops() {
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
        name: stop.name.split(',')[0],
        id: stop.id,
        mode: stop.mode,
        propType: 'stop',
        index,
      });

      if (stop.name.split(',')[1]) {
        feature.set('platform', stop.name.split(',')[1])
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
        propType: 'vehicle',
      });

      this.vehicleSource!.addFeature(feature);
    });
  }

  async refresh() {
    console.log('Updating vehicle positions...')
    this.updateVehiclePositions()
    if (this.sidebarOpen) {
      if (this.propType === 'vehicle') {
        console.log('Updating current schedule...')

        this.currentVehicle = this.vehicles.find((vehicle) => vehicle.vehicle?.id === this.selectedProps.id)!
        this.currentRealtimeTrip = await getTripUpdates(this.selectedProps.tripId)

        this.updateBar()
      }
    }
  }

  async openSidebar(props: any) {
    if (this.sidebarOpen && this.currentVehicle?.vehicle?.id === props.id) return

    console.log(this.propType)
    console.log(props)
    if (props.propType !== this.propType) this.resetSidebar()
    
    this.sidebarOpen = true
    this.selectedProps = props

    if (props.propType === 'vehicle') {
      if (props['id'] === "UNASSIGNED") return
      this.propType = 'vehicle'

      this.currentVehicle = this.vehicles.find((vehicle) => vehicle.vehicle?.id === this.selectedProps.id)!
      this.currentVehicle.consist = this.currentVehicle.trip?.directionId === 1 ? this.currentVehicle.consist?.reverse() : this.currentVehicle.consist

      this.currentRealtimeTrip = await getTripUpdates(props.tripId)
      this.currentScheduledTrip = await getMetroTrip(this.selectedProps.tripId)
      
      console.log(this.currentVehicle)
      console.log(this.currentRealtimeTrip)
      console.log(this.currentScheduledTrip)
      this.updateBar()
    } else if (props.propType === 'stop') {
      console.log("here")
      console.log(this.stops.find((stop) => stop.id === props['id']))

      this.propType = 'stop'

      this.currentStop = this.stops.find((stop) => stop.id === props['id'])!
      this.currentStopScheduledServices = await getMetroStopTimes(this.selectedProps.id)

      console.log(this.currentStopScheduledServices)

      const now = new Date();

      const index = this.currentStopScheduledServices.findIndex((service) => {
        const time = new Date()
        const [h, m, s] = service.arrivalTime.split(':').map(Number)
        time.setHours(h, m, s, 0)
        return time > now
      })

      setTimeout(() => {
        this.scrollToIndex(index ?? 0)
      }, 0)
    }
  }

  closeSidebar() {
    this.sidebarOpen = false
    this.resetSidebar()
  }

  resetSidebar() {
    this.selectedProps = null
    this.propType = ''
    this.currentVehicle = null
    this.currentRealtimeTrip = null
    this.currentScheduledTrip = null
  }

  getVehicleForTrip(tripId: string): VehiclePosition | undefined {
    return this.vehicles.find((vehicle) => vehicle.trip?.tripId === tripId)
  }

  scrollToIndex(index: number) {
    const container = document.querySelector('.sidebar-body-content') as HTMLElement
    if (container) {
      const itemHeight = 72
      container.scrollTop = index * itemHeight
    }
  }

  updateBar() {
    const bar = document.querySelector('.sidebar-body-bar')! as HTMLElement;
    const barCover = document.querySelector('.sidebar-body-bar-cover')! as HTMLElement;

    bar.style.height = `${this.currentRealtimeTrip?.stopTimeUpdate.length! * 72 - 32}px`
    
    const currentTripStartSegment = this.stops.find((stop) => stop.id === this.currentVehicle!.stopId)?.id!
    const nextIndex = Number(this.currentRealtimeTrip?.stopTimeUpdate.findIndex((stop) => stop.stopId === currentTripStartSegment.toString())) + 1
    const currentTripEndSegment = this.stops.find((stop) => stop.id === this.currentRealtimeTrip?.stopTimeUpdate[nextIndex].stopId)?.id!

    const start = this.stops.find((stop) => stop.id === currentTripStartSegment!)!
    const end = this.stops.find((stop) => stop.id === currentTripEndSegment!)!

    let startBestIndex = 0;
    let startBestDiff = [Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][0].latitude - start.latitude!), Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][0].longitude - start.longitude!)];
    let endBestIndex = 0;
    let endBestDiff = [Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][0].latitude - end.latitude!), Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][0].longitude - end.longitude!)];

    for (let i = 1; i < this.shapes[this.currentScheduledTrip!.shapeId].length; i++) {
      const startDiff = [Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][i].latitude - start.latitude!), Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][i].longitude - start.longitude!)];
      if (startDiff[0] < startBestDiff[0] && startDiff[1] < startBestDiff[1]) {
        startBestIndex = i;
        startBestDiff = startDiff;
      }

      const endDiff = [Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][i].latitude - end.latitude!), Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][i].longitude - end.longitude!)];
      if (endDiff[0] < endBestDiff[0] && endDiff[1] < endBestDiff[1]) {
        endBestIndex = i;
        endBestDiff = endDiff;
      }
    }

    let currentBestIndex = startBestIndex;
    let currentBestDiff = [Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][startBestIndex].latitude - this.currentVehicle!.position!.latitude!), Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][startBestIndex].longitude - this.currentVehicle!.position!.longitude!)];

    for (let i = startBestIndex; i < endBestIndex; i++) {
      const currentDiff = [Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][i].latitude - this.currentVehicle!.position!.latitude!), Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][i].longitude - this.currentVehicle!.position!.longitude!)];
      if (currentDiff[0] < currentBestDiff[0] && currentDiff[1] < currentBestDiff[1]) {
        currentBestIndex = i;
        currentBestDiff = currentDiff;
      }
    }

    const progress = (this.shapes[this.currentScheduledTrip!.shapeId][currentBestIndex].distanceTravelled - this.shapes[this.currentScheduledTrip!.shapeId][startBestIndex].distanceTravelled) / (this.shapes[this.currentScheduledTrip!.shapeId][endBestIndex].distanceTravelled - this.shapes[this.currentScheduledTrip!.shapeId][startBestIndex].distanceTravelled);
    
    const percent = progress * 100;
    const increment = 72
    const currentProgress = increment * (nextIndex - 1) + increment * (percent / 100) + 32

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
