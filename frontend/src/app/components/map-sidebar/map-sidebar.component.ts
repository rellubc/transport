import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { coloursMap, routesMap, vehiclesMap } from '../../../shared/models/constants';
import { CommonModule } from '@angular/common';
import { StopTimeUpdate, TripUpdate, VehiclePosition } from '../../../shared/models/realtime';
import { Trip } from '../../../shared/models/trip';
import { Stop } from '../../../shared/models/stop';
import { StopTime } from '../../../shared/models/stopTime';
import { LucideAngularModule, X } from 'lucide-angular';
import { getSydneyMetroStopTimes, getSydneyMetroTrip, getSydneyMetroTripUpdates } from '../../sydney-metro/sydney-metro-helpers';
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
  currentRealtimeTrip: TripUpdate | null = null
  currentTrip: Trip | null = null
  currentTripStopTimes: StopTimeUpdate[] = []

  currentStopScheduledServices: StopTime[] = []
  container: HTMLElement | null = null

  preLoadingDone: boolean = false
  postLoadingDone: boolean = false

  async refresh() {
    if (!this.open) return

    if (this.currentProps.propType === 'vehicle') {
      console.log('Updating current schedule...')

      this.currentVehicle = this.vehicles.find((vehicle) => vehicle.vehicle?.id === this.currentProps.id)!
      if (this.currentProps.mode === 'metro')
        this.currentRealtimeTrip = await getSydneyMetroTripUpdates(this.currentProps.tripId)
      else if (this.currentProps.mode === 'sydneytrains')
        this.currentRealtimeTrip = await getSydneyTrainsTripUpdates(this.currentProps.tripId)

      this.updateBar()
    } else if (this.currentProps.propType === 'stop') {
      console.log('Updating current departures...')
    }
  }

  async openSidebar(incomingProps: any) {
    if (this.currentProps && incomingProps.propType === this.currentProps.propType && incomingProps.id === this.currentProps.id) return

    this.resetSidebar()
    
    this.open = true
    this.currentProps = incomingProps
    if (incomingProps.propType === 'vehicle') {
      console.log('vehicle')
      if (incomingProps.id === "UNASSIGNED") return
      if (incomingProps.tripId.includes("NonTimetabled")) return

      this.currentVehicle = this.vehicles.find((vehicle) => vehicle.vehicle?.id === this.currentProps.id)!

      console.log(this.currentProps.type)

      if (this.currentProps.type === 'metro') {
        this.currentRealtimeTrip = await getSydneyMetroTripUpdates(incomingProps.tripId)
        this.currentTrip = await getSydneyMetroTrip(this.currentProps.tripId)
      } else {
        this.currentRealtimeTrip = await getSydneyTrainsTripUpdates(incomingProps.tripId)
        this.currentTrip = await getSydneyTrainsTrip(this.currentProps.tripId)
        const tripStopTimes: StopTime[] = await getSydneyTrainsTripStopTimes(this.currentProps.tripId, new Date().toISOString())

        console.log(this.currentRealtimeTrip)
        console.log(tripStopTimes)

        this.currentTripStopTimes = tripStopTimes.map((st) => {
          const corresponding = this.currentRealtimeTrip!.stopTimeUpdate.find((stu) => stu.stopId === st.stopId)
          const today = new Date();
          const [arrivalH, arrivalM, arrivalS] = st.arrivalTime.split(':').map(Number)
          const [departureH, departureM, departureS] = st.departureTime.split(':').map(Number)

          if (!corresponding) {
            return {
              stopName: st.stopName.replace('Station', 'Station,'),
              stopSequence: Number(st.stopSequence),
              stopId: st.stopId,
              arrival: {
                time: new Date(today.getFullYear(), today.getMonth(), today.getDate(), arrivalH, arrivalM, arrivalS, 0),
              },
              departure: {
                time: new Date(today.getFullYear(), today.getMonth(), today.getDate(), departureH, departureM, departureS, 0),
              },
            }
          }

          return {
            stopName: st.stopName.replace('Station', 'Station,'),
            stopSequence: Number(st.stopSequence),
            stopId: st.stopId,
            arrival: {
              delay: corresponding.arrival?.delay ?? 0,
              time: new Date(today.getFullYear(), today.getMonth(), today.getDate(), arrivalH, arrivalM, arrivalS, 0),
              uncertainty: corresponding.arrival?.uncertainty ?? 0,
            },
            departure: {
              delay: corresponding.departure?.delay ?? 0,
              time: new Date(today.getFullYear(), today.getMonth(), today.getDate(), departureH, departureM, departureS, 0),
              uncertainty: corresponding.departure?.uncertainty ?? 0,
            },
            departureOccupancyStatus: corresponding.departureOccupancyStatus,
            carriageSeqPredictiveOccupancy: corresponding.carriageSeqPredictiveOccupancy
          }
        })
        const temp = this.currentRealtimeTrip!.stopTimeUpdate
        this.currentRealtimeTrip!.stopTimeUpdate = this.currentTripStopTimes
        this.currentTripStopTimes = temp
      }

      this.updateBar()
    } else if (incomingProps.propType === 'stop') {
      console.log('stop')
      if (this.currentProps.mode === 'metro')
        this.currentStopScheduledServices = await getSydneyMetroStopTimes(this.currentProps.name, new Date().toISOString(), false)
      else if (this.currentProps.mode === 'sydneytrains')
        this.currentStopScheduledServices = await getSydneyTrainsStopTimes(this.currentProps.name, new Date().toISOString(), false)
      else if (this.currentProps.mode === 'combined') {
        this.currentStopScheduledServices = await getSydneyCombinedStopTimes(this.currentProps.name, new Date().toISOString(), false)
        console.log(this.currentStopScheduledServices)
      }

      console.log(this.currentStopScheduledServices)

      if (this.currentStopScheduledServices.length < 24) {
        this.preLoadingDone = true
        this.postLoadingDone = true
      }

      this.container = document.querySelector('.sidebar-body-content') as HTMLElement
      this.container.addEventListener('scroll', this.stopScroller)

      setTimeout(() => {
        if (!this.container) return
        this.container.scrollTop = 48
      }, 0)
    } else if (incomingProps.propType === 'route') {
      console.log('route')
    }
  }

  closeSidebar() {
    console.log('close')
    this.open = false
    this.resetSidebar()
  }

  resetSidebar() {
    console.log('reset')
    this.currentProps = null

    this.currentVehicle = null
    this.currentRealtimeTrip = null
    this.currentTrip = null
    this.currentTripStopTimes = []

    this.currentStopScheduledServices = []
  
    this.preLoadingDone = false
    this.postLoadingDone = false
  }

  updateBar() {
    const vehicle = this.currentVehicle
    const realtime = this.currentRealtimeTrip
    const trip = this.currentTrip
    if (!vehicle || !realtime || !trip) return

    const bar = document.querySelector('.sidebar-body-bar')! as HTMLElement;
    const barCover = document.querySelector('.sidebar-body-bar-cover')! as HTMLElement;

    bar.style.height = `${realtime.stopTimeUpdate.length! * 72 - 32}px`

    const currentTripStartSegment = this.currentProps.type === 'metro' ? this.stops.find((stop) => stop.id === vehicle.stopId)?.id! : realtime.stopTimeUpdate[realtime.stopTimeUpdate.length - this.currentTripStopTimes.length - 1].stopId!
    const nextIndex = Number(realtime.stopTimeUpdate.findIndex((stop) => stop.stopId === currentTripStartSegment.toString())) + 1
    const currentTripEndSegment = this.stops.find((stop) => stop.id === realtime.stopTimeUpdate[nextIndex].stopId)?.id!

    const start = this.stops.find((stop) => stop.id === currentTripStartSegment!)!
    const end = this.stops.find((stop) => stop.id === currentTripEndSegment!)!

    let startBestIndex = 0;
    let startBestDiff = [Math.abs(this.shapes[trip.shapeId][0].latitude - start.latitude!), Math.abs(this.shapes[trip.shapeId][0].longitude - start.longitude!)];
    let endBestIndex = 0;
    let endBestDiff = [Math.abs(this.shapes[trip.shapeId][0].latitude - end.latitude!), Math.abs(this.shapes[trip.shapeId][0].longitude - end.longitude!)];

    for (let i = 1; i < this.shapes[trip.shapeId].length; i++) {
      const startDiff = [Math.abs(this.shapes[trip.shapeId][i].latitude - start.latitude!), Math.abs(this.shapes[trip.shapeId][i].longitude - start.longitude!)];
      if (startDiff[0] < startBestDiff[0] && startDiff[1] < startBestDiff[1]) {
        startBestIndex = i;
        startBestDiff = startDiff;
      }

      const endDiff = [Math.abs(this.shapes[trip.shapeId][i].latitude - end.latitude!), Math.abs(this.shapes[trip.shapeId][i].longitude - end.longitude!)];
      if (endDiff[0] < endBestDiff[0] && endDiff[1] < endBestDiff[1]) {
        endBestIndex = i;
        endBestDiff = endDiff;
      }
    }

    let currentBestIndex = startBestIndex;
    let currentBestDiff = [Math.abs(this.shapes[trip.shapeId][startBestIndex].latitude - vehicle.position!.latitude!), Math.abs(this.shapes[trip.shapeId][startBestIndex].longitude - vehicle.position!.longitude!)];

    for (let i = startBestIndex; i < endBestIndex; i++) {
      const currentDiff = [Math.abs(this.shapes[trip.shapeId][i].latitude - vehicle.position!.latitude!), Math.abs(this.shapes[trip.shapeId][i].longitude - vehicle.position!.longitude!)];
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
    const currentProgress = increment * (nextIndex - 1) + increment * (percent / 100) + 32
    
    barCover.style.height = `calc(${currentProgress}px)`
  }

  stopScroller = async () => {
    if (!this.container) return
    if (this.currentStopScheduledServices.length === 0) return
    if (this.container.scrollTop === 0) {
      let temp: StopTime[] = []
      if (this.currentProps.mode === 'metro')
        temp = await getSydneyMetroStopTimes(this.currentProps.name, this.currentStopScheduledServices[0].arrivalTime, true)
      else if (this.currentProps.mode === 'sydneytrains')
        temp = await getSydneyTrainsStopTimes(this.currentProps.name, this.currentStopScheduledServices[0].arrivalTime, true)
      else if (this.currentProps.mode === 'combined') {
        temp = await getSydneyCombinedStopTimes(this.currentProps.name, this.currentStopScheduledServices[0].arrivalTime, true)
        console.log(temp)
      }
      // const temp = await getSydneyTrainsStopTimes(this.currentProps.name, this.currentStopScheduledServices[0].arrivalTime, true)

      if (temp.length > 0)
        this.currentStopScheduledServices = temp.concat(this.currentStopScheduledServices)
      else
        this.preLoadingDone = true
      setTimeout(() => {
        if (!this.container) return
        this.container.scrollTop = 72 * temp.length
      }, 0)
    } else if (this.container.scrollTop === 72 * Math.max(0, (this.currentStopScheduledServices.length - 12)) + (this.preLoadingDone ? 62 : 110)) {
      let temp: StopTime[] = []
      if (this.currentProps.mode === 'metro')
        temp = await getSydneyMetroStopTimes(this.currentProps.name, this.currentStopScheduledServices[this.currentStopScheduledServices.length - 1].arrivalTime, false)
      else if (this.currentProps.mode === 'sydneytrains')
        temp = await getSydneyTrainsStopTimes(this.currentProps.name, this.currentStopScheduledServices[this.currentStopScheduledServices.length - 1].arrivalTime, false)
      else if (this.currentProps.mode === 'combined') {
        temp = await getSydneyCombinedStopTimes(this.currentProps.name, this.currentStopScheduledServices[this.currentStopScheduledServices.length - 1].arrivalTime, false)
        console.log(temp)
      }
      // const temp = await getSydneyTrainsStopTimes(this.currentProps.name, this.currentStopScheduledServices[this.currentStopScheduledServices.length - 1].arrivalTime, false)

      if (temp.length > 0)
        this.currentStopScheduledServices = this.currentStopScheduledServices.concat(temp)
      else
        this.postLoadingDone = true
    }
  }

  getVehicleForTrip(tripId: string): VehiclePosition | undefined {
    return this.vehicles.find((vehicle) => vehicle.trip?.tripId === tripId)
  }

  getVehicleModel(vehicleModel: string, mode: string) {
    if (mode === 'metro')
      return vehiclesMap[vehicleModel] 
    return vehiclesMap[vehicleModel] 
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
    if (mode === 'metro')
      return routesMap[routeId.split('_')[1]] 
    return routesMap[routeId.split('_')[0]] 
  }

  getCorrespondingRouteColour(routeId: string, mode: string) {
    if (mode === 'metro')
      return coloursMap[routeId.split('_')[1]] 
    return coloursMap[routeId.split('_')[0]]
  }
}
