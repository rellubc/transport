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
  import type { ShapeCoord, Shapes } from '$lib/types/shapes.types';
  import { getRouteColours, getSydneyNow } from '$lib/helpers';
  import type { ModeIcon } from '$lib/types/general.type';
  import { LineColours, ModeLabels } from '$lib/constants';
  import type { Vehicle, Vehicles } from '$lib/types/vehicles.types';
  import { stopsApi, stopTimesApi, vehiclesApi } from "$lib/api/client.api";
  import type { StopStopTime, VehicleStopTime } from '$lib/types/stoptimes.types';
  import type { Stop, Stops } from '$lib/types/stops.types';
  import StopSidebarHeader from '$lib/components/Sidebar/StopSidebarHeader.svelte';
  import StopSidebarBody from '$lib/components/Sidebar/StopSidebarBody.svelte';
  import VehicleSidebarHeader from '$lib/components/Sidebar/VehicleSidebarHeader.svelte';
  import VehicleSidebarBody from '$lib/components/Sidebar/VehicleSidebarBody.svelte';
  import Search from './Search.svelte';

  let map!: maplibregl.Map
  let mapContainer: HTMLElement

  const BUFFER_PX = 32

  let refreshInterval: ReturnType<typeof setInterval> | null = null

  let activeItem = $state<Stop | Vehicle | null>(null)
  let activeTrip = $state<string>('')
  let activeStopTimes = $state<StopStopTime[] | VehicleStopTime[]>([])

  let loading = $state<boolean>(false)
  let listElement = $state<HTMLElement | null>(null)
  let searchElement = $state<HTMLElement | null>(null)
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

  const addShapes = (shapes: Shapes) => {
    for (const [shapeId, points] of Object.entries(shapes)) {
      const lines = shapeId.split("_").slice(1, -2)
      const sourceId = `${shapeId.split("_").slice(-2).join("-")}-shape`
      const colours = [...getRouteColours(lines)]
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

      if (colours.length === 1) {
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
            // const offset = (index - (colours.length - 1) / 2) * 3
            map.addLayer({
              id: `${shapeId}-shape-${index}`,
              type: 'line',
              source: sourceId,
              layout: { 'line-join': 'round', 'line-cap': 'round' },
              paint: {
                'line-color': colour,
                'line-width': 2,
                // 'line-offset': [
                //   'interpolate',
                //   ['exponential', 2],
                //   ['zoom'],
                //   11, 1,
                //   12, offset,
                //   15, offset,
                //   16, offset,
                //   22, offset * 100,
                // ]
              }
            })
          }
        })
      }
    }
  }

  const addStops = (stops: Stops) => {
    for (const [mode, modeStops] of Object.entries(stops)) {
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
              stopName: stop.stopName,
              stopLat: stop.stopLat,
              stopLon: stop.stopLon,
              stopParentStation: stop.stopParentStation,
              stopWheelchairBoarding: stop.stopWheelchairBoarding,
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
              stopName: stop.stopName,
              stopLat: stop.stopLat,
              stopLon: stop.stopLon,
              stopParentStation: stop.stopParentStation,
              stopWheelchairBoarding: stop.stopWheelchairBoarding,
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
    const lineFeatures: Record<string, Feature<Point>[]> = {}

    for (const [_mode, modeVehicles] of Object.entries(vehicles)) {
      for (const line of $state.snapshot(transportDataStore.modes)) {
        const features = Object.values(modeVehicles)
          .filter((vehicle) => vehicle.tripRouteShortName === line)
          .map((vehicle) => ({
            type: 'Feature' as const,
            properties: {
              type: 'vehicle',
              tripId: vehicle.tripId,
              tripRouteId: vehicle.tripRouteId,
              tripRouteShortName: vehicle.tripRouteShortName,
              tripScheduleRelationship: vehicle.tripScheduleRelationship,
              vehicleId: vehicle.vehicleId,
              vehicleLabel: vehicle.vehicleLabel,
              vehicleModel: vehicle.vehicleModel,
              positionLatitude: vehicle.positionLatitude,
              positionLongitude: vehicle.positionLongitude,
              timestamp: vehicle.timestamp,
              congestionLevel: vehicle.congestionLevel,
              occupancyStatus: vehicle.occupancyStatus,
              routeType: vehicle.routeType,
            },
            geometry: {
              type: 'Point' as const,
              coordinates: [
                vehicle.positionLongitude,
                vehicle.positionLatitude
              ]
            }
          }))

        if (!features.length) continue

        if (!lineFeatures[line]) {
          lineFeatures[line] = []
        }

        lineFeatures[line].push(...features)
      }
    }

    for (const [line, features] of Object.entries(lineFeatures)) {
      const sourceId = `${line}-vehicle-source`
      const layerId = `${line}-vehicle-layer`

      const data = {
        type: 'FeatureCollection' as const,
        features
      }

      if (!map.getSource(sourceId)) {
        map.addSource(sourceId, {
          type: 'geojson',
          data
        })
      } else {
        (map.getSource(sourceId) as maplibregl.GeoJSONSource).setData(data)
      }

      if (!map.getLayer(layerId)) {
        map.addLayer({
          id: layerId,
          type: 'circle',
          source: sourceId,
          paint: {
            'circle-radius': 6,
            'circle-color': LineColours[line],
            'circle-stroke-width': 1,
            'circle-stroke-color': '#FFFFFF'
          }
        })
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
    map.on('zoomend', () => {
      console.log(map.getZoom())
      saveView()
    })

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

      addShapes($state.snapshot(transportDataStore.displayShapes))
      addStops($state.snapshot(transportDataStore.stops))
      addVehicles($state.snapshot(transportDataStore.vehicles))

      const interval = setInterval(async () => {
        if (activeItem && isStop(activeItem) && !disableRefresh) {
          activeStopTimes = await stopTimesApi.getForStop(activeItem.stopId, "initial", getSydneyNow())
          console.log("Refreshed stop stop times: ", $state.snapshot(activeStopTimes))
        } else if (activeItem && isVehicle(activeItem)) {
          activeItem = await vehiclesApi.getById(activeItem.vehicleId)
          activeStopTimes = await stopTimesApi.getForTrip(activeTrip, activeItem.positionLongitude, activeItem.positionLatitude)
          console.log("Refreshed vehicle info: ", $state.snapshot(activeItem))
          console.log("Refreshed vehicle stop times: ", $state.snapshot(activeStopTimes))
        }
      }, 10000)

      return () => clearInterval(interval)
    })

    map.on('click', (e) => {
      const features = map.queryRenderedFeatures(e.point)

      if (features.length > 0) {
        console.log('Clicked:', features[0].properties)

        if (features[0].properties.type === 'stop') {
          getStopInfo(features[0].properties.stopId)
        } else if (features[0].properties.type === 'vehicle') {
          getVehicleInfoByVehicle(features[0].properties.vehicleId)
        }
      }
      
      for (const line of $state.snapshot(transportDataStore.modes)) {
        map.setPaintProperty(`${line}-vehicle-layer`, 'circle-radius', [
          'case',
          ['==', ['get', 'vehicleId'], features.length !== 0 && features[0].properties.type === 'vehicle' && features[0].properties.vehicleId],
          10,
          6
        ]);
      }
    })

    return () => {
      map?.remove()
    }
  })
  
  $effect(() => {
    const vehicles = $state.snapshot(transportDataStore.vehicles);
    const vehicleModes = Object.keys(vehicles)
    
    if (!map || !map.isStyleLoaded()) return
    if (vehicleModes.length === 0) return

    addVehicles(vehicles)
  });

  $effect(() => {
    if (!activeItem) return
    if (!listElement) return
    if (listElement.scrollTop === 0) listElement.scrollTop = BUFFER_PX
    const list = listElement

    // surely can do something more cleaner when there are less than 20 stop times
    const onScroll = async () => {
      if (!activeItem) return
      if (!isStop(activeItem)) return
      if (fetching || activeStopTimes.length === 0) return
      activeStopTimes = activeStopTimes as StopStopTime[]

      const atTop = list.scrollTop === 0
      const atBottom = Math.abs(list.scrollTop + list.clientHeight - list.scrollHeight) <= 1 / window.devicePixelRatio

      if (atTop) {
        fetching = true
        try {
          const newTimes = await stopTimesApi.getForStop(activeItem.stopId, "prev", activeStopTimes[0].displayTime)
          if (newTimes.length === 0) return
          activeStopTimes = [...newTimes, ...activeStopTimes]

          await tick()
          let additions = newTimes.filter((stopTime) => stopTime.stopType === 'pass' || stopTime.stopType === 'terminate' || stopTime.stopType === 'continues').length * 24
          let platformAdditions = 0
          if (activeItem.stopParentStation) {
            platformAdditions = newTimes.filter((stopTime) => stopTime.stopType === 'stop' || 'depart').length * 60
          } else {
            let viaAdditions = newTimes.filter((stopTime) => stopTime.tripHeadsign.includes('via')).length * 80
            let nonViaAdditions = newTimes.filter((stopTime) => !stopTime.tripHeadsign.includes('via')).length * 60
            platformAdditions = viaAdditions + nonViaAdditions
          }
          list.scrollTop = platformAdditions + newTimes.length + additions
        } catch (error) {
          console.error(error)
        } finally {
          fetching = false
          disableRefresh = true
        }
      } else if (atBottom) {
        fetching = true
        try {
          const newTimes = await stopTimesApi.getForStop(activeItem.stopId, "next", activeStopTimes[activeStopTimes.length - 1].displayTime)
          if (newTimes.length === 0) return
          activeStopTimes = [...activeStopTimes, ...newTimes]
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
  const getStopInfo = async (stopId: string) => {
    loading = true
    try {
      activeItem = await stopsApi.getById(stopId)
      activeStopTimes = await stopTimesApi.getForStop(stopId, "initial", getSydneyNow())
      
      console.log("Stop times: ", $state.snapshot(activeStopTimes))
      console.log("Active stop: ", $state.snapshot(activeItem))
    } catch (err) {
      console.error(err)
    } finally {
      loading = false
    }
  }

  const getVehicleInfoByTrip = async (tripId: string) => {
    loading = true
    try {
      activeItem = await vehiclesApi.getByTrip(tripId)
      activeTrip = tripId
      activeStopTimes = await stopTimesApi.getForTrip(tripId, activeItem.positionLongitude, activeItem.positionLatitude)
      
      console.log("Stop times: ", $state.snapshot(activeStopTimes))
      console.log("Active vehicle: ", $state.snapshot(activeItem))
    } catch (err) {
      console.error(err)
    } finally {
      loading = false
    }
  }

  const getVehicleInfoByVehicle = async (vehicleId: string) => {
    loading = true
    try {
      activeItem = await vehiclesApi.getById(vehicleId)
      activeTrip = activeItem.tripId
      activeStopTimes = await stopTimesApi.getForVehicle(activeItem.vehicleId, activeItem.positionLongitude, activeItem.positionLatitude)
      
      console.log("Stop times: ", $state.snapshot(activeStopTimes))
      console.log("Active vehicle: ", $state.snapshot(activeItem))
    } catch (err) {
      console.error(err)
    } finally {
      loading = false
    }
  }

  const isStop = (item: Stop | Vehicle): item is Stop => {
    return "stopId" in item
  }

  const isVehicle = (item: Stop | Vehicle): item is Vehicle => {
    return "vehicleId" in item
  }

  const isStopStopTime = (stopTimes: StopStopTime[] | VehicleStopTime[]): stopTimes is StopStopTime[] => {
    return stopTimes.length > 0 && !("progress" in stopTimes[0]);
  }

  const isVehicleStopTime = (stopTimes: StopStopTime[] | VehicleStopTime[]): stopTimes is VehicleStopTime[] => {
    return stopTimes.length > 0 && "progress" in stopTimes[0];
  }

</script>

<svelte:window onclick={(e: MouseEvent) => {
  if (!activeItem) return

  const path = e.composedPath();
  if (sidebarElement && !path.includes(sidebarElement)) {
    if (!searchElement || (searchElement && !path.includes(searchElement))) {
      activeItem = null
      activeTrip = ''
      activeStopTimes = []
    }
  }
}}/>

<div class="relative w-screen h-screen">
  <div bind:this={mapContainer} id="map" class="w-full h-full opacity-50"></div>
  <div class="absolute top-4 left-4 flex flex-col">
    <Search bind:searchElement getStopInfo={getStopInfo} />
    <div class="mt-16">
      {#if activeItem && isStop(activeItem) && isStopStopTime(activeStopTimes) && !loading}
        <div bind:this={sidebarElement} class="bg-white w-md h-[calc(100vh-6rem)] flex flex-col p-8 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)]">
          <StopSidebarHeader title={activeItem.stopName} id={activeItem.stopId} />
          <StopSidebarBody bind:listElement activeStop={activeItem} stopTimes={activeStopTimes} getVehicleInfo={getVehicleInfoByTrip}/>
        </div>
      {:else if activeItem && isVehicle(activeItem) && isVehicleStopTime(activeStopTimes) && !loading}
        <div bind:this={sidebarElement} class="bg-white w-md h-[calc(100vh-6rem)] flex flex-col p-8 rounded-2xl shadow-[0px_0px_20px_10px_rgba(0,0,0,0.3)]">
          {#if [...activeStopTimes].find((stopTime) => stopTime.progress === "passed")}
            <VehicleSidebarHeader title={[...activeStopTimes].reverse().find((stopTime) => stopTime.progress === "passed")?.tripHeadsign} id={activeItem.vehicleId} routeShortName={[...activeStopTimes].reverse().find((stopTime) => stopTime.progress === "passed")?.routeShortName} routeColour={[...activeStopTimes].reverse().find((stopTime) => stopTime.progress === "passed")?.routeColour} />
          {:else}
            <VehicleSidebarHeader title={activeStopTimes[0].tripHeadsign} id={activeItem.vehicleId} routeShortName={activeStopTimes[0].routeShortName} routeColour={activeStopTimes[0].routeColour} />
          {/if}
          <VehicleSidebarBody stopTimes={activeStopTimes} getStopInfo={getStopInfo} />
        </div>
      {/if}
    </div>
  </div>
</div>
