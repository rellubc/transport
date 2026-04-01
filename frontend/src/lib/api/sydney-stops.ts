import type { Stop } from "$lib/types/stop"

export const getSydneyStops = async (fetchFn: typeof fetch, mode: string): Promise<Stop[]>=> {
  try {
    const res = await fetchFn(`https://localhost:7002/api/sydney/stops?mode=${mode}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: Stop[] = await res.json()

    console.log(data)

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}