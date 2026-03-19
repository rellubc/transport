import { VehiclePosition } from "../../shared/models/realtime"

export const getSydneyVehiclePositions = async (mode: string): Promise<VehiclePosition[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/realtime-vehicle-positions?mode=${mode}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: VehiclePosition[] = await res.json()

    data.map((vehicle) => {
      if (vehicle.timestamp) {
        vehicle.timestamp = new Date(Number(vehicle.timestamp) * 1000)
      }
    })

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}