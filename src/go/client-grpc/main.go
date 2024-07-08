package main

import (
	"context"
	"log"
	"time"

	pb "github.com/guitarrapc/envoy-lab/src/go/client-grpc/api"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
	"google.golang.org/grpc/metadata"
)

func main() {
	conn, err := grpc.NewClient("localhost:8081", grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		log.Fatalf("did not connect: %v", err)
	}
	defer conn.Close()

	echoRequest(conn)
	reverseRequest(conn)
}

func echoRequest(conn *grpc.ClientConn) {
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
	log.Printf("Echo: %s, Header: %s", r.Content, h)

	logHeaders(headers)
}

func reverseRequest(conn *grpc.ClientConn) {
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
	log.Printf("Reverse: %s, Header: %s", r.Content, h)

	logHeaders(headers)
}

func logHeaders(headers metadata.MD) {
	log.Printf("  Header:")
	for k, v := range headers {
		log.Printf("    * %s: %s", k, v[0])
	}
}
