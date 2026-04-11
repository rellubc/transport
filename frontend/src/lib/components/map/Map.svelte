<script lang="ts">
  import { onMount } from 'svelte'
  import maplibregl from 'maplibre-gl'
  import 'maplibre-gl/dist/maplibre-gl.css'
  import type { Feature, Point } from 'geojson'

  import metroImg from '$lib/assets/metro.png'
  import railImg from '$lib/assets/rail.png'
  import { modes, shapes, stops, vehicles } from '$lib/stores'
  import { coloursMap } from '$lib/types/constants'
  import type { Vehicles } from '$lib/types/realtime'

  let map!: maplibregl.Map

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

  const addShapes = () => {
    Object.entries($modes).forEach(([_mode, lines]) => {
      const coordinates: number[][][] = []
      lines.forEach((line) => {
        Object.entries($shapes)
          .filter(([shapeId]) => shapeId.startsWith(line + "_"))
          .forEach(([_, points]) => {
            const coords = points.map(point => [
              point.shape_pt_lon,
              point.shape_pt_lat
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

  const addStops = () => {
    // const platformFeatures: Feature<Point>[] = $stops.filter((stop: Stop) => stop.stop_parent_station).map((stop: Stop) => ({
    //   type: 'Feature',
    //   properties: {
    //     stop: stop
    //   },
    //   geometry: {
    //     type: 'Point',
    //     coordinates: [stop.stop_lon, stop.stop_lat]
    //   }
    // }))

    // map.addSource('stop-platforms-source', {
    //   type: 'geojson',
    //   data: {
    //     type: 'FeatureCollection',
    //     features: platformFeatures
    //   }
    // })

    // map.addLayer({
    //   id: 'stop-platforms-layer',
    //   type: 'symbol',
    //   source: 'stop-platforms-source',
    //   layout: {
    //     'icon-image': 'metro-icon',
    //     'icon-size': 0.07,
    //     'icon-allow-overlap': true
    //   },
    //   minzoom: 17
    // })

    // const stationFeatures: Feature<Point>[] = $stops.filter((stop: Stop) => !stop.stop_parent_station).map((stop: Stop) => ({
    //   type: 'Feature',
    //   properties: {
    //     stop: stop
    //   },
    //   geometry: {
    //     type: 'Point',
    //     coordinates: [stop.stop_lon, stop.stop_lat]
    //   }
    // }))

    // map.addSource('stop-stations-source', {
    //   type: 'geojson',
    //   data: {
    //     type: 'FeatureCollection',
    //     features: stationFeatures
    //   }
    // })

    // map.addLayer({
    //   id: 'stop-stations-layer',
    //   type: 'symbol',
    //   source: 'stop-stations-source',
    //   layout: {
    //     'icon-image': 'metro-icon',
    //     'icon-size': 0.07,
    //     'icon-allow-overlap': true
    //   },
    //   maxzoom: 16.99
    // })
  }

  const initVehicles = () => {
    Object.entries($modes).forEach(([_mode, lines]) => {
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
            vehicle
          },
          geometry: {
            type: 'Point',
            coordinates: [
              vehicle.position_longitude,
              vehicle.position_latitude
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

    map.on('load', () => {
      const img = new Image()
      img.onload = () => {
        map.addImage('metro-icon', img)
        addStops()
      }
      img.src = metroImg

      addShapes()
      initVehicles()
      updateVehicles($vehicles, $modes)
  
      const unsubVehicles = vehicles.subscribe((v) => {
        if (!map || !map.isStyleLoaded()) return
        updateVehicles(v, $modes)
      })

      return () => {
        unsubVehicles?.()
        map?.remove()
      }
    })

    map.on('click', (e) => {
      const features = map.queryRenderedFeatures(e.point)

      if (features.length > 0) {
        console.log('Clicked:', features[0].properties)
      }
    })

    map.on('moveend', () => {
      saveView()
    })
  })
</script>

<div id="map"></div>

<style>
  #map {
    width: 100%;
    height: 100vh;
  }
</style>