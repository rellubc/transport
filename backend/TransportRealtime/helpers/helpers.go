package helpers

import (
	"fmt"
	"regexp"
)

var nonAlnum = regexp.MustCompile(`[^a-zA-Z0-9]+`)

func Clean(s string) string {
	return nonAlnum.ReplaceAllString(s, "")
}

func secondsToTime(sec int) string {
	h := sec / 3600
	m := (sec % 3600) / 60
	s := sec % 60
	return fmt.Sprintf("%02d:%02d:%02d", h, m, s)
}
