package main

import (
	"context"
	"log"
	"time"

	pb "github.com/guitarrapc/envoy-lab/src/go/client-grpc/api"

	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
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

	r, err := c.Echo(ctx, &pb.EchoRequest{Content: "hello"})
	if err != nil {
		log.Fatalf("could not greet: %v", err)
	}
	log.Printf("Echo: %s", r.Content)
}

func reverseRequest(conn *grpc.ClientConn) {
	c := pb.NewReverseClient(conn)

	ctx, cancel := context.WithTimeout(context.Background(), time.Second)
	defer cancel()

	r, err := c.Reverse(ctx, &pb.ReverseRequest{Content: "hello"})
	if err != nil {
		log.Fatalf("could not greet: %v", err)
	}
	log.Printf("Reverse: %s", r.Content)
}
