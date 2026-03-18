import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { coloursMap, routesMap, vehiclesMap } from '../../../shared/models/constants';
import { CommonModule } from '@angular/common';
import { StopTimeUpdate, TripUpdate, VehiclePosition } from '../../../shared/models/realtime';
import { Trip } from '../../../shared/models/trip';
import { Stop } from '../../../shared/models/stop';
import { StopTime } from '../../../shared/models/stopTime';
import { GalleryThumbnailsIcon, LucideAngularModule, X } from 'lucide-angular';
import { getSydneyMetroStopTimes, getSydneyMetroTrip, getSydneyMetroTripStopTimes, getSydneyMetroTripUpdates } from '../../sydney-metro/sydney-metro-helpers';
import { getSydneyTrainsStopTimes, getSydneyTrainsTrip, getSydneyTrainsTripStopTimes, getSydneyTrainsTripUpdates } from '../../sydney-trains/sydney-trains-helpers';
import { Shape } from '../../../shared/models/shape';
import { getSydneyCombinedStopTimes } from '../../sydney/sydney-helpers';

@Component({
  selector: 'app-map-sidebar',
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './map-sidebar.component.html',
  styleUrl: './map-sidebar.component.css'
})
export class MapSidebarComponent {
  @Input() shapes: Shape = {};
  @Input() stops: Stop[] = [];
  @Input() vehicles: VehiclePosition[] = [];

  readonly XIcon = X
  
  open: boolean = false

  currentProps: any = null

  currentVehicle: VehiclePosition | null = null
  currentScheduledTrip: Trip | null = null
  currentScheduledTripStopTimes: StopTime[] = []
  currentRealtimeTrip: TripUpdate | null = null

  currentScheduledStopTimes: StopTime[] = []
  container: HTMLElement | null = null

  preLoadingDone: boolean = false
  postLoadingDone: boolean = false

  async refresh() {
    if (!this.open) return

    if (this.currentProps.type === 'vehicle') {
      console.log('Updating current schedule...')

      this.currentVehicle = this.vehicles.find((vehicle) => vehicle.vehicle?.id === this.currentProps.id)!

      this.updateTripStopTimes()
      this.updateBar()
    } else if (this.currentProps.type === 'stop') {
      console.log('Updating current departures...')

      this.currentScheduledStopTimes = await getSydneyMetroStopTimes(this.currentProps.name, new Date().toISOString(), false)

      console.log(this.currentScheduledStopTimes)
    }
  }

  async openSidebar(incomingProps: any) {
    if (this.currentProps && incomingProps.id === this.currentProps.id) return

    // edge case Light Rail service on metro api
    if (incomingProps.id == "UNASSIGNED") return

    this.resetSidebar()
    
    this.open = true
    this.currentProps = incomingProps
    if (this.currentProps.type === 'vehicle') {
      this.currentVehicle = this.vehicles.find((vehicle) => vehicle.vehicle?.id === this.currentProps.id)!

      if (this.currentProps.mode === 'Metro') {
        this.currentScheduledTrip = await getSydneyMetroTrip(incomingProps.tripId)
        this.currentScheduledTripStopTimes = await getSydneyMetroTripStopTimes(incomingProps.tripId, new Date().toISOString())
      }
      else if (this.currentProps.mode === 'sydneytrains') {
        console.log('rail')
      }
      
      this.updateTripStopTimes()
    } else if (this.currentProps.type === 'stop') {
      console.log('stop')

      if (this.currentProps.mode === 'Metro') {
        this.currentScheduledStopTimes = await getSydneyMetroStopTimes(this.currentProps.name, new Date().toISOString(), false)
      }

      console.log(this.currentScheduledStopTimes)

      if (this.currentScheduledStopTimes.length < 24) {
        this.preLoadingDone = true
        this.postLoadingDone = true
      }

      this.container = document.querySelector('.sidebar-body-content') as HTMLElement
      this.container.addEventListener('scroll', this.stopScroller)

      setTimeout(() => {
        if (!this.container) return
        this.container.scrollTop = 48
      }, 0)
    }
  }

  closeSidebar() {
    this.open = false
    this.resetSidebar()
  }

  resetSidebar() {
    this.currentProps = null

    this.currentVehicle = null
    this.currentScheduledTrip = null
    this.currentScheduledTripStopTimes = []
    this.currentRealtimeTrip = null

    this.currentScheduledStopTimes = []
  
    this.preLoadingDone = false
    this.postLoadingDone = false
  }

