<script lang="ts">
  import { onDestroy, onMount } from 'svelte'
  import maplibregl from 'maplibre-gl'
  import 'maplibre-gl/dist/maplibre-gl.css'
  import type { Feature, Point } from 'geojson'

  import metroImg from '$lib/assets/metro.png'
  import sydneytrainsImg from '$lib/assets/sydneytrains.png'
  import lightrailImg from '$lib/assets/lightrail.png'

  import { modes, shapes, stops, vehicles } from '$lib/stores'
  import { LineColours, ModeLabels, ModeType } from '$lib/constants'
  import { getRouteColours } from '$lib/helpers';
  import type { Vehicles } from '$lib/types/realtime'
  import type { ShapeCoord, Shapes } from '$lib/types/shape';
  import type { Stops } from '$lib/types/stop';
  import type { ModeIcon } from '$lib/types/general';

  import MapSidebar from './MapSidebar.svelte';
  import TransportModes from './modes/TransportModes.svelte';

  const icons: ModeIcon[] = [
    { name: 'sydneytrains-icon', url: sydneytrainsImg },
    { name: 'metro-icon', url: metroImg },
    { name: 'lightrail-icon', url: lightrailImg }
  ]

  let map!: maplibregl.Map
  let subModes: (() => void)
  let subVehicles: (() => void)
  let selectedFeature: any = $state(null)

  const getStoredView = () => {
    const storedCentre = localStorage.getItem('centre')
    const centre: maplibregl.LngLatLike = storedCentre ? [Number(storedCentre.split(',')[0]), Number(storedCentre.split(',')[1])] : [151.05, -33.82]
    const zoom = Number(localStorage.getItem('zoom')) ?? 11.8
  
    return { centre, zoom }
  }

  const saveView = () => {
    const { lng, lat } = map.getCenter()

    localStorage.setItem('centre', `${lng},${lat}`)
    localStorage.setItem('zoom', map.getZoom().toString())
  }

  const addShapes = (shapes: Shapes, modes: Record<number, Set<string>>) => {
    for (const [shapeId, points] of Object.entries(shapes)) {
      const line = shapeId.split("_")[0]

      let skip = false
      if (line[0] === 'T') {
        for (let i = 1; i < line.length; i++) {
          if (!modes[ModeType.RAIL].has(line[0] + line[i])) skip = true
        }
      } else if (line[0] === 'M') {
        for (let i = 1; i < line.length; i++) {
          if (!modes[ModeType.METRO].has(line[0] + line[i])) skip = true
        }
      } else if (line[0] === 'L') {
        for (let i = 1; i < line.length; i++) {
          if (!modes[ModeType.LIGHT_RAIL].has(line[0] + line[i])) skip = true
        }
      }
      if (skip) continue
      
      const coords = points.map((point: ShapeCoord) => [
        point.shapePtLon,
        point.shapePtLat
      ])

      const sourceId = `${shapeId}-shape`

      if (!map.getSource(sourceId)) {
        map.addSource(sourceId, {
          type: 'geojson',
          data: {
            type: "FeatureCollection",
            features: [{
              type: 'Feature',
              properties: { line },
              geometry: {
                type: 'LineString',
                coordinates: coords
              }
            }]
          }
        })
      }

      const colours = getRouteColours(line)

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

  const addStops = (stops: Stops, modes: Record<number, Set<string>>) => {
    Object.keys(modes).forEach((mode) => {
      const modeText = ModeLabels[Number(mode)]

      let platformFeatures: Feature<Point>[] = []
      Object.entries(stops)
        .filter(([stopsMode, _stops]) => stopsMode === modeText)
        .forEach(([_stopsMode, modeStops]) => {
          platformFeatures = modeStops.filter((stop) => stop.stopParentStation).map((stop) => ({
            type: 'Feature',
            properties: {
              stop: stop
            },
            geometry: {
              type: 'Point',
              coordinates: [stop.stopLon, stop.stopLat]
            }
          }))
        })

      let stationFeatures: Feature<Point>[] = []
      Object.entries(stops)
        .filter(([stopsMode, _stops]) => stopsMode === modeText)
        .forEach(([_stopsMode, modeStops]) => {
          stationFeatures = modeStops.filter((stop) => !stop.stopParentStation).map((stop) => ({
            type: 'Feature',
            properties: {
              stop: stop
            },
            geometry: {
              type: 'Point',
              coordinates: [stop.stopLon, stop.stopLat]
            }
          }))
        })

      map.addSource(`${modeText}-platforms-source`, {
        type: 'geojson',
        data: {
          type: 'FeatureCollection',
          features: platformFeatures
        }
      })

      map.addLayer({
        id: `${modeText}-platforms-layer`,
        type: 'symbol',
        source: `${modeText}-platforms-source`,
        layout: {
          'icon-image': `${modeText}-icon`,
          'icon-size': 0.06,
          'icon-allow-overlap': true
        },
        minzoom: 17
      })

      map.addSource(`${modeText}-stations-source`, {
        type: 'geojson',
        data: {
          type: 'FeatureCollection',
          features: stationFeatures
        }
      })

      map.addLayer({
        id: `${modeText}-stations-layer`,
        type: 'symbol',
        source: `${modeText}-stations-source`,
        layout: {
          'icon-image': `${modeText}-icon`,
          'icon-size': 0.06,
          'icon-allow-overlap': false
        },
        maxzoom: 16.99
      })
    })
  }

  const initVehicles = (modes: Record<number, Set<string>>) => {
    Object.entries(modes).forEach(([_mode, lines]) => {
      lines.forEach((line) => {
        if (!/^[TML](\d||CC)$/.test(line)) return

        map.addSource(`${line}-vehicle-source`, {
          type: 'geojson',
          data: {
            type: 'FeatureCollection',
            features: []
          }
        })

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
      })
    })
  }

  const updateVehicles = (vehicles: Vehicles, modes: Record<number, Set<string>>) => {
    Object.entries(modes).forEach(([mode, lines]) => {false
      lines.forEach((line) => {
        if (!vehicles[line]) return
        const vehicleFeatures: Feature<Point>[] = vehicles[line].map((vehicle) => ({
          type: 'Feature',
          properties: {
            type: 'vehicle',
            id: vehicle.vehicleId,
            tripId: vehicle.tripId,
            tripRouteId: vehicle.tripRouteId,
            vehicleLabel: vehicle.vehicleLabel,
            vehicleModel: vehicle.vehicleModel,
            stopId: vehicle.stopId,
            congestionLevel: vehicle.congestion_level,
            occupancyStatus: vehicle.occupancy_status,
            mode: ModeLabels[Number(mode)]
          },
          geometry: {
            type: 'Point',
            coordinates: [
              vehicle.positionLongitude,
              vehicle.positionLatitude
            ]
          }
        }))

        const source = map.getSource(`${line}-vehicle-source`) as maplibregl.GeoJSONSource

        if (source) {
          source.setData({
            type: 'FeatureCollection',
            features: vehicleFeatures
          })
        }
      })
    })
  }

  const clearShapes = (modes: Record<number, Set<string>>) => {
    const style = map.getStyle()
    if (!style) return

    const activeLines = new Set(Object.values(modes).flatMap((lines) => [...lines]))

    style.layers
      .filter(layer => layer.id.includes('-shape'))
      .forEach(layer => {
        const shapeId = layer.id.replace(/-shape.*$/, '')
        const line = shapeId[0] + shapeId[1]
        if (!activeLines.has(line) && map.getLayer(layer.id)) {
          map.removeLayer(layer.id)
        }
      })

    Object.keys(style.sources)
      .filter(sourceId => sourceId.includes('-shape'))
      .forEach(sourceId => {
        const shapeId = sourceId.replace(/-shape.*$/, '')
        const line = shapeId[0] + shapeId[1]
        if (!activeLines.has(line) && map.getSource(sourceId)) {
          map.removeSource(sourceId)
        }
      })
  }

  onMount(() => {
    console.log($modes, $shapes, $stops, $vehicles)
    const { centre, zoom } = getStoredView()

    map = new maplibregl.Map({
      container: 'map',
      style: `https://api.maptiler.com/maps/019d4395-1a2c-7b23-a73e-bacf70f86ef7/style.json?key=${import.meta.env.VITE_MAPTILER_KEY}`,
      center: centre,
      zoom: zoom
    })

    map.addControl(new maplibregl.NavigationControl())

    map.on('load', async () => {
      addShapes($shapes, $modes)

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

      // addStops($stops, $modes)
      // initVehicles($modes)
      // updateVehicles($vehicles, $modes)
  
      subModes = modes.subscribe((m) => {
        clearShapes(m)
        addShapes($shapes, m)
      })
      subVehicles = vehicles.subscribe((v) => updateVehicles(v, $modes))
    })

    map.on('click', (e) => {
      const features = map.queryRenderedFeatures(e.point)

      if (features.length > 0) {
        console.log('Clicked:', features[0].properties)
        selectedFeature = features[0].properties

        if (selectedFeature.type === 'vehicle') {
          Object.entries($modes).forEach(([_mode, lines]) => {
            lines.forEach((line) => {
              if (!/^[TML](\d||CC)$/.test(line)) return
              map.setPaintProperty(`${line}-vehicle-layer`, 'circle-radius', [
                'case',
                ['==', ['get', 'id'], selectedFeature.id],
                10,
                6
              ]);
            })
          })
        }
      }
    })

    map.on('moveend', () => {
      saveView()
    })
  })

  onDestroy(() => {
    subModes?.()
    subVehicles?.()
    map?.remove()
  })
</script>

<div class="relative w-screen h-screen">
  <div id="map" class="w-full h-full"></div>
  {#if selectedFeature}
    <MapSidebar selectedFeature={selectedFeature} />
  {/if}
  <TransportModes />
</div>
