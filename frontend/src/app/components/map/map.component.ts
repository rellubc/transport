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

  stopSource: VectorSource | null = null;
  stopLayer: VectorLayer | null = null;
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

      this.stopSource?.clear()

      if (zoom && zoom > 18) {
        this.updateStops('Platform', false);
      } else if (zoom && zoom <= 18 && zoom >= 11) {
        this.updateStops('Station', false);
      } else if (zoom && zoom < 11) {
        this.updateStops('Station', true)
      }
    });
  }

  addShape = (shapeId: string, type?: string) => {
    const lineCoords = this.shapes[shapeId].map(s => fromLonLat([s.longitude, s.latitude]))
    
    let route: string
    if (shapeId === parseInt(shapeId).toString())
      route = 'M1'
    // else if (type === 'sydneytrains')
    else
      route = shapeId.split('_')[0]

    const lineFeature = new Feature({
      geometry: new LineString(lineCoords),
      propType: 'route',
    })
    
    const shapeSource = new VectorSource({
      features: [lineFeature]
    })

    const shapeLayer = new VectorLayer({
      source: shapeSource,
      style: new Style({
        stroke: new Stroke({
          color: coloursMap[route] || '#0000000',
          width: 2,
        }),
      }),
    })

    shapeLayer.set('name', shapeId.toString())
    this.map.addLayer(shapeLayer)
  }

  addStops(mode: string) {
    this.stopSource = new VectorSource({})
    this.stopLayer = new VectorLayer({
      source: this.stopSource,
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
    
    this.stopLayer.set('name', 'stops')
    this.map.addLayer(this.stopLayer)

    const zoom = this.map.getView().getZoom();
    if (zoom && zoom > 18) {
      this.updateStops('Platform');
    } else if (zoom && zoom <= 18 && zoom >= 11) {
      this.updateStops('Station');
    } else if (zoom && zoom < 11) {
      this.updateStops('Station', true)
    }
  }

  updateStops(type: string, regional?: boolean) {
    this.stops.forEach((stop, index) => {
      if (stop.locationType !== type) return
      // if (regional && stop.network !== 'regional') return
      const coord = fromLonLat([stop.longitude, stop.latitude])

      const feature = new Feature({
        geometry: new Point(coord),
        name: stop.name,
        id: stop.id,
        mode: stop.mode,
        propType: 'stop',
        index,
      });

      if (stop.name.includes('Platform')) {
        feature.set('platform', stop.name)
      } else {
        feature.set('station', stop.name)
      }

      this.stopSource!.addFeature(feature);
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
    // if (this.sidebarOpen) {
    //   if (this.propType === 'vehicle') {
    //     console.log('Updating current schedule...')

    //     this.currentVehicle = this.vehicles.find((vehicle) => vehicle.vehicle?.id === this.selectedProps.id)!
    //     if (this.selectedProps.mode === 'metro')
    //       this.currentRealtimeTrip = await getSydneyMetroTripUpdates(this.selectedProps.tripId)
    //     else if (this.selectedProps.mode === 'sydneytrains')
    //       this.currentRealtimeTrip = await getSydneyTrainsTripUpdates(this.selectedProps.tripId)
        
    //     console.log(this.currentRealtimeTrip)

    //     this.mapSidebar.updateBar()
    //   }
    // }
  }

  // async openSidebar(props: any) {
  //   if (this.sidebarOpen && this.currentVehicle?.vehicle?.id === props.id) return
  //   if (this.selectedProps && props.propType === this.propType && props.id === this.selectedProps.id) return

  //   console.log(props)
  //   console.log(this.selectedProps)
  //   this.resetSidebar()
    
  //   this.sidebarOpen = true
  //   this.selectedProps = props

  //   if (props.propType === 'vehicle') {
  //     if (props['id'] === "UNASSIGNED") return
  //     this.propType = 'vehicle'

  //     this.currentVehicle = this.vehicles.find((vehicle) => vehicle.vehicle?.id === this.selectedProps.id)!
  //     this.currentVehicle.consist = this.currentVehicle.trip?.directionId === 1 ? this.currentVehicle.consist?.reverse() : this.currentVehicle.consist


  //     if (this.selectedProps.type === 'metro') {
  //       this.currentRealtimeTrip = await getSydneyMetroTripUpdates(props.tripId)
  //       this.currentScheduledTrip = await getSydneyMetroTrip(this.selectedProps.tripId)
  //     } else {
  //       this.currentRealtimeTrip = await getSydneyTrainsTripUpdates(props.tripId)
  //       this.currentScheduledTrip = await getSydneyTrainsTrip(this.selectedProps.tripId)
  //     }
      
  //     console.log(this.currentVehicle)
  //     console.log(this.currentRealtimeTrip)
  //     console.log(this.currentScheduledTrip)
  //     this.updateBar()
  //   } else if (props.propType === 'stop') {
  //     this.propType = 'stop'

  //     if (this.selectedProps.mode === 'metro')
  //       this.currentStopScheduledServices = await getSydneyMetroStopTimes(this.selectedProps.name)
  //     else if (this.selectedProps.mode === 'sydneytrains')
  //       this.currentStopScheduledServices = await getSydneyTrainsStopTimes(this.selectedProps.name, new Date().toISOString(), false)

  //     console.log(this.currentStopScheduledServices)

  //     if (this.currentStopScheduledServices.length < 24) {
  //       this.preLoadingDone = true
  //       this.postLoadingDone = true
  //     }

  //     const container = document.querySelector('.sidebar-body-content') as HTMLElement
  //     container.addEventListener('scroll', async () => {
  //       console.log(this.currentStopScheduledServices.length)
  //       if (this.currentStopScheduledServices.length === 0) return
  //       if (container.scrollTop === 0) {
  //         const temp = await getSydneyTrainsStopTimes(this.selectedProps.name, this.currentStopScheduledServices[0].arrivalTime, true)
          
  //         console.log(temp)
  //         if (temp.length > 0)
  //           this.currentStopScheduledServices = temp.concat(this.currentStopScheduledServices)
  //         else
  //           this.preLoadingDone = true
  //         setTimeout(() => {
  //           container.scrollTop = 72 * temp.length
  //         }, 0)
  //       } else if (container.scrollTop === 72 * Math.max(0, (this.currentStopScheduledServices.length - 12)) + (this.preLoadingDone ? 62 : 110)) {
  //         const temp = await getSydneyTrainsStopTimes(this.selectedProps.name, this.currentStopScheduledServices[this.currentStopScheduledServices.length - 1].arrivalTime, false)

  //         console.log(temp)

  //         if (temp.length > 0)
  //           this.currentStopScheduledServices = this.currentStopScheduledServices.concat(temp)
  //         else
  //           this.postLoadingDone = true
  //       }
  //     })

  //     setTimeout(() => {
  //       container.scrollTop = 48
  //     }, 0)
  //   } else if (props.propType === 'route') {
  //     console.log('route')
  //   }
  // }

  // closeSidebar() {
  //   this.sidebarOpen = false
  //   this.resetSidebar()
  // }

  // resetSidebar() {
  //   console.log('here')
  //   this.selectedProps = null
  //   this.propType = ''
  //   this.currentVehicle = null
  //   this.currentRealtimeTrip = null
  //   this.currentScheduledTrip = null

  //   this.preLoadingDone = false
  //   this.postLoadingDone = false
  //   this.currentStopScheduledServices = []
  //   const container = document.querySelector('.sidebar-body-content') as HTMLElement;
  //   container?.removeEventListener('scroll');
  // }

  // getVehicleForTrip(tripId: string): VehiclePosition | undefined {
  //   return this.vehicles.find((vehicle) => vehicle.trip?.tripId === tripId)
  // }

  // updateBar() {
  //   const bar = document.querySelector('.sidebar-body-bar')! as HTMLElement;
  //   const barCover = document.querySelector('.sidebar-body-bar-cover')! as HTMLElement;

  //   bar.style.height = `${this.currentRealtimeTrip?.stopTimeUpdate.length! * 72 - 32}px`
    
  //   const currentTripStartSegment = this.stops.find((stop) => stop.id === this.currentVehicle!.stopId)?.id!
  //   const nextIndex = Number(this.currentRealtimeTrip?.stopTimeUpdate.findIndex((stop) => stop.stopId === currentTripStartSegment.toString())) + 1
  //   const currentTripEndSegment = this.stops.find((stop) => stop.id === this.currentRealtimeTrip?.stopTimeUpdate[nextIndex].stopId)?.id!

  //   const start = this.stops.find((stop) => stop.id === currentTripStartSegment!)!
  //   const end = this.stops.find((stop) => stop.id === currentTripEndSegment!)!

  //   let startBestIndex = 0;
  //   let startBestDiff = [Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][0].latitude - start.latitude!), Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][0].longitude - start.longitude!)];
  //   let endBestIndex = 0;
  //   let endBestDiff = [Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][0].latitude - end.latitude!), Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][0].longitude - end.longitude!)];

  //   for (let i = 1; i < this.shapes[this.currentScheduledTrip!.shapeId].length; i++) {
  //     const startDiff = [Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][i].latitude - start.latitude!), Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][i].longitude - start.longitude!)];
  //     if (startDiff[0] < startBestDiff[0] && startDiff[1] < startBestDiff[1]) {
  //       startBestIndex = i;
  //       startBestDiff = startDiff;
  //     }

  //     const endDiff = [Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][i].latitude - end.latitude!), Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][i].longitude - end.longitude!)];
  //     if (endDiff[0] < endBestDiff[0] && endDiff[1] < endBestDiff[1]) {
  //       endBestIndex = i;
  //       endBestDiff = endDiff;
  //     }
  //   }

  //   let currentBestIndex = startBestIndex;
  //   let currentBestDiff = [Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][startBestIndex].latitude - this.currentVehicle!.position!.latitude!), Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][startBestIndex].longitude - this.currentVehicle!.position!.longitude!)];

  //   for (let i = startBestIndex; i < endBestIndex; i++) {
  //     const currentDiff = [Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][i].latitude - this.currentVehicle!.position!.latitude!), Math.abs(this.shapes[this.currentScheduledTrip!.shapeId][i].longitude - this.currentVehicle!.position!.longitude!)];
  //     if (currentDiff[0] < currentBestDiff[0] && currentDiff[1] < currentBestDiff[1]) {
  //       currentBestIndex = i;
  //       currentBestDiff = currentDiff;
  //     }
  //   }

  //   const progress = (this.shapes[this.currentScheduledTrip!.shapeId][currentBestIndex].distanceTravelled - this.shapes[this.currentScheduledTrip!.shapeId][startBestIndex].distanceTravelled) / (this.shapes[this.currentScheduledTrip!.shapeId][endBestIndex].distanceTravelled - this.shapes[this.currentScheduledTrip!.shapeId][startBestIndex].distanceTravelled);
    
  //   const percent = progress * 100;
  //   const increment = 72
  //   const currentProgress = increment * (nextIndex - 1) + increment * (percent / 100) + 32

  //   barCover.style.height = `calc(${currentProgress}px)`
  // }

  // getOccupancyColour(status: number): string {
  //   if (status === 2 || status === 3) {
  //     return 'yellow'
  //   } else if (status === 4 || status === 5) {
  //     return 'red'
  //   } else if (status === 6) {
  //     return 'black'
  //   } else {
  //     return 'green'
  //   }
  // }

  // getRouteCode(routeId: string, mode: string) {
  //   if (mode === 'metro')
  //     return routesMap[routeId.split('_')[1]] 
  //   return routesMap[routeId.split('_')[0]] 
  // }

  // getCorrespondingRouteColour(routeId: string, mode: string) {
  //   if (mode === 'metro')
  //     return coloursMap[routeId.split('_')[1]] 
  //   return coloursMap[routeId.split('_')[0]]
  // }

  ngOnInit(): void {
    this.createMap()
  }
}
