<script lang="ts">
  import { onMount, tick } from 'svelte';

  import maplibregl from 'maplibre-gl';
  import type { Feature, Point } from 'geojson'

  import metroImg from '$lib/assets/metro.png'
  import sydneytrainsImg from '$lib/assets/sydneytrains.png'
  import lightrailImg from '$lib/assets/lightrail.png'
  import nswtrainsImg from '$lib/assets/nswtrains.png'
  import busImg from '$lib/assets/bus.png'
  import ferryImg from '$lib/assets/ferry.png'
  import schoolbusImg from '$lib/assets/schoolbus.png'
  import coachImg from '$lib/assets/coach.png'

  import { transportDataStore } from '$lib/stores.svelte';
  import type { ShapeCoord } from '$lib/types/shapes.types';
  import { getRouteColours, getSydneyNow } from '$lib/helpers';
  import type { ModeIcon } from '$lib/types/general.type';
  import { LineColours, ModeLabels } from '$lib/constants';
  import type { Vehicle, Vehicles } from '$lib/types/vehicles.types';
  import { stopTimesApi } from '$lib/api/stoptimes';
  import { vehiclesApi } from '$lib/api/vehicles';
  import type { StopStopTime, VehicleStopTime } from '$lib/types/stoptimes.types';
  import type { Stop } from '$lib/types/stops.types';
  import VehicleSidebarHeader from '../Sidebar/VehicleSidebarHeader.svelte';
  import StopSidebarHeader from '../Sidebar/StopSidebarHeader.svelte';
  import StopSidebarBody from '../Sidebar/StopSidebarBody.svelte';
  import VehicleSidebarBody from '../Sidebar/VehicleSidebarBody.svelte';

  let map!: maplibregl.Map
  let mapContainer: HTMLElement

  const BUFFER_PX = 32

  let refreshInterval: ReturnType<typeof setInterval> | null = null

  let activeStop = $state<Stop | null>(null)
  let activeVehicle = $state<Vehicle | null>(null)

  let stopTimes = $state<StopStopTime[] | VehicleStopTime[]>([])

  let listElement = $state<HTMLElement | null>(null)
  let sidebarElement = $state<HTMLElement | null>(null)
  let fetching = $state<boolean>(false)
  let disableRefresh = $state<boolean>(false)

  const icons: ModeIcon[] = [
    { name: 'sydneytrains-icon', url: sydneytrainsImg },
    { name: 'metro-icon', url: metroImg },
    { name: 'lightrail-icon', url: lightrailImg },
    { name: 'nswtrains-icon', url: nswtrainsImg },
    { name: 'bus-icon', url: busImg },
    { name: 'ferry-icon', url: ferryImg },
    { name: 'schoolbus-icon', url: schoolbusImg },
    { name: 'coach-icon', url: coachImg }
  ]

  const getStoredView = () => {
    const storedCentre = localStorage.getItem('centre')
    const center: maplibregl.LngLatLike = storedCentre ? [Number(storedCentre.split(',')[0]), Number(storedCentre.split(',')[1])] : [151.05, -33.82]
    const zoom = Number(localStorage.getItem('zoom')) ?? 11.8
  
    return { center, zoom }
  }

  const saveView = () => {
    const { lng, lat } = map.getCenter()

    localStorage.setItem('centre', `${lng},${lat}`)
    localStorage.setItem('zoom', map.getZoom().toString())
  }

  const addShapes = () => {
    const shapes = $state.snapshot(transportDataStore.displayShapes)
    for (const [shapeId, points] of Object.entries(shapes)) {
      const line = shapeId.split("DISPLAY_")[1]
      const colours = getRouteColours(line.split("_")[0])
      const sourceId = `${line}-shape`

      const coords = points.map((point: ShapeCoord) => [point.shapePtLon, point.shapePtLat])
      
      if (!map.getSource(sourceId)) {
        map.addSource(sourceId, {
          type: 'geojson',
          data: {
            type: 'FeatureCollection',
            features: [{
              type: 'Feature',
              properties: {},
              geometry: {
                type: 'LineString',
                coordinates: coords
              }
            }]
          }
        })
      }

      if (colours.size === 1) {
        if (!map.getLayer(`${shapeId}-shape`)) {
          map.addLayer({
            id: `${shapeId}-shape`,
            type: 'line',
            source: sourceId,
            layout: { 'line-join': 'round', 'line-cap': 'round' },
            paint: {
              'line-color': [...colours][0],
              'line-width': 2,
              'line-dasharray': [1, 0],
              'line-offset': 0
            }
          })
        }
      } else {
        [...colours].forEach((colour, index) => {
          if (!map.getLayer(`${shapeId}-shape-${index}`)) {
            const offset = (index - (colours.size - 1) / 2) * 3
            map.addLayer({
              id: `${shapeId}-shape-${index}`,
              type: 'line',
              source: sourceId,
              layout: { 'line-join': 'round', 'line-cap': 'round' },
              paint: {
                'line-color': colour,
                'line-width': 2,
                'line-dasharray': [1, 0],
                'line-offset': offset
              }
            })
          }
        })
      }
    }
  }

  const addStops = () => {
    const stops = $state.snapshot(transportDataStore.stops)
    for (const [mode, modeStops] of Object.entries(stops)) {
      console.log(mode, modeStops)
      const modeText = ModeLabels[Number(mode)]
      const imageSource = modeText.split('/')[0]

      const platformFeatures: Feature<Point>[] = []
      Object.values(modeStops)
        .filter((stop) => stop.stopParentStation)
        .forEach((stop) => {
          platformFeatures.push({
            type: 'Feature',
            properties: {
              type: 'stop',
              stopId: stop.stopId,
              stopCode: stop.stopCode,
              stopName: stop.stopName,
              stopLat: stop.stopLat,
              stopLon: stop.stopLon,
              stopZoneId: stop.stopZoneId,
              stopUrl: stop.stopUrl,
              stopLocationType: stop.stopLocationType,
              stopParentStation: stop.stopParentStation,
              stopTimezone: stop.stopTimezone,
              stopWheelchairBoarding: stop.stopWheelchairBoarding,
              stopPlatformCode: stop.stopPlatformCode,
              routeType: stop.routeType,
            },
            geometry: {
              type: 'Point',
              coordinates: [stop.stopLon, stop.stopLat]
            }
          })
        })

      const stationFeatures: Feature<Point>[] = []
      Object.values(modeStops)
        .filter((stop) => !stop.stopParentStation)
        .forEach((stop) => {
          stationFeatures.push({
            type: 'Feature',
            properties: {
              type: 'stop',
              stopId: stop.stopId,
              stopCode: stop.stopCode,
              stopName: stop.stopName,
              stopLat: stop.stopLat,
              stopLon: stop.stopLon,
              stopZoneId: stop.stopZoneId,
              stopUrl: stop.stopUrl,
              stopLocationType: stop.stopLocationType,
              stopParentStation: stop.stopParentStation,
              stopTimezone: stop.stopTimezone,
              stopWheelchairBoarding: stop.stopWheelchairBoarding,
              stopPlatformCode: stop.stopPlatformCode,
              routeType: stop.routeType,
            },
            geometry: {
              type: 'Point',
              coordinates: [stop.stopLon, stop.stopLat]
            }
          })
        })
      
      if (!map.getSource(`${modeText}-platforms-source`)) {
        map.addSource(`${modeText}-platforms-source`, {
          type: 'geojson',
          data: {
            type: 'FeatureCollection',
            features: platformFeatures
          }
        })
      }

      if (!map.getSource(`${modeText}-stations-source`)) {
        map.addSource(`${modeText}-stations-source`, {
          type: 'geojson',
          data: {
            type: 'FeatureCollection',
            features: stationFeatures
          }
        })
      }

      if (!map.getLayer(`${modeText}-platforms-layer`)) {
        map.addLayer({
          id: `${modeText}-platforms-layer`,
          type: 'symbol',
          source: `${modeText}-platforms-source`,
          layout: {
            'icon-image': `${imageSource}-icon`,
            'icon-size': 0.06,
            'icon-allow-overlap': true
          },
          minzoom: 17
        })
      }

      if (!map.getLayer(`${modeText}-stations-layer`)) {
        map.addLayer({
          id: `${modeText}-stations-layer`,
          type: 'symbol',
          source: `${modeText}-stations-source`,
          layout: {
            'icon-image': `${imageSource}-icon`,
            'icon-size': 0.06,
            'icon-allow-overlap': false
          },
          maxzoom: 16.99
        })
      }
    }
  }

  const addVehicles = (vehicles: Vehicles) => {
    console.log(vehicles)
    for (const [mode, modeVehicles] of Object.entries(vehicles)) {
      console.log(mode, modeVehicles)

      for (const line of transportDataStore.modes) {
        const vehicleFeatures: Feature<Point>[] = []
        Object.values(modeVehicles)
          .filter((vehicle) => vehicle.tripRouteShortName === line)  
          .forEach((vehicle) => {
            vehicleFeatures.push({
              type: 'Feature',
              properties: {
                type: 'vehicle',
                tripId: vehicle.tripId,
                tripRouteId: vehicle.tripRouteId,
                tripScheduleRelationship: vehicle.tripScheduleRelationship,
                vehicleId: vehicle.vehicleId,
                vehicleLabel: vehicle.vehicleLabel,
                vehicleModel: vehicle.vehicleModel,
                positionLatitude: vehicle.positionLatitude,
                positionLongitude: vehicle.positionLongitude,
                stopId: vehicle.stopId,
                timestamp: vehicle.timestamp,
                congestionLevel: vehicle.congestionLevel,
                occupancyStatus: vehicle.occupancyStatus,
                routeType: vehicle.routeType,
              },
              geometry: {
                type: 'Point',
                coordinates: [vehicle.positionLongitude, vehicle.positionLatitude]
              }
            })
          })

        if (!vehicleFeatures.length) continue

        if (!map.getSource(`${line}-vehicle-source`)) {
          map.addSource(`${line}-vehicle-source`, {
            type: 'geojson',
            data: {
              type: 'FeatureCollection',
              features: vehicleFeatures
            }
          })
        } else {
          (map.getSource(`${line}-vehicle-source`) as maplibregl.GeoJSONSource).setData({
            type: 'FeatureCollection',
            features: vehicleFeatures
          })
        }

        if (!map.getLayer(`${line}-vehicle-layer`)) {
          map.addLayer({
            id: `${line}-vehicle-layer`,
            type: 'circle',
            source: `${line}-vehicle-source`,
            paint: {
              'circle-radius': 6,
              'circle-color': LineColours[line],
              'circle-stroke-width': 1,
              'circle-stroke-color': '#FFFFFF'
            },
          })
        }
      }
    }
  }

  onMount(() => {
    const { center, zoom } = getStoredView()
    map = new maplibregl.Map({
      container: mapContainer,
      style: `https://api.maptiler.com/maps/019dfbef-34ee-7a66-a158-87b7c0aba3a3/style.json?key=${import.meta.env.VITE_MAPTILER_KEY}#1.0/0.00000/0.00000`,
      center,
      zoom
    })

    map.addControl(new maplibregl.FullscreenControl())
    map.addControl(new maplibregl.NavigationControl())

    map.on('moveend', saveView)
    map.on('zoomend', saveView)

    map.on('load', async () => {
      await Promise.all(
        icons.map(({ name, url }) => {
          new Promise<void>((resolve) => {
            const img = new Image()
            img.onload = () => {
              map.addImage(name, img)
              resolve()
            }
            img.src = url
          })
        }
      ))

      addShapes()
      addStops()
      addVehicles($state.snapshot(transportDataStore.vehicles))

      const interval = setInterval(async () => {
        if (!listElement || disableRefresh) return

        if (activeStop) {
          stopTimes = await stopTimesApi.getForStop(activeStop.stopId, "initial", getSydneyNow())
          console.log("Refreshed stop stop times: ", $state.snapshot(stopTimes))
        } else if (activeVehicle) {
          activeVehicle = await vehiclesApi.getById(activeVehicle.vehicleId)
          stopTimes = await stopTimesApi.getForTrip(activeVehicle.tripId, activeVehicle.positionLongitude, activeVehicle.positionLatitude)
          console.log("Refreshed vehicle info: ", $state.snapshot(activeVehicle))
          console.log("Refreshed vehicle stop times: ", $state.snapshot(stopTimes))
        }
      }, 10000)

      return () => clearInterval(interval)
    })

    map.on('click', (e) => {
      const features = map.queryRenderedFeatures(e.point)

      if (features.length > 0) {
        console.log('Clicked:', features[0].properties)

        if (features[0].properties.type === 'stop') {
          stopStopTimes(features[0].properties as Stop)
        } else if (features[0].properties.type === 'vehicle') {
          vehicleStopTimes(features[0].properties as Vehicle)
        }
      }
    })

    return () => {
      map?.remove()
    }
  })
  
  $effect(() => {
    if (!map || !map.isStyleLoaded()) return;

    const vehicles = $state.snapshot(transportDataStore.vehicles);
    if (!vehicles) return;

    addVehicles(vehicles);
  });

  $effect(() => {
    if (!activeStop) return
    if (!listElement) return
    if (listElement.scrollTop === 0) listElement.scrollTop = BUFFER_PX
    const list = listElement

    // surely can do something more cleaner when there are less than 20 stop times
    const onScroll = async () => {
      const atTop = list.scrollTop === 0
      const atBottom = Math.abs(list.scrollTop + list.clientHeight - list.scrollHeight) <= 1 / window.devicePixelRatio

      stopTimes = stopTimes as StopStopTime[]

      if (fetching || stopTimes.length === 0) return


      if (atTop) {
        fetching = true
        try {
          const newTimes = await stopTimesApi.getForStop(activeStop!.stopId, "prev", stopTimes[0].displayTime)
          if (newTimes.length === 0) return
          stopTimes = [...newTimes, ...stopTimes]

          await tick()
          let additions = newTimes.filter((stopTime) => stopTime.stopType === 'pass' || stopTime.stopType === 'terminate').length * 24
          list.scrollTop = newTimes.length * 60 + newTimes.length + additions
        } catch (error) {
          console.error(error)
        } finally {
          fetching = false
          disableRefresh = true
        }
      } else if (atBottom) {
        fetching = true
        try {
          const newTimes = await stopTimesApi.getForStop(activeStop!.stopId, "next", stopTimes[stopTimes.length - 1].displayTime)
          if (newTimes.length === 0) return
          stopTimes = [...stopTimes, ...newTimes]
        } catch (error) {
          console.error(error)
        } finally {
          fetching = false
          disableRefresh = true
        }
      }
    }

    list.addEventListener('scroll', onScroll)
    return () => list.removeEventListener('scroll', onScroll)
  })

  // todo: add refreshing when scrolled
  // todo: for regional trains, remove duplicated entries on sydney trains
  const stopStopTimes = async (stop: Stop) => {
    try {
      stopTimes = await stopTimesApi.getForStop(stop.stopId, "initial", getSydneyNow())
      activeStop = stop
      
      console.log("Stop times: ", $state.snapshot(stopTimes))
      console.log("Active stop: ", $state.snapshot(activeStop))
    } catch (err) {
      console.error(err)
    }
  }

  const vehicleStopTimes = async (vehicle: Vehicle) => {
    try {
      stopTimes = await stopTimesApi.getForTrip(vehicle.tripId, vehicle.positionLongitude, vehicle.positionLatitude)
      activeVehicle = vehicle

      console.log("Vehicle info: ", $state.snapshot(activeVehicle))
      console.log("Vehicle stop times: ", $state.snapshot(stopTimes))
    } catch (err) {
      console.error(err)
    }
  }
