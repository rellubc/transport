import { LineColours } from "./constants"

export const getSydneyNow = (): number => {
  const sydneyStr = new Date().toLocaleString('en-AU', { timeZone: 'Australia/Sydney' })
  return new Date(sydneyStr).getTime()
}

export const secondsToTime = (seconds: number): string => {
  seconds %= 86400
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = seconds % 60
  return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`
}

export const stopDelayText = (seconds: number): string => {
  const delay = Math.floor(Math.abs(seconds) / 60)
  if (delay < 0) return `${delay}m early`
  else if (delay > 0) return `${delay}m late`
  else return 'On time'
}

export const stopDelayColour = (seconds: number): string => {
  const delay = Math.floor(Math.abs(seconds) / 60)
  if (delay < 0) return '#0000FF'
  else if (delay > 0) return '#FF0000'
  else return '#00FF00'
}

export const getRouteColours = (line: string): Set<string> => {
  if (LineColours[line]) return new Set([LineColours[line]])

  const route = line.split('_')[0]
  if (LineColours[route]) return new Set([LineColours[route]])

  const found: Set<string> = new Set()
  for (let i = 1; i < route.length; i++) {
    const key = route[0] + route[i]
    if (LineColours[key] && !found.has(LineColours[key])) {
      found.add(LineColours[key])
    }
  }

  return found.size ? found : new Set(['#000000'])
}