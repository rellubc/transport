import { stopsApi, stopTimesApi, vehiclesApi } from "$lib/api/client.api"
import { getSydneyNow } from "$lib/helpers"
import type { Stop } from "$lib/types/stops.types"
import type { StopStopTime, VehicleStopTime } from "$lib/types/stoptimes.types"
import type { Vehicle } from "$lib/types/vehicles.types"
import { tick } from "svelte"

export interface SelectionStore {
  loading: boolean
  activeItem: Stop | Vehicle | null
  activeTrip: string
  activeStopTimes: StopStopTime[] | VehicleStopTime[]
}

export const selectionStore = $state<SelectionStore>({
  loading: false,
  activeItem: null,
  activeTrip: '',
  activeStopTimes: []
})

export const selectStop = async (stopId: string) => {
  selectionStore.loading = true
  try {
    selectionStore.activeItem = await stopsApi.getById(stopId)
    selectionStore.activeStopTimes = await stopTimesApi.getForStop(stopId, "initial", getSydneyNow())
    
    console.log("Stop times: ", $state.snapshot(selectionStore.activeStopTimes))
    console.log("Active stop: ", $state.snapshot(selectionStore.activeItem))
  } catch (err) {
    console.error(err)
  } finally {
    selectionStore.loading = false
  }
}

export const selectVehicleByTripId = async (tripId: string) => {
  selectionStore.loading = true
  try {
    selectionStore.activeItem = await vehiclesApi.getByTrip(tripId)
    selectionStore.activeTrip = tripId
    selectionStore.activeStopTimes = await stopTimesApi.getForTrip(tripId, selectionStore.activeItem.positionLongitude, selectionStore.activeItem.positionLatitude)
    
    console.log("Stop times: ", $state.snapshot(selectionStore.activeStopTimes))
    console.log("Active vehicle: ", $state.snapshot(selectionStore.activeItem))
  } catch (err) {
    console.error(err)
  } finally {
    selectionStore.loading = false
  }
}

export const selectVehicleByVehicleId = async (vehicleId: string) => {
  selectionStore.loading = true
  try {
    selectionStore.activeItem = await vehiclesApi.getById(vehicleId)
    selectionStore.activeTrip = selectionStore.activeItem.tripId
    selectionStore.activeStopTimes = await stopTimesApi.getForVehicle(selectionStore.activeItem.vehicleId, selectionStore.activeItem.positionLongitude, selectionStore.activeItem.positionLatitude)
    
    console.log("Stop times: ", $state.snapshot(selectionStore.activeStopTimes))
    console.log("Active vehicle: ", $state.snapshot(selectionStore.activeItem))
  } catch (err) {
    console.error(err)
  } finally {
    selectionStore.loading = false
  }
}

export const refreshStopStopTimes = async (stopId: string) => {
  selectionStore.activeStopTimes = await stopTimesApi.getForStop(stopId, "initial", getSydneyNow())
}

export const refreshVehicleStopTimes = async (vehicleId: string) => {
  selectionStore.activeItem = await vehiclesApi.getById(vehicleId)
  selectionStore.activeStopTimes = await stopTimesApi.getForTrip(selectionStore.activeTrip, selectionStore.activeItem.positionLongitude, selectionStore.activeItem.positionLatitude)
}
