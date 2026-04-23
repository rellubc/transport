import { LineColours, ModeType } from "./constants"

export const secondsToTime = (seconds: number) => {
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = seconds % 60
  return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`
}

export const stopDelay = (seconds: number) => {
  const delay = Math.floor(Math.abs(seconds) / 60)
  if (delay < 0) return `${delay}m early`
  else if (delay > 0) return `${delay}m late`
  else return 'On time'
}

export const getRoutes = (line: string): Set<string> => {
  const route = line.split('_')[0]
  const found: Set<string> = new Set()
  for (let i = 1; i < route.length; i++) {
    const key = route[0] + route[i]
    found.add(key)
  }

  return found
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

export const checkDisabled = (line: string, modes: Record<number, Set<string>>) => {
  if (line[0] === 'T') {
    for (let i = 1; i < line.length; i++) {
      if (modes[ModeType.RAIL].has(line[0] + line[i])) return false
    }
  } else if (line[0] === 'M') {
    for (let i = 1; i < line.length; i++) {
      if (modes[ModeType.METRO].has(line[0] + line[i])) return false
    }
  } else if (line[0] === 'L') {
    for (let i = 1; i < line.length; i++) {
      if (modes[ModeType.LIGHT_RAIL].has(line[0] + line[i])) return false
    }
  }
  return true
}