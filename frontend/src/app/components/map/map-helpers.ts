import { StopTime } from "../../../shared/models/stopTime"

export const getDepartures = async (stopName: string): Promise<StopTime[]> => {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/combined/stop-times?stopName=${stopName}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StopTime[] = await res.json()

    data.sort((a, b) => {
      const normalise = (time: string) => {
        let temp = parseInt(time.substring(0, 2))
        if (temp < 4) {
          temp += 24
          time = temp.toString() + time.substring(2, time.length - 1)
        }
        return time
      }
      return normalise(a.arrivalTime).localeCompare(normalise(b.arrivalTime))
    })

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}

export const getSydneyMetroStopTimes = async (stopId: string): Promise<StopTime[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/metro/stop-times?stopId=${stopId}`)
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: StopTime[] = await res.json()

    data.sort((a, b) => {
      const normalise = (time: string) => {
        let temp = parseInt(time.substring(0, 2))
        if (temp < 4) {
          temp += 24
          time = temp.toString() + time.substring(2, time.length - 1)
        }
        return time
      }
      return normalise(a.arrivalTime).localeCompare(normalise(b.arrivalTime))
    })

    return data
  } catch (error) {
    console.error('Fetch failed:', error)
    return []
  }
}