  async updateTripStopTimes() {
    if (!this.currentVehicle || !this.currentScheduledTrip) return

    if (this.currentProps.mode === 'Metro') {
      this.currentRealtimeTrip = await getSydneyMetroTripUpdates(this.currentProps.tripId)
      // todo: assign realtime trip times to scheduled trip times then calculate delays
    } else if (this.currentProps.mode === 'sydneytrains') {
      console.log('rail')
    }

    if (!this.currentRealtimeTrip) return
    const realtimeTrip = this.currentRealtimeTrip
    this.currentScheduledTripStopTimes.forEach((stopTime) => {
      let arrival = false
      const realtimeStopTime = realtimeTrip.stopTimeUpdate.find((stopTimeUpdate) => stopTimeUpdate.stopId === stopTime.stopId)
      
      if (!realtimeStopTime) return
      if (!realtimeStopTime.departure?.time) arrival = true
      
      // possibly departure time delay not accounted for
      const pad = (n: number) => String(n).padStart(2, "0");
      let realtimeString: string
      if (arrival) {
        realtimeString = pad(realtimeStopTime.arrival?.time!.getHours()!) + ":" + pad(realtimeStopTime.arrival?.time!.getMinutes()!) + ":" + pad(realtimeStopTime.arrival?.time!.getSeconds()!)
      } else {
        realtimeString = pad(realtimeStopTime.departure?.time!.getHours()!) + ":" + pad(realtimeStopTime.departure?.time!.getMinutes()!) + ":" + pad(realtimeStopTime.departure?.time!.getSeconds()!)
      }

      stopTime.departureTime = realtimeString
      stopTime.delay = realtimeStopTime.departure?.delay || realtimeStopTime.arrival?.delay
    })

    this.updateBar()
  }

  updateBar() {
    const vehicle = this.currentVehicle
    const realtime = this.currentRealtimeTrip
    const trip = this.currentScheduledTrip

    if (!vehicle || !trip || !realtime) return

    const segmentStartStopTime = this.currentScheduledTripStopTimes[vehicle.currentStopSequence! - 1]
    const segmentEndStopTime = this.currentScheduledTripStopTimes[vehicle.currentStopSequence!]

    const start = this.stops.find((stop) => stop.id === segmentStartStopTime.stopId)!
    const end = this.stops.find((stop) => stop.id === segmentEndStopTime.stopId)!

    const shape = this.shapes[trip.shapeId]

    let startBestIndex = 0;
    let startBestDiff = [Math.abs(shape[0].latitude - start.latitude!), Math.abs(shape[0].longitude - start.longitude!)];
    let endBestIndex = 0;
    let endBestDiff = [Math.abs(shape[0].latitude - end.latitude!), Math.abs(shape[0].longitude - end.longitude!)];
  
    for (let i = 1; i < shape.length; i++) {
      const startDiff = [Math.abs(shape[i].latitude - start.latitude!), Math.abs(shape[i].longitude - start.longitude!)];
      if (startDiff[0] < startBestDiff[0] && startDiff[1] < startBestDiff[1]) {
        startBestIndex = i;
        startBestDiff = startDiff;
      }

      const endDiff = [Math.abs(shape[i].latitude - end.latitude!), Math.abs(shape[i].longitude - end.longitude!)];
      if (endDiff[0] < endBestDiff[0] && endDiff[1] < endBestDiff[1]) {
        endBestIndex = i;
        endBestDiff = endDiff;
      }
    }

    let currentBestIndex = startBestIndex;
    let currentBestDiff = [Math.abs(shape[startBestIndex].latitude - vehicle.position!.latitude!), Math.abs(shape[startBestIndex].longitude - vehicle.position!.longitude!)];

    for (let i = startBestIndex; i < endBestIndex; i++) {
      const currentDiff = [Math.abs(shape[i].latitude - vehicle.position!.latitude!), Math.abs(shape[i].longitude - vehicle.position!.longitude!)];
      if (currentDiff[0] < currentBestDiff[0] && currentDiff[1] < currentBestDiff[1]) {
        currentBestIndex = i;
        currentBestDiff = currentDiff;
      }
    }

    const progress = this.currentProps.type === 'metro'
      ? (this.shapes[trip.shapeId][currentBestIndex].distanceTravelled - this.shapes[trip.shapeId][startBestIndex].distanceTravelled) / (this.shapes[trip.shapeId][endBestIndex].distanceTravelled - this.shapes[trip.shapeId][startBestIndex].distanceTravelled)
      : (this.shapes[trip.shapeId][currentBestIndex].sequence - this.shapes[trip.shapeId][startBestIndex].sequence) / (this.shapes[trip.shapeId][endBestIndex].sequence - this.shapes[trip.shapeId][startBestIndex].sequence)

    const percent = progress * 100;
    const increment = 72
    const currentProgress = increment * (vehicle.currentStopSequence! - 1) + increment * (percent / 100) + 32
    
    const bar = document.querySelector('.sidebar-body-bar')! as HTMLElement;
    const barCover = document.querySelector('.sidebar-body-bar-cover')! as HTMLElement;

    bar.style.height = `${this.currentScheduledTripStopTimes.length * 72 - 32}px`
    barCover.style.height = `calc(${currentProgress}px)`
  }

