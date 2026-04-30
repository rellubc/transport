export const BASE_URL = import.meta.env.VITE_API_BASE_URL

export const ModeType = {
  LIGHT_RAIL: 0,
  METRO: 401,
  RAIL: 2,
  BUS: 3,
  FERRY: 4,
  REGIONAL_RAIL: 105,
} as const

export const ModeLabels: Record<number, string> = {
  0: 'lightrail/newcastle',
  401: 'metro',
  2: 'sydneytrains',
  3: 'bus',
  4: 'ferry',
  900: 'lightrail/sydney',
  9999: 'inactive'
}

export const VehicleLabels: Record<string, string> = {
  'T8': 'Tangara (T)',
  'A8': 'Waratah (A)',
  'B8': 'Waratah (B)',
  'D4': 'Mariyung (D)',
  'D6': 'Mariyung (D)',
  'D8': 'Mariyung (D)',
  'D10': 'Mariyung (D)',
  'H4': 'OSCAR (H)',
  'H8': 'OSCAR (H)',
  'M4': 'Millenium (M)',
  'M8': 'Millenium (M)',
}

export const LineRoutes: Record<string, string> = {
  "NSN": "T1",
  "WST": "T1",
  "IWL": "T2",
  "T3": "T3",
  "ESI": "T4",
  "CMB": "T5",
  "T6": "T6",
  "OLY": "T7",
  "APS": "T8",
  "NTH": "T9",

  "BMT": "BMT",
  "CCN": "CCN",
  'SCO': 'SCO',
  "SHL": "SHL",

  "RTTA": "NR",

  "M1": "M1",
}

export const LineColours: Record<string, string> = {
  'BMT': '#F99D1C',
  'NSN': '#F99D1C',
  'WST': '#F99D1C',
  'IWL': '#0098CD',
  'ESI': '#005AA3',
  'SCO': '#005AA3',
  'CMB': '#C4258F',
  'OLY': '#6F818E',
  'APS': '#00954C',
  'SHL': '#00954C',
  'NTH': '#D11F2F',
  'CCN': '#D11F2F',
  'CTY': '#F6891F',
  'HUN': '#833134',
  
  'RTTA': '#888888',

  'M1': '#168388',

  'T1': '#F99D1C',
  'T2': '#0098CD',
  'T3': '#F37021',
  'T4': '#005AA3',
  'T5': '#C4258F',
  'T6': '#7C3E21',
  'T8': '#00954C',
  'T9': '#D11F2F',

  'TCC': '#000000',

  'L1': '#BE1622',
  'L2': '#DD1E25',
  'L3': '#781140',
  'L4': '#BB2043',
  'NLR': '#EE343F',
}

export const OccupancyColours: Record<string, string> = {
  'EMPTY': '#22C55E',
  'MANY_SEATS_AVAILABLE': '#16A34A',
  'FEW_SEATS_AVAILABLE': '#FACC15',
  'STANDING_ROOM_ONLY': '#F97316',
  'CRUSHED_STANDING_ROOM_ONLY': '#EF4444',
  'FULL': '#B91C1C',
  'NOT_ACCEPTING_PASSENGERS': '#6B7280',
}