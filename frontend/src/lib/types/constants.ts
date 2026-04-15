export const ROUTE_TYPE_LIGHT_RAIL = 0
export const ROUTE_TYPE_METRO = 401
export const ROUTE_TYPE_RAIL = 2
export const ROUTE_TYPE_BUS = 3
export const ROUTE_TYPE_FERRY = 4
export const ROUTE_TYPE_REGIONAL_RAIL = 105

export const modeMap: Record<number, string> = {
    0: 'lightrail',
    401: 'metro',
    2: 'sydneytrains',
    3: 'bus',
    4: 'ferry',
    105: 'regionalrail',
}

export const vehiclesMap: Record<string, string> = {
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

export const coloursMap: Record<string, string> = {
    'BMT': '#F99D1C',
    'NSN': '#F99D1C',
    'RTTA': '#F99D1C',
    'WST': '#F99D1C',
    'IWL': '#0098CD',
    'T3': '#F37021',
    'ESI': '#005AA3',
    'SCO': '#005AA3',
    'CMB': '#C4258F',
    'T6': '#7C3E21',
    'OLY': '#6F818E',
    'APS': '#00954C',
    'SHL': '#00954C',
    'NTH': '#D11F2F',
    'CCN': '#D11F2F',
    'CTY': '#F6891F',
    'HUN': '#833134',
    'M1': '#168388',
    'T4': '#005AA3',
}

export const routesMap: Record<string, string> = {
    'BMT': 'BMT',
    'NSN': 'T1',
    'RTTA': 'T1',
    'WST': 'T1',
    'IWL': 'T2',
    'T3': 'T3',
    'ESI': 'T4',
    'SCO': 'T4',
    'CMB': 'T5',
    'T6': 'T6',
    'OLY': 'T7',
    'APS': 'T8',
    'SHL': 'SHL',
    'NTH': 'T9',
    'CCN': 'CCN',
    'CTY': 'CTY',
    'HUN': 'HUN',
    'M1': 'M1',
}

export const days = ['sunday', 'monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday']

export const occupancyColours: Record<string, string> = {
    'EMPTY': '#22C55E',
    'MANY_SEATS_AVAILABLE': '#16A34A',
    'FEW_SEATS_AVAILABLE': '#FACC15',
    'STANDING_ROOM_ONLY': '#F97316',
    'CRUSHED_STANDING_ROOM_ONLY': '#EF4444',
    'FULL': '#B91C1C',
    'NOT_ACCEPTING_PASSENGERS': '#6B7280',
}