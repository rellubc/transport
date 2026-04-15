<script lang="ts">
  import { onDestroy, onMount } from 'svelte'
  import maplibregl from 'maplibre-gl'
  import 'maplibre-gl/dist/maplibre-gl.css'
  import type { Feature, Point } from 'geojson'

  import metroImg from '$lib/assets/metro.png'
  import sydneytrainsImg from '$lib/assets/sydneytrains.png'
  import { modes, shapes, stops, vehicles } from '$lib/stores'
  import { coloursMap, modeMap, routesMap } from '$lib/types/constants'
  import type { Vehicles } from '$lib/types/realtime'
  import type { ShapeCoord, Shapes } from '$lib/types/shape';
  import type { Stop, Stops } from '$lib/types/stop';
  import MapSidebar from './MapSidebar.svelte';

  const icons = [
    { name: 'metro-icon', url: metroImg },
    { name: 'sydneytrains-icon', url: sydneytrainsImg }
  ]

  let map!: maplibregl.Map
  let subVehicles: (() => void)

  let selectedFeature: any = $state(null)

  const getStoredView = () => {
    const storedCentre = localStorage.getItem('centre')
    const centre: maplibregl.LngLatLike = storedCentre ? [Number(storedCentre.split(',')[0]), Number(storedCentre.split(',')[1])] : [151.05, -33.82]
    const storedZoom = localStorage.getItem('zoom')
    const zoom = storedZoom ? Number(storedZoom) : 11.8
  
    return { centre, zoom }
  }

  const saveView = () => {
    const centre = map.getCenter()
    const zoom = map.getZoom()

    localStorage.setItem('centre', `${centre.lng},${centre.lat}`)
    localStorage.setItem('zoom', zoom.toString())
  }

  const addShapes = (shapes: Shapes, modes: Record<number, Set<string>>) => {
    Object.entries(modes).forEach(([_mode, lines]) => {
      const coordinates: number[][][] = []
      lines.forEach((line) => {
        Object.entries(shapes)
          .filter(([shapeId]) => shapeId.startsWith(line + "_"))
          .forEach(([_, points]) => {
            const coords = points.map((point: ShapeCoord) => [
              point.shapePtLon,
              point.shapePtLat
            ])

            coordinates.push(coords)
          })

        map.addSource(`${line}-shape`, {
          type: 'geojson',
          data: {
            type: "FeatureCollection",
            features: [{
              type: 'Feature',
              properties: {
                line
              },
              geometry: {
                type: 'MultiLineString',
                coordinates
              }
            }]
          }
        })

        map.addLayer({
          id: `${line}-shape`,
          type: 'line',
          source: `${line}-shape`,
          layout: {
            'line-join': 'round',
            'line-cap': 'round'
          },
          paint: {
            'line-color': coloursMap[line],
            'line-width': 2
          }
        })
      })
    })
  }

  const addStops = (stops: Stops, modes: Record<number, Set<string>>) => {
    Object.keys(modes).forEach((mode) => {
      const modeText = modeMap[Number(mode)]

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

      map.addSource(`${mode}-platforms-source`, {
        type: 'geojson',
        data: {
          type: 'FeatureCollection',
          features: platformFeatures
        }
      })

      map.addLayer({
        id: `${mode}-platforms-layer`,
        type: 'symbol',
        source: `${mode}-platforms-source`,
        layout: {
          'icon-image': 'sydneytrains-icon',
          'icon-size': 0.06,
          'icon-allow-overlap': true
        },
        minzoom: 17
      })

      map.addSource(`${mode}-stations-source`, {
        type: 'geojson',
        data: {
          type: 'FeatureCollection',
          features: stationFeatures
        }
      })

      map.addLayer({
        id: `${mode}-stations-layer`,
        type: 'symbol',
        source: `${mode}-stations-source`,
        layout: {
          'icon-image': 'sydneytrains-icon',
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
            'circle-color': coloursMap[line],
            'circle-stroke-width': 1,
            'circle-stroke-color': '#FFFFFF'
          },
        })
      })
    })
  }

  const updateVehicles = (vehicles: Vehicles, modes: Record<number, Set<string>>) => {
    Object.entries(modes).forEach(([_mode, lines]) => {
      lines.forEach((line) => {
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
            consist: vehicle.consist
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
        icons.map(async ({ name, url }) => {
          const img = new Image()
          img.onload = () => {
            map.addImage(name, img)
          }
          img.src = url
        }
      ))

      addStops($stops, $modes)

      initVehicles($modes)
      updateVehicles($vehicles, $modes)
  
      subVehicles = vehicles.subscribe((v) => {
        updateVehicles(v, $modes)
      })
    })

    map.on('click', (e) => {
      const features = map.queryRenderedFeatures(e.point)

      if (features.length > 0) {
        console.log('Clicked:', features[0].properties)
        selectedFeature = features[0].properties

        if (selectedFeature.type === 'vehicle') {
          Object.entries($modes).forEach(([_mode, lines]) => {
            lines.forEach((line) => {
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
    subVehicles?.()
    map?.remove()
  })
</script>

<div class="relative w-screen h-screen">
  <div id="map" class="w-full h-full"></div>
  {#if selectedFeature}
    <MapSidebar selectedFeature={selectedFeature} />
  {/if}
</div>
