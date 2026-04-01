package realtime

import (
	pb "TransportRealtime/proto"
	"io"
	"net/http"

	"google.golang.org/protobuf/proto"
)

func fetchProtobuf(url, apiKey string) (*pb.FeedMessage, error) {
	data, err := doHTTPRequest(url, apiKey)
	if err != nil {
		return nil, err
	}

	feed := &pb.FeedMessage{}
	if err := proto.Unmarshal(data, feed); err != nil {
		return nil, err
	}

	return feed, nil
}

func doHTTPRequest(url, apiKey string) ([]byte, error) {
	req, err := http.NewRequest("GET", url, nil)
	if err != nil {
		return nil, err
	}

	req.Header.Add("Authorization", "apikey "+apiKey)
	req.Header.Add("Accept", "application/x-protobuf")

	client := &http.Client{}
	res, err := client.Do(req)
	if err != nil {
		return nil, err
	}
	defer res.Body.Close()

	return io.ReadAll(res.Body)
}