</script>

<svelte:window onclick={(e: MouseEvent) => {
  if (!activeStop && !activeVehicle) return
  if (sidebarElement && !sidebarElement.contains(e.target as Node)) {
    activeStop = null
    activeVehicle = null
    if (refreshInterval) clearInterval(refreshInterval)
  }
}}/>

<div class="relative w-screen h-screen">
  <div bind:this={mapContainer} id="map" class="w-full h-full"></div>
  {#if activeStop}
    <div bind:this={sidebarElement} class="absolute top-4 left-4 bg-white w-md h-[calc(100vh-2rem)] flex flex-col p-8 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)]">
      <StopSidebarHeader title={activeStop.stopName} id={activeStop.stopId} />
      <StopSidebarBody bind:listElement stopTimes={stopTimes as StopStopTime[]} />
    </div>
  {:else if activeVehicle}
    <div bind:this={sidebarElement} class="absolute top-4 left-4 bg-white w-md h-[calc(100vh-2rem)] flex flex-col p-8 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)]">
      <VehicleSidebarHeader stopTime={stopTimes[0] as VehicleStopTime} activeVehicle={activeVehicle} />
      <VehicleSidebarBody bind:listElement stopTimes={stopTimes as VehicleStopTime[]} />
    </div>
  {/if}
</div>
