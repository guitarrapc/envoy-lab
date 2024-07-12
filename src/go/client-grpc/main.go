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
	httpEchoRequest(url, map[string]string{})

	// gRPC
	conn, err := grpc.NewClient(*addr, grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		log.Fatalf("did not connect: %v", err)
	}
	defer conn.Close()

	grpcEchoRequest(conn, map[string]string{})
	grpcReverseRequest(conn, map[string]string{})

	// dynamic
	httpEchoRequest(url, map[string]string{"X-Host-Port": "service:8080"})
	grpcEchoRequest(conn, map[string]string{"X-Host-Port": "echo-grpc:8080"})       // dynamic cluster
	grpcReverseRequest(conn, map[string]string{"X-Host-Port": "reverse-grpc:8080"}) // dynamic cluster
}

func httpEchoRequest(url string, reqHeaders map[string]string) {
	client := http.Client{}
	req, _ := http.NewRequest("GET", url, nil)
	for k, v := range reqHeaders {
		req.Header.Set(k, v)
	}
	resp, err := client.Do(req)
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

	h := resp.Header.Get("custom-header")

	log.Printf("Http CustomHeader: %s", h)
	log.Printf("  Marshalled: %s", jsonOutput)
	log.Printf("  Unmarshalled:")

	for _, forecast := range forecasts {
		log.Printf("    * Date: %s, TemperatureC: %d, TemperatureF: %d, Summary: %s\n",
			forecast.Date.Format(time.RFC3339), forecast.TemperatureC, forecast.TemperatureF, forecast.Summary)
	}

	logHttpHeaders(reqHeaders, resp.Header)
}

func grpcEchoRequest(conn *grpc.ClientConn, reqHeaders map[string]string) {
	c := pb.NewEchoClient(conn)

	ctx, cancel := context.WithTimeout(context.Background(), time.Second)
	defer cancel()
	md := metadata.New(reqHeaders)
	ctx = metadata.NewOutgoingContext(ctx, md)

	var resHeaders metadata.MD

	r, err := c.Echo(ctx, &pb.EchoRequest{Content: "hello"}, grpc.Header(&resHeaders))
	if err != nil {
		log.Fatalf("could not greet: %v", err)
	}

	var h string
	if values := resHeaders["custom-header"]; len(values) > 0 {
		h = values[0]
	}
	log.Printf("gRPC Echo: %s, CustomHeader: %s", r.Content, h)

	logGrpcHeaders(reqHeaders, resHeaders)
}

func grpcReverseRequest(conn *grpc.ClientConn, reqHeaders map[string]string) {
	c := pb.NewReverseClient(conn)

	ctx, cancel := context.WithTimeout(context.Background(), time.Second)
	defer cancel()
	md := metadata.New(reqHeaders)
	ctx = metadata.NewOutgoingContext(ctx, md)

	var resHeaders metadata.MD

	r, err := c.Reverse(ctx, &pb.ReverseRequest{Content: "hello"}, grpc.Header(&resHeaders))
	if err != nil {
		log.Fatalf("could not greet: %v", err)
	}

	var h string
	if values := resHeaders["custom-header"]; len(values) > 0 {
		h = values[0]
	}
	log.Printf("gRPC Reverse: %s, CustomHeader: %s", r.Content, h)

	logGrpcHeaders(reqHeaders, resHeaders)
}

func logHttpHeaders(reqHeaders map[string]string, resHeaders http.Header) {
	log.Printf("  Request Header:")
	for k, v := range reqHeaders {
		log.Printf("    * %s: %s", k, v)
	}

	log.Printf("  Response Header:")
	for k, v := range resHeaders {
		log.Printf("    * %s: %s", k, v[0])
	}
}

func logGrpcHeaders(reqHeaders map[string]string, resHeaders metadata.MD) {
	log.Printf("  Request Header:")
	for k, v := range reqHeaders {
		log.Printf("    * %s: %s", k, v)
	}

	log.Printf("  Response Header:")
	for k, v := range resHeaders {
		log.Printf("    * %s: %s", k, v[0])
	}
}
