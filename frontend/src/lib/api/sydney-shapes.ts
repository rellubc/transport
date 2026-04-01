import type { Shapes } from "$lib/types/shape"

export const getSydneyShapes = async (fetchFn: typeof fetch, mode: string): Promise<Shapes>=> {
  try {
    const res = await fetchFn(`https://localhost:7002/api/sydney/shapes?mode=${mode}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Shapes = await res.json()

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return {}
  }
}