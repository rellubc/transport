package helpers

import "regexp"

var nonAlnum = regexp.MustCompile(`[^a-zA-Z0-9]+`)

func Clean(s string) string {
	return nonAlnum.ReplaceAllString(s, "")
}
