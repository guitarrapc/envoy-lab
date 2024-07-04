# syntax=docker/dockerfile:1

# Build the application from source
FROM golang:1.21 as build

WORKDIR /app

COPY go.mod go.sum ./
RUN go mod download

COPY . ./
RUN CGO_ENABLED=0 GOOS=linux go build -o /grpc-server

# Deploy the application binary into a lean image
FROM gcr.io/distroless/base-debian11 AS build-release-stage

WORKDIR /
ENV PORT=8080
EXPOSE 8080

COPY --from=build /grpc-server /grpc-server

USER nonroot:nonroot
ENTRYPOINT ["/grpc-server"]