  stopScroller = async () => {
    if (!this.container) return
    if (this.currentScheduledStopTimes.length === 0) return

    if (this.container.scrollTop === 0) {
      let temp: StopTime[] = []
      if (this.currentProps.mode === 'Metro')
        temp = await getSydneyMetroStopTimes(this.currentProps.name, this.currentScheduledStopTimes[0].arrivalTime, true)
      else if (this.currentProps.mode === 'sydneytrains')
        temp = await getSydneyTrainsStopTimes(this.currentProps.name, this.currentScheduledStopTimes[0].arrivalTime, true)
      else if (this.currentProps.mode === 'combined') {
        temp = await getSydneyCombinedStopTimes(this.currentProps.name, this.currentScheduledStopTimes[0].arrivalTime, true)
        console.log(temp)
      }
      // const temp = await getSydneyTrainsStopTimes(this.currentProps.name, this.currentScheduledStopTimes[0].arrivalTime, true)

      if (temp.length > 0)
        this.currentScheduledStopTimes = temp.concat(this.currentScheduledStopTimes)
      else
        this.preLoadingDone = true
      setTimeout(() => {
        if (!this.container) return
        this.container.scrollTop = 72 * temp.length
      }, 0)
    } else if (this.container.scrollTop === 72 * Math.max(0, (this.currentScheduledStopTimes.length - 12)) + (this.preLoadingDone ? 116 : 164)) {
      let temp: StopTime[] = []
      if (this.currentProps.mode === 'Metro')
        temp = await getSydneyMetroStopTimes(this.currentProps.name, this.currentScheduledStopTimes[this.currentScheduledStopTimes.length - 1].arrivalTime, false)
      else if (this.currentProps.mode === 'sydneytrains')
        temp = await getSydneyTrainsStopTimes(this.currentProps.name, this.currentScheduledStopTimes[this.currentScheduledStopTimes.length - 1].arrivalTime, false)
      else if (this.currentProps.mode === 'combined') {
        temp = await getSydneyCombinedStopTimes(this.currentProps.name, this.currentScheduledStopTimes[this.currentScheduledStopTimes.length - 1].arrivalTime, false)
        console.log(temp)
      }
      // const temp = await getSydneyTrainsStopTimes(this.currentProps.name, this.currentScheduledStopTimes[this.currentScheduledStopTimes.length - 1].arrivalTime, false)

      if (temp.length > 0)
        this.currentScheduledStopTimes = this.currentScheduledStopTimes.concat(temp)
      else
        this.postLoadingDone = true
    }
  }

  getVehicleForTrip(tripId: string): VehiclePosition | undefined {
    return this.vehicles.find((vehicle) => vehicle.trip?.tripId === tripId)
  }

  getVehicleModel(vehicleModel: string, mode: string) {
    if (mode === 'Metro')
      return vehiclesMap[vehicleModel] 
    return vehiclesMap[vehicleModel] 
  }

  getDepartureDate(time: string) {
    return new Date(`1970-01-01T${time}`);
  }

  getDelay(delay: number) {
    const floored = delay < 0 ? Math.ceil(delay / 60) : Math.floor(delay / 60)

    if (floored < 0) return `${Math.abs(floored)} mins early`
    else if (floored > 0) return `${Math.abs(floored)} mins late`
    else return 'On time'
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

  getRouteCode(routeId: string, mode: string) {
    if (mode === 'Metro')
      return routesMap[routeId.split('_')[1]] 
    return routesMap[routeId.split('_')[0]] 
  }

  getCorrespondingRouteColour(routeId: string, mode: string) {
    if (mode === 'Metro')
      return coloursMap[routeId.split('_')[1]] 
    return coloursMap[routeId.split('_')[0]]
  }
}
