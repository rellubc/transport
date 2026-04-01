<script lang="ts">
  import { onMount } from 'svelte'
  import maplibregl from 'maplibre-gl'
  import 'maplibre-gl/dist/maplibre-gl.css'
  import type { Feature, LineString, Point } from 'geojson';

  import type { Stop } from '$lib/types/stop';
  import metroImg from '$lib/assets/metro.png';
  import { lineShapes, modes, shapes, stops } from '$lib/stores';
  import { lineColours } from '$lib/constants';
  import type { ShapeDetails } from '$lib/types/shape';
  import { coloursMap } from '$lib/types/constants';

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

  const addShapeSource = (modeLine: string) => {
    map.addSource(`${modeLine}-shape`, {
      type: 'geojson',
      data: {
        type: "FeatureCollection",
        features: []
      }
    })

    map.addLayer({
      id: `${modeLine}-shape`,
      type: 'line',
      source: `${modeLine}-shape`,
      layout: {
        'line-join': 'round',
        'line-cap': 'round'
      },
      paint: {
        'line-color': coloursMap[modeLine],
        'line-width': 2
      }
    })
  }

  const addStopSource = () => {
    // sources
    map.addSource('stop-platforms', {
      type: 'geojson',
      data: {
        type: "FeatureCollection",
        features: []
      }
    })

    map.addSource('stop-stations', {
      type: 'geojson',
      data: {
        type: "FeatureCollection",
        features: []
      }
    })

    // layers
    map.addLayer({
      id: 'stop-platforms',
      type: 'symbol',
      source: 'stop-platforms',
      layout: {
        'icon-image': 'metro-icon',
        'icon-size': 0.07,
        'icon-allow-overlap': true
      }
    })

    map.addLayer({
      id: 'stop-stations',
      type: 'symbol',
      source: 'stop-stations',
      layout: {
        'icon-image': 'metro-icon',
        'icon-size': 0.07
      }
    })
  }

  const addVehicleSource = () => {

  }

  const addShapes = (modeLine: string) => {
    let lineFeatures: Feature<LineString, { id: string }>[] = []
    for (const line of $lineShapes[modeLine]) {
      const points = $shapes[line]
      lineFeatures.push({
        type: 'Feature',
        properties: {
          id: line
        },
        geometry: {
          type: 'LineString',
          coordinates: points.map((point) => [point.longitude, point.latitude])
        }
      })
    }

    const lineSource = map.getSource(`${modeLine}-shape`) as maplibregl.GeoJSONSource

    lineSource.setData({
      type: 'FeatureCollection',
      features: lineFeatures
    })
  }

  const addStops = () => {
    const platformFeatures: Feature<Point, { stop: Stop }>[] = $stops.filter((stop) => stop.parentStationId).map((stop) => ({
      type: 'Feature',
      properties: {
        stop: stop
      },
      geometry: {
        type: 'Point',
        coordinates: [stop.longitude, stop.latitude]
      }
    }))

    const stationFeatures: Feature<Point, { stop: Stop }>[] = $stops.filter((stop) => !stop.parentStationId).map((stop) => ({
      type: 'Feature',
      properties: {
        stop: stop
      },
      geometry: {
        type: 'Point',
        coordinates: [stop.longitude, stop.latitude]
      }
    }))

    const platformSource = map.getSource('stop-platforms') as maplibregl.GeoJSONSource

    platformSource.setData({
      type: 'FeatureCollection',
      features: platformFeatures
    })

    const stationSource = map.getSource('stop-stations') as maplibregl.GeoJSONSource

    stationSource.setData({
      type: 'FeatureCollection',
      features: stationFeatures
    })
  }

  const setStopVisibility = () => {
    const zoom = map.getZoom()
    map.setLayoutProperty('stop-platforms', 'visibility', zoom >= 17 ? 'visible' : 'none')
    map.setLayoutProperty('stop-stations', 'visibility', zoom < 17 ? 'visible' : 'none')
  }

  onMount(() => {
    console.log($modes, $lineShapes, $shapes, $stops)
    const { centre, zoom } = getStoredView()

    map = new maplibregl.Map({
      container: 'map',
      style: `https://api.maptiler.com/maps/019d4395-1a2c-7b23-a73e-bacf70f86ef7/style.json?key=${import.meta.env.VITE_MAPTILER_KEY}`,
      center: centre,
      zoom: zoom
    })

    map.addControl(new maplibregl.NavigationControl())

    map.on('load', () => {
      const img = new Image();
      img.onload = () => {
        map.addImage('metro-icon', img);
        addStopSource();
        addStops();
      }
      img.src = metroImg;

      Object.entries($modes).forEach(([mode, modeLines]) => {
        modeLines.forEach((modeLine) => {
          addShapeSource(modeLine)
          addShapes(modeLine)
        })
      })
      addVehicleSource()
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

    map.on('zoom', () => {
      setStopVisibility()
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