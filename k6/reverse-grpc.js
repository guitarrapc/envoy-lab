import grpc from 'k6/net/grpc';
import { check } from "k6";

export const options = {
  vus: 200,
  duration: "1m",
};

const client = new grpc.Client();
client.load(['proto'], 'reverse.proto');

export default () => {
  client.connect('envoy:8080', {
    plaintext: true
  });

  const data = { content: 'hello' };
  const params = {
    timeout: '120s',
  }
  const response = client.invoke('api.Reverse/Reverse', data, params);

  check(response, {
    'is status OK': (r) => r && r.status === grpc.StatusOK,
  });

};

client.close();
