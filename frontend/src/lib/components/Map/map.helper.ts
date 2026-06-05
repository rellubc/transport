import { icons, LineColours, ModeLabels } from "$lib/constants"
import { getRouteColours } from "$lib/helpers"
import { selectionStore } from "$lib/stores/map-selection.store.svelte"
import type { ShapeCoord, Shapes } from "$lib/types/shapes.types"
import type { Stop, Stops } from "$lib/types/stops.types"
import type { StopStopTime, VehicleStopTime } from "$lib/types/stoptimes.types"
import type { Vehicle, Vehicles } from "$lib/types/vehicles.types"
import type { Feature, Point } from "geojson"

export const getStoredView = () => {
  const storedCentre = localStorage.getItem('centre')
  const center: maplibregl.LngLatLike = storedCentre ? [Number(storedCentre.split(',')[0]), Number(storedCentre.split(',')[1])] : [151.05, -33.82]
  const zoom = Number(localStorage.getItem('zoom')) ?? 11.8

  return { center, zoom }
}

export const saveView = (map: maplibregl.Map) => {
  const { lng, lat } = map.getCenter()

  localStorage.setItem('centre', `${lng},${lat}`)
  localStorage.setItem('zoom', map.getZoom().toString())
}

export const loadIcons = async (map: maplibregl.Map) => {
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
}

export const addShapes = (map: maplibregl.Map, shapes: Shapes) => {
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

export const addStops = (map: maplibregl.Map, stops: Stops) => {
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

export const addVehicles = (map: maplibregl.Map, vehicles: Vehicles, modes: Set<string>) => {
  const lineFeatures: Record<string, Feature<Point>[]> = {}

  for (const [_mode, modeVehicles] of Object.entries(vehicles)) {
    for (const line of modes) {
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
          'circle-stroke-color': '#000000'
        }
      })

      map.addLayer({
        id: `${layerId}-highlight`,
        type: "circle",
        source: sourceId,
        filter: ["==", ["get", "vehicleId"], ""],
        paint: {
          "circle-radius": 10,
          'circle-color': LineColours[line],
          'circle-stroke-width': 2,
          'circle-stroke-color': '#000000'
        }
      })
    }
  }
}

export const isStop = (item: Stop | Vehicle): item is Stop => {
  return "stopId" in item
}

export const isVehicle = (item: Stop | Vehicle): item is Vehicle => {
  return "vehicleId" in item
}

export const isStopStopTime = (stopTimes: StopStopTime[] | VehicleStopTime[]): stopTimes is StopStopTime[] => {
  return stopTimes.length === 0 || stopTimes.length > 0 && !("progress" in stopTimes[0]);
}

export const isVehicleStopTime = (stopTimes: StopStopTime[] | VehicleStopTime[]): stopTimes is VehicleStopTime[] => {
  return stopTimes.length === 0 || stopTimes.length > 0 && "progress" in stopTimes[0];
}

export const updateVehicleHighlight = (map: maplibregl.Map, vehicleId: string) => {
  for (const layer of map.getStyle().layers) {
    if (!layer.id.includes("vehicle-layer-highlight")) continue

    map.setFilter(layer.id, [
      '==',
      ['get', 'vehicleId'],
      vehicleId ?? ""
    ]);
  }
}

export const closeSidebar = () => {
  selectionStore.activeItem = null
  selectionStore.activeTrip = ''
  selectionStore.activeStopTimes = []

  console.log(selectionStore)
}