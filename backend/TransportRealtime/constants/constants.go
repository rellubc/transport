package constants

type ModeType int

const (
	LightrailNewcastle ModeType = 0
	Metro              ModeType = 401
	SydneyTrains       ModeType = 2
	Buses              ModeType = 3
	Ferry              ModeType = 4
	LightrailSydney    ModeType = 900
)

var M1ShapeIds = []int{
	3722,
	16714,
	16760,
	16803,
	16815,
	17811,
	17928,
	18173,
	21005,
	21115,
	21141,
	21164,
	21180,
	21267,
	21273,
	21329,
	21342,
	21531,
	21545,
	21690,
}

var RoutesLookup = map[string]string{
	"WST": "T1",
	"NSN": "T1",
	"IWL": "T2",
	"T3":  "T3",
	"ESI": "T4",
	"CMB": "T5",
	"T6":  "T6",
	"OLY": "T7",
	"APS": "T8",
	"NTH": "T9",

	"BMT": "BMT",
	"CCN": "CCN",
	"SCO": "SCO",
	"SHL": "SHL",

	"M1": "M1",

	"IWLR": "L1",
	"L2":   "L2",
	"L3":   "L3",
	"L4":   "L4",

	"MFF": "MFF",
	"F1":  "F1",
	"F2":  "F2",
	"F3":  "F3",
	"F4":  "F4",
	"F5":  "F5",
	"F6":  "F6",
	"F7":  "F7",
	"F8":  "F8",
	"F9":  "F9",
	"F10": "F10",
}
