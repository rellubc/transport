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
import { Shapes } from '../../../shared/models/shape';
// import { getSydneyCombinedStopTimes } from '../../sydney/sydney-helpers';
import { getCurrentScheduledTrip } from './map-sidebar-helpers';
import { getSydneyTrip, getSydneyTripStopTimes } from '../../api/sydney-trips';
import { getSydneyStopTimes } from '../../api/sydney-stop-times';
import { getSydneyRealtimeTripStopTimes, getSydneyTripUpdates } from '../../api/sydney-realtime-tripupdates';

@Component({
  selector: 'app-map-sidebar',
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './map-sidebar.component.html',
  styleUrl: './map-sidebar.component.css'
})
export class MapSidebarComponent {
  @Input() shapes: Shapes = {};
  @Input() stops: Stop[] = [];
  @Input() vehicles: VehiclePosition[] = [];

  readonly XIcon = X
  
  open: boolean = false

  props: any = null

  vehicle: VehiclePosition | null = null
  
  scheduledTrip: Trip | null = null
  realtimeTrip: TripUpdate | null = null
  stopTimes: StopTime[] = []

  container: HTMLElement | null = null

  preLoadingDone: boolean = false
  postLoadingDone: boolean = false

  async refresh() {
    // if (!this.open) return

    // if (this.props.type === 'vehicle') {
    //   console.log('Updating current schedule...')

    //   this.vehicle = this.vehicles.find((vehicle) => vehicle.vehicle?.id === this.props.id)!

    //   this.updateTripStopTimes()
    //   this.updateBar()
    // } else if (this.props.type === 'stop') {
    //   console.log('Updating current departures...')

    //   this.stopTimes = await getSydneyMetroStopTimes(this.props.name, new Date().toISOString(), false)
    // }
  }

  async openSidebar(incomingProps: any) {
    console.log("Incoming Props: ",incomingProps)

    if (this.props && incomingProps.id === this.props.id) return

    // edge case Light Rail service on metro api
    if (incomingProps.id == "UNASSIGNED") return

    this.resetSidebar()
    
    this.open = true
    this.props = incomingProps

    if (this.props.type === 'vehicle') {
      this.vehicle = this.vehicles.find((vehicle) => vehicle.vehicle?.id === this.props.id)!
      console.log("Current Vehicle: ", this.vehicle)

      this.scheduledTrip = await getSydneyTrip(this.props.tripId)
      console.log("Current Trip Details: ", this.scheduledTrip)
      
      this.updateTripStopTimes()
    } else if (this.props.type === 'stop') {

      // map current stop times for corresponding vehicle to the current stop
      this.stopTimes = await getSydneyStopTimes(this.props.mode, this.props.name, new Date().toISOString(), false)
      console.log("Current Stop Times: ", this.stopTimes)

      if (this.stopTimes.length < 24) {
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
    this.props = null

    this.vehicle = null
    this.scheduledTrip = null
    this.realtimeTrip = null

    this.stopTimes = []
  
    this.preLoadingDone = false
    this.postLoadingDone = false
  }

  async updateTripStopTimes() {
    if (!this.vehicle || !this.scheduledTrip) return

    this.realtimeTrip = await getSydneyTripUpdates(this.props.mode, this.props.tripId)
    this.stopTimes = await getSydneyRealtimeTripStopTimes(this.props.mode, this.props.tripId, new Date().toISOString())

    console.log("Current Real Time Trip: ", this.realtimeTrip)
    console.log("Current Trip Stop Times: ", this.stopTimes)

    this.updateBar()
  }

  updateBar() {
    const vehicle = this.vehicle
    const realtimeTrip = this.realtimeTrip
    const scheduledTrip = this.scheduledTrip
    const stopTimes = this.stopTimes

    if (!vehicle || !realtimeTrip || !scheduledTrip || !stopTimes) return

    let segmentStartStopTime: StopTime
    let segmentEndStopTime: StopTime
    let stopSequenceProgress: number
    if (this.props.mode === 'Metro') {
      segmentStartStopTime = stopTimes[vehicle.currentStopSequence! - 1]
      segmentEndStopTime = stopTimes[vehicle.currentStopSequence!]
      stopSequenceProgress = vehicle.currentStopSequence! - 1
    // } else if (this.props.mode === 'Rail') {
    } else {
      const index = Math.max(0, stopTimes.length - realtimeTrip.stopTimeUpdate.length - 1)
      segmentStartStopTime = stopTimes[index]
      segmentEndStopTime = stopTimes[index + 1]
      stopSequenceProgress = index
    }

    const start = this.stops.find((stop) => stop.id === segmentStartStopTime.stopId)!
    const end = this.stops.find((stop) => stop.id === segmentEndStopTime.stopId)!

    console.log(segmentStartStopTime, segmentEndStopTime)

    const shape = this.shapes[scheduledTrip.shapeId]

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

    const progress = this.props.type === 'metro'
      ? (shape[currentBestIndex].distanceTravelled - shape[startBestIndex].distanceTravelled) / (shape[endBestIndex].distanceTravelled - shape[startBestIndex].distanceTravelled)
      : (shape[currentBestIndex].sequence - shape[startBestIndex].sequence) / (shape[endBestIndex].sequence - shape[startBestIndex].sequence)

    const percent = progress * 100;
    const increment = 72
    const currentProgress = increment * (stopSequenceProgress) + increment * (percent / 100)
    
    const bar = document.querySelector('.sidebar-body-bar')! as HTMLElement;
    const barCover = document.querySelector('.sidebar-body-bar-cover')! as HTMLElement;

    bar.style.height = `${this.stopTimes.length * 72 - 64}px`
    barCover.style.height = `calc(${currentProgress}px)`
  }

  stopScroller = async () => {
    if (!this.container) return
    if (this.stopTimes.length === 0) return

    if (this.container.scrollTop === 0) {
      const stopTimes: StopTime[] = await getSydneyStopTimes(this.props.mode, this.props.name, this.stopTimes[0].departureTime, true)

      if (stopTimes.length > 0)
        this.stopTimes = stopTimes.concat(this.stopTimes)
      else
        this.preLoadingDone = true

      setTimeout(() => {
        if (!this.container) return
        this.container.scrollTop = 72 * stopTimes.length
      }, 0)
    } else if (this.container.scrollTop === 72 * Math.max(0, (this.stopTimes.length - 12)) + (this.preLoadingDone ? 116 : 164)) {
      const stopTimes: StopTime[] = await getSydneyStopTimes(this.props.mode, this.props.name, this.stopTimes[this.stopTimes.length - 1].departureTime, false)
  
      if (stopTimes.length > 0)
        this.stopTimes = this.stopTimes.concat(stopTimes)
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

  getDate(time: string) {
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
