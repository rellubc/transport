import { Shapes } from "../../shared/models/shape"

export const getSydneyShapes = async (mode: string): Promise<Shapes>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/shapes?mode=${mode}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Shapes = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return {}
  }
}