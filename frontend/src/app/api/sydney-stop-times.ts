import { StopTime } from "../../shared/models/stopTime"

export const getSydneyStopTimes = async (mode: string, stopName: string, timeString: string, before: boolean): Promise<StopTime[]>=> {
  try {
    const res = await fetch(`https://localhost:7284/api/sydney/stop-times?mode=${mode}&stopName=${stopName}&timeString=${timeString}&before=${before}`)
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