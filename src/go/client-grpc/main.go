package main

import (
	"context"
	"encoding/json"
	"flag"
	"fmt"
	"log"
	"net/http"
	"os"
	"time"

	pb "github.com/guitarrapc/envoy-lab/src/go/client-grpc/api"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
	"google.golang.org/grpc/metadata"
)

func main() {
	addr := flag.String("addr", "localhost:8080", "The address to connect to (host:port)")
	flag.Parse()

	if *addr == "" {
		fmt.Println("Usage of the program:")
		flag.PrintDefaults()
		os.Exit(1)
	}

	// Web API
	url := fmt.Sprintf("http://%s/weatherforecast", *addr)
	httpEchoRequest(url)

	// gRPC
	conn, err := grpc.NewClient(*addr, grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		log.Fatalf("did not connect: %v", err)
	}
	defer conn.Close()

	grpcEchoRequest(conn)
	grpcReverseRequest(conn)
}

func httpEchoRequest(url string) {
	resp, err := http.Get(url)
	if err != nil {
		log.Fatalf("Failed to make request: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		log.Fatalf("Request failed with status: %s", resp.Status)
	}

	// deserialize
	var forecasts []WeatherForecast
	if err := json.NewDecoder(resp.Body).Decode(&forecasts); err != nil {
		log.Fatalf("Failed to decode response: %v", err)
	}

	// serialize again
	jsonOutput, err := json.Marshal(forecasts)
	if err != nil {
		log.Fatalf("Failed to marshal response: %v", err)
	}

	log.Printf("Http: %s", jsonOutput)
	log.Printf("  Unmarshalled:")

	for _, forecast := range forecasts {
		log.Printf("    * Date: %s, TemperatureC: %d, TemperatureF: %d, Summary: %s\n",
			forecast.Date.Format(time.RFC3339), forecast.TemperatureC, forecast.TemperatureF, forecast.Summary)
	}
}

func grpcEchoRequest(conn *grpc.ClientConn) {
	c := pb.NewEchoClient(conn)

	ctx, cancel := context.WithTimeout(context.Background(), time.Second)
	defer cancel()

	var headers metadata.MD

	r, err := c.Echo(ctx, &pb.EchoRequest{Content: "hello"}, grpc.Header(&headers))
	if err != nil {
		log.Fatalf("could not greet: %v", err)
	}

	var h string
	if values := headers["custom-header"]; len(values) > 0 {
		h = values[0]
	}
	log.Printf("gRPC Echo: %s, Header: %s", r.Content, h)

	logHeaders(headers)
}

func grpcReverseRequest(conn *grpc.ClientConn) {
	c := pb.NewReverseClient(conn)

	ctx, cancel := context.WithTimeout(context.Background(), time.Second)
	defer cancel()

	var headers metadata.MD

	r, err := c.Reverse(ctx, &pb.ReverseRequest{Content: "hello"}, grpc.Header(&headers))
	if err != nil {
		log.Fatalf("could not greet: %v", err)
	}

	var h string
	if values := headers["custom-header"]; len(values) > 0 {
		h = values[0]
	}
	log.Printf("gRPC Reverse: %s, Header: %s", r.Content, h)

	logHeaders(headers)
}

func logHeaders(headers metadata.MD) {
	log.Printf("  Header:")
	for k, v := range headers {
		log.Printf("    * %s: %s", k, v[0])
	}
}
