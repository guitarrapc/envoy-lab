import http from "k6/http";
import { check } from "k6";

export const options = {
  vus: 200,
  duration: "1m",
};
export default function () {
  const params = {
    headers: {
      "X-Host-Port": "service:8080",
    },
  };
  const res = http.get("http://envoy:8080/weatherforecast", params);
  check(res, {
    "is status 200": (r) => r.status === 200,
  });
}
