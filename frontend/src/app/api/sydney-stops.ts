import { Stop } from "../../shared/models/stop"

export const getSydneyStops = async (mode: string): Promise<Stop[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/stops?mode=${mode}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Stop[] = await res.json()
    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}