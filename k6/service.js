import http from "k6/http";
import { check } from "k6";

export const options = {
  vus: 200,
  duration: "1m",
};
export default function () {
  const res = http.get("http://envoy:8080/weatherforecast");
  check(res, {
    "is status 200": (r) => r.status === 200,
  });
}
