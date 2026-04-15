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
