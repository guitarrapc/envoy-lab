## envoy-lab

## Fundamentals

### Access Envoy admin running on Kubernetes

port forward to envoy pod's admin port.
```shell
kubectl port-forward $(kubectl get pods -o name | grep envoy | head -n1) 8080:9901
```

now you can acces to admin portal via `localhost:8080`

# Envoy Benchmark

> [!TIP]
> Configuration parctice.
>
> 1. max_connections > max_requests
> 1. max_pending_requests >= max_requests
> 1. max_requests >= system rps limit
> 1. overload_manager.resource_monitors.max_active_downstream_connections == max_connections

## Parrern 1 [vus: 100]

circuit-break

| max_connections | max_pending_requests | max_requests | overload_manager.resource_monitors.max_active_downstream_connections |
| --------------- | -------------------- | ------------ | ------------ |
| 100 | 100 | 100 | 10000 | 10000 |

result

* connection count: 100
* pending connections: 0
* request Rate: 50-100req/s
* pending requuests: 0
* request pending overflow: 0
* circuit-breaker: cx_open
* status code 200/503: 68.9k/0

![image](https://github.com/guitarrapc/envoy-lab/assets/3856350/558f9783-4e81-4d96-a1c9-d0379af23d6c)

```
k6-1            |      ✓ is status 200
k6-1            |
k6-1            |      checks.........................: 100.00% ✓ 5043681      ✗ 0
k6-1            |      data_received..................: 3.1 GB  52 MB/s
k6-1            |      data_sent......................: 459 MB  7.6 MB/s
k6-1            |      http_req_blocked...............: avg=1.39µs  min=261ns    med=911ns   max=17.91ms p(90)=1.4µs   p(95)=1.69µs
k6-1            |      http_req_connecting............: avg=78ns    min=0s       med=0s      max=17.35ms p(90)=0s      p(95)=0s
k6-1            |      http_req_duration..............: avg=1.13ms  min=154.27µs med=1.04ms  max=90.46ms p(90)=1.74ms  p(95)=2.02ms
k6-1            |        { expected_response:true }...: avg=1.13ms  min=154.27µs med=1.04ms  max=90.46ms p(90)=1.74ms  p(95)=2.02ms
k6-1            |      http_req_failed................: 0.00%   ✓ 0            ✗ 5043681
k6-1            |      http_req_receiving.............: avg=20.68µs min=4.54µs   med=14.27µs max=17.42ms p(90)=22.41µs p(95)=25.79µs
k6-1            |      http_req_sending...............: avg=6.81µs  min=1.36µs   med=4.7µs   max=17.76ms p(90)=6.66µs  p(95)=7.68µs
k6-1            |      http_req_tls_handshaking.......: avg=0s      min=0s       med=0s      max=0s      p(90)=0s      p(95)=0s
k6-1            |      http_req_waiting...............: avg=1.1ms   min=134.38µs med=1.02ms  max=90.25ms p(90)=1.71ms  p(95)=1.98ms
k6-1            |      http_reqs......................: 5043681 84057.594345/s
k6-1            |      iteration_duration.............: avg=1.18ms  min=177.01µs med=1.08ms  max=91.93ms p(90)=1.79ms  p(95)=2.09ms
k6-1            |      iterations.....................: 5043681 84057.594345/s
k6-1            |      vus............................: 100     min=100        max=100
k6-1            |      vus_max........................: 100     min=100        max=100
k6-1            |
k6-1            |
k6-1            | running (1m00.0s), 000/100 VUs, 5043681 complete and 0 interrupted iterations
k6-1            | default ✓ [ 100% ] 100 VUs  1m0s
```

## Parrern 2 [vus: 120]

circuit-break

| max_connections | max_pending_requests | max_requests | overload_manager.resource_monitors.max_active_downstream_connections |
| --------------- | -------------------- | ------------ | ------------ |
| 100 | 100 | 100 | 10000 | 10000 |

result

* connection count: 100
* pending connections: 5-9
* request Rate: 53req/s
* pending requuests: 5-9req/s
* request pending overflow: 834
* circuit-breaker: cx_open
* status code 200/503: 72.5k/823

![image](https://github.com/guitarrapc/envoy-lab/assets/3856350/6886bb66-7f70-412b-8118-725d7671dc9e)

```
k6-1            |      ✗ is status 200
k6-1            |       ↳  99% — ✓ 5204384 / ✗ 5863
k6-1            |
k6-1            |      checks.........................: 99.88%  ✓ 5204384      ✗ 5863
k6-1            |      data_received..................: 3.2 GB  53 MB/s
k6-1            |      data_sent......................: 474 MB  7.9 MB/s
k6-1            |      http_req_blocked...............: avg=1.83µs  min=260ns     med=892ns   max=21.94ms  p(90)=1.4µs   p(95)=1.7µs
k6-1            |      http_req_connecting............: avg=115ns   min=0s        med=0s      max=19.67ms  p(90)=0s      p(95)=0s
k6-1            |      http_req_duration..............: avg=1.31ms  min=18.37µs   med=1.19ms  max=102.98ms p(90)=2.06ms  p(95)=2.45ms
k6-1            |        { expected_response:true }...: avg=1.31ms  min=18.37µs   med=1.19ms  max=102.98ms p(90)=2.06ms  p(95)=2.45ms
k6-1            |      http_req_failed................: 0.11%   ✓ 5863         ✗ 5204384
k6-1            |      http_req_receiving.............: avg=22.45µs min=-430555ns med=13.83µs max=17.91ms  p(90)=22.33µs p(95)=26.33µs
k6-1            |      http_req_sending...............: avg=7.46µs  min=-688585ns med=4.6µs   max=17.5ms   p(90)=6.65µs  p(95)=7.74µs
k6-1            |      http_req_tls_handshaking.......: avg=0s      min=0s        med=0s      max=0s       p(90)=0s      p(95)=0s
k6-1            |      http_req_waiting...............: avg=1.28ms  min=0s        med=1.17ms  max=102.95ms p(90)=2.03ms  p(95)=2.4ms
k6-1            |      http_reqs......................: 5210247 86834.517309/s
k6-1            |      iteration_duration.............: avg=1.37ms  min=86.07µs   med=1.24ms  max=106.5ms  p(90)=2.13ms  p(95)=2.54ms
k6-1            |      iterations.....................: 5210247 86834.517309/s
k6-1            |      vus............................: 120     min=120        max=120
k6-1            |      vus_max........................: 120     min=120        max=120
k6-1            |
k6-1            |
k6-1            | running (1m00.0s), 000/120 VUs, 5210247 complete and 0 interrupted iterations
k6-1            | default ✓ [ 100% ] 120 VUs  1m0s
```

## Parrern 3 [vus: 200]

circuit-break

| max_connections | max_pending_requests | max_requests | overload_manager.resource_monitors.max_active_downstream_connections |
| --------------- | -------------------- | ------------ | ------------ |
| 100 | 100 | 100 | 10000 | 10000 |

result

* connection count: 100-103
* pending connections: 51-73
* request Rate: 91-99req/s
* pending requuests: 51-73req/s
* request pending overflow: 0
* circuit-breaker: cx_open
* status code 200/503: 80.6k/0

![image](https://github.com/guitarrapc/envoy-lab/assets/3856350/463eab77-429d-4d7e-8d41-80c83ad6222c)

```
k6-1            |      ✗ is status 200
k6-1            |       ↳  98% — ✓ 5813607 / ✗ 58792
k6-1            |
k6-1            |      checks.........................: 98.99%  ✓ 5813607      ✗ 58792
k6-1            |      data_received..................: 3.6 GB  60 MB/s
k6-1            |      data_sent......................: 534 MB  8.9 MB/s
k6-1            |      http_req_blocked...............: avg=2.63µs  min=250ns    med=892ns   max=31.69ms p(90)=1.37µs  p(95)=1.67µs
k6-1            |      http_req_connecting............: avg=143ns   min=0s       med=0s      max=26.5ms  p(90)=0s      p(95)=0s
k6-1            |      http_req_duration..............: avg=1.93ms  min=71.52µs  med=1.71ms  max=94.53ms p(90)=3.18ms  p(95)=3.83ms
k6-1            |        { expected_response:true }...: avg=1.94ms  min=138.77µs med=1.71ms  max=94.53ms p(90)=3.19ms  p(95)=3.84ms
k6-1            |      http_req_failed................: 1.00%   ✓ 58792        ✗ 5813607
k6-1            |      http_req_receiving.............: avg=39.24µs min=3.93µs   med=13.16µs max=22.81ms p(90)=21.34µs p(95)=25.34µs
k6-1            |      http_req_sending...............: avg=14.97µs min=1.22µs   med=4.52µs  max=27.29ms p(90)=6.54µs  p(95)=7.53µs
k6-1            |      http_req_tls_handshaking.......: avg=0s      min=0s       med=0s      max=0s      p(90)=0s      p(95)=0s
k6-1            |      http_req_waiting...............: avg=1.88ms  min=61.53µs  med=1.68ms  max=94.41ms p(90)=3.13ms  p(95)=3.75ms
k6-1            |      http_reqs......................: 5872399 97870.805367/s
k6-1            |      iteration_duration.............: avg=2.02ms  min=89.85µs  med=1.76ms  max=97.91ms p(90)=3.28ms  p(95)=4ms
k6-1            |      iterations.....................: 5872399 97870.805367/s
k6-1            |      vus............................: 200     min=200        max=200
k6-1            |      vus_max........................: 200     min=200        max=200
k6-1            |
k6-1            |
k6-1            | running (1m00.0s), 000/200 VUs, 5872399 complete and 0 interrupted iterations
k6-1            | default ✓ [ 100% ] 200 VUs  1m0s
```

## Parrern 4 [vus: 200]

circuit-break

| max_connections | max_pending_requests | max_requests | overload_manager.resource_monitors.max_active_downstream_connections |
| --------------- | -------------------- | ------------ | ------------ |
| 100 | 10 | 100 | 10000 | 10000 |

result

* connection count: 100
* pending connections: 11
* request Rate: 83-99req/s
* pending requuests: 11req/s
* request pending overflow: 45171
* circuit-breaker: cx_open, rq_pending_open
* status code 200/503: 46.7k/44.5k

![image](https://github.com/guitarrapc/envoy-lab/assets/3856350/ebe24d75-f1da-4931-a7a0-512e7ef64db1)

```
k6-1            |      ✗ is status 200
k6-1            |       ↳  49% — ✓ 3697531 / ✗ 3766028
k6-1            |
k6-1            |      checks.........................: 49.54%  ✓ 3697531       ✗ 3766028
k6-1            |      data_received..................: 3.3 GB  55 MB/s
k6-1            |      data_sent......................: 679 MB  11 MB/s
k6-1            |      http_req_blocked...............: avg=2.78µs  min=261ns     med=902ns   max=39.93ms  p(90)=1.42µs  p(95)=1.78µs
k6-1            |      http_req_connecting............: avg=117ns   min=0s        med=0s      max=27.15ms  p(90)=0s      p(95)=0s
k6-1            |      http_req_duration..............: avg=1.48ms  min=21.85µs   med=1.11ms  max=112.15ms p(90)=3.1ms   p(95)=3.96ms
k6-1            |        { expected_response:true }...: avg=2.15ms  min=140.1µs   med=1.81ms  max=112.15ms p(90)=3.85ms  p(95)=4.77ms
k6-1            |      http_req_failed................: 50.45%  ✓ 3766028       ✗ 3697531
k6-1            |      http_req_receiving.............: avg=33.98µs min=-375859ns med=12.81µs max=25.03ms  p(90)=20.31µs p(95)=25.56µs
k6-1            |      http_req_sending...............: avg=12.02µs min=1.26µs    med=4.62µs  max=28.56ms  p(90)=6.81µs  p(95)=8.08µs
k6-1            |      http_req_tls_handshaking.......: avg=0s      min=0s        med=0s      max=0s       p(90)=0s      p(95)=0s
k6-1            |      http_req_waiting...............: avg=1.43ms  min=0s        med=1.08ms  max=112.06ms p(90)=3.04ms  p(95)=3.88ms
k6-1            |      http_reqs......................: 7463559 124388.669136/s
k6-1            |      iteration_duration.............: avg=1.58ms  min=81.31µs   med=1.17ms  max=114.03ms p(90)=3.25ms  p(95)=4.21ms
k6-1            |      iterations.....................: 7463559 124388.669136/s
k6-1            |      vus............................: 200     min=200         max=200
k6-1            |      vus_max........................: 200     min=200         max=200
k6-1            |
k6-1            |
k6-1            | running (1m00.0s), 000/200 VUs, 7463559 complete and 0 interrupted iterations
k6-1            | default ✓ [ 100% ] 200 VUs  1m0s
```


## Parrern 5 [vus: 200]

circuit-break

| max_connections | max_pending_requests | max_requests | overload_manager.resource_monitors.max_active_downstream_connections |
| --------------- | -------------------- | ------------ | ------------ |
| 120 | 100 | 100 | 10000 | 10000 |

result

* connection count: 120
* pending connections: 60-68
* request Rate: 100req/s
* pending requuests: 68req/s
* request pending overflow: 9834
* circuit-breaker: cx_open, rq_open
* status code 200/503: 65-81k/9.83k

![image](https://github.com/guitarrapc/envoy-lab/assets/3856350/fd2b798a-7cb8-4457-b4b7-9cd5abc800d9)

```
k6-1            |      ✗ is status 200
k6-1            |       ↳  90% — ✓ 5710048 / ✗ 632795
k6-1            |
k6-1            |      checks.........................: 90.02%  ✓ 5710048       ✗ 632795
k6-1            |      data_received..................: 3.7 GB  62 MB/s
k6-1            |      data_sent......................: 577 MB  9.6 MB/s
k6-1            |      http_req_blocked...............: avg=2.59µs  min=260ns     med=872ns   max=28.79ms  p(90)=1.35µs  p(95)=1.66µs
k6-1            |      http_req_connecting............: avg=137ns   min=0s        med=0s      max=26.59ms  p(90)=0s      p(95)=0s
k6-1            |      http_req_duration..............: avg=1.78ms  min=-12544ns  med=1.57ms  max=111.48ms p(90)=3.06ms  p(95)=3.71ms
k6-1            |        { expected_response:true }...: avg=1.87ms  min=144.79µs  med=1.65ms  max=111.48ms p(90)=3.13ms  p(95)=3.79ms
k6-1            |      http_req_failed................: 9.97%   ✓ 632795        ✗ 5710048
k6-1            |      http_req_receiving.............: avg=33.21µs min=-296324ns med=12.95µs max=25.47ms  p(90)=20.96µs p(95)=25.07µs
k6-1            |      http_req_sending...............: avg=12.4µs  min=-313787ns med=4.49µs  max=19.97ms  p(90)=6.54µs  p(95)=7.55µs
k6-1            |      http_req_tls_handshaking.......: avg=0s      min=0s        med=0s      max=0s       p(90)=0s      p(95)=0s
k6-1            |      http_req_waiting...............: avg=1.74ms  min=0s        med=1.55ms  max=110.05ms p(90)=3.01ms  p(95)=3.64ms
k6-1            |      http_reqs......................: 6342843 105711.130132/s
k6-1            |      iteration_duration.............: avg=1.87ms  min=86.77µs   med=1.62ms  max=112.93ms p(90)=3.17ms  p(95)=3.89ms
k6-1            |      iterations.....................: 6342843 105711.130132/s
k6-1            |      vus............................: 200     min=200         max=200
k6-1            |      vus_max........................: 200     min=200         max=200
k6-1            |
k6-1            |
k6-1            | running (1m00.0s), 000/200 VUs, 6342843 complete and 0 interrupted iterations
k6-1            | default ✓ [ 100% ] 200 VUs  1m0s
```


## Parrern 6 [vus: 200]

circuit-break

| max_connections | max_pending_requests | max_requests | overload_manager.resource_monitors.max_active_downstream_connections |
| --------------- | -------------------- | ------------ | ------------ |
| 120 | 0 | 100 | 10000 | 10000 |

result

* connection count: 0
* pending connections: 0
* request Rate: 0req/s
* pending requuests: 0req/s
* request pending overflow: 138305
* circuit-breaker: -
* status code 200/503: 0/138k

![image](https://github.com/guitarrapc/envoy-lab/assets/3856350/fd2b798a-7cb8-4457-b4b7-9cd5abc800d9)

```
k6-1            |      ✗ is status 200
k6-1            |       ↳  0% — ✓ 0 / ✗ 10810588
k6-1            |
k6-1            |      checks.....................: 0.00%    ✓ 0             ✗ 10810588
k6-1            |      data_received..............: 3.0 GB   50 MB/s
k6-1            |      data_sent..................: 984 MB   16 MB/s
k6-1            |      http_req_blocked...........: avg=2.23µs   min=250ns   med=802ns    max=43.31ms p(90)=1.29µs  p(95)=1.64µs
k6-1            |      http_req_connecting........: avg=79ns     min=0s      med=0s       max=29.16ms p(90)=0s      p(95)=0s
k6-1            |      http_req_duration..........: avg=986.91µs min=61.19µs med=654.82µs max=42.9ms  p(90)=2.14ms  p(95)=2.86ms
k6-1            |      http_req_failed............: 100.00%  ✓ 10810588      ✗ 0
k6-1            |      http_req_receiving.........: avg=24.57µs  min=3.59µs  med=11.27µs  max=22.14ms p(90)=16.89µs p(95)=21.32µs
k6-1            |      http_req_sending...........: avg=9.01µs   min=1.24µs  med=4.29µs   max=18.6ms  p(90)=6.57µs  p(95)=7.71µs
k6-1            |      http_req_tls_handshaking...: avg=0s       min=0s      med=0s       max=0s      p(90)=0s      p(95)=0s
k6-1            |      http_req_waiting...........: avg=953.31µs min=51.92µs med=631.01µs max=42.88ms p(90)=2.09ms  p(95)=2.79ms
k6-1            |      http_reqs..................: 10810588 180171.202127/s
k6-1            |      iteration_duration.........: avg=1.09ms   min=81.77µs med=712.65µs max=44.87ms p(90)=2.32ms  p(95)=3.13ms
k6-1            |      iterations.................: 10810588 180171.202127/s
k6-1            |      vus........................: 200      min=200         max=200
k6-1            |      vus_max....................: 200      min=200         max=200
k6-1            |
k6-1            |
k6-1            | running (1m00.0s), 000/200 VUs, 10810588 complete and 0 interrupted iterations
k6-1            | default ✓ [ 100% ] 200 VUs  1m0s
```

# Envoy Benchmark - resource_limits.listener.example_listener_name.connection_limit

## Parrern 7 [vus: 200]

circuit-break

| max_connections | max_pending_requests | max_requests | overload_manager.resource_monitors.max_active_downstream_connections |
| --------------- | -------------------- | ------------ | ------------ |
| 100 | 100 | 100 | 10 | 10000 |

result

* connection count: 102
* pending connections: 92
* request Rate: 99req/s
* pending requuests: 92req/s
* request pending overflow: 630
* circuit-breaker: cx_open
* status code 200/503: 92.6k/630

![image](https://github.com/guitarrapc/envoy-lab/assets/3856350/c3d1c5e1-bc9c-4d68-976d-b9d7e1521c0e)

```
k6 - 1 |      ✗ is status 200
k6 - 1 |       ↳  99 % — ✓ 6061735 / ✗ 14313
k6 - 1 |
k6 - 1 | checks.........................: 99.76 %  ✓ 6061735       ✗ 14313
k6 - 1 | data_received..................: 3.7 GB  62 MB / s
k6 - 1 | data_sent......................: 553 MB  9.2 MB / s
k6 - 1 | http_req_blocked...............: avg = 2.57µs  min = 260ns     med = 852ns   max = 31.16ms p(90) = 1.34µs p(95) = 1.64µs
k6 - 1 | http_req_connecting............: avg = 156ns   min = 0s        med = 0s      max = 28.12ms p(90) = 0s     p(95) = 0s
k6 - 1 | http_req_duration..............: avg = 1.87ms  min = 70.92µs   med = 1.64ms  max = 90.55ms p(90) = 3.12ms p(95) = 3.79ms
k6 - 1 | { expected_response: true }...: avg = 1.87ms  min = 137.57µs  med = 1.64ms  max = 90.55ms p(90) = 3.12ms p(95) = 3.79ms
k6 - 1 | http_req_failed................: 0.23 %   ✓ 14313         ✗ 6061735
k6 - 1 | http_req_receiving.............: avg = 33.84µs min = -204954ns med = 13.02µs max = 22.06ms p(90) = 21.1µs p(95) = 24.91µs
k6 - 1 | http_req_sending...............: avg = 12.18µs min = 1.25µs    med = 4.45µs  max = 20.52ms p(90) = 6.41µs p(95) = 7.37µs
k6 - 1 | http_req_tls_handshaking.......: avg = 0s      min = 0s        med = 0s      max = 0s      p(90) = 0s     p(95) = 0s
k6 - 1 | http_req_waiting...............: avg = 1.82ms  min = 65.13µs   med = 1.62ms  max = 90.34ms p(90) = 3.07ms p(95) = 3.72ms
k6 - 1 | http_reqs......................: 6076048 101262.130233 / s
k6 - 1 | iteration_duration.............: avg = 1.96ms  min = 84.85µs   med = 1.69ms  max = 93.13ms p(90) = 3.23ms p(95) = 3.97ms
k6 - 1 | iterations.....................: 6076048 101262.130233 / s
k6 - 1 | vus............................: 200     min = 200         max = 200
k6 - 1 | vus_max........................: 200     min = 200         max = 200
k6 - 1 |
k6 - 1 |
k6 - 1 | running(1m00.0s), 000 / 200 VUs, 6076048 complete and 0 interrupted iterations
k6 - 1 | default ✓[100 % ] 200 VUs  1m0s
```

## Parrern 8 [vus: 200]

circuit-break

| max_connections | max_pending_requests | max_requests | overload_manager.resource_monitors.max_active_downstream_connections |
| --------------- | -------------------- | ------------ | ------------ |
| 16384 | 16384 | 16384 | 16384 | 16384 |

result

* connection count: 200
* pending connections: 0
* request Rate: 150-176req/s
* pending requuests: 0
* request pending overflow: 0
* circuit-breaker: -
* status code 200/503: 94.5k/0

```
k6-1            |      ✓ is status 200
k6-1            |
k6-1            |      checks.........................: 100.00% ✓ 6173751       ✗ 0
k6-1            |      data_received..................: 3.8 GB  63 MB/s
k6-1            |      data_sent......................: 562 MB  9.4 MB/s
k6-1            |      http_req_blocked...............: avg=2.59µs  min=260ns    med=862ns   max=29.29ms  p(90)=1.36µs  p(95)=1.67µs
k6-1            |      http_req_connecting............: avg=136ns   min=0s       med=0s      max=26.58ms  p(90)=0s      p(95)=0s
k6-1            |      http_req_duration..............: avg=1.84ms  min=140.16µs med=1.67ms  max=107.46ms p(90)=2.93ms  p(95)=3.46ms
k6-1            |        { expected_response:true }...: avg=1.84ms  min=140.16µs med=1.67ms  max=107.46ms p(90)=2.93ms  p(95)=3.46ms
k6-1            |      http_req_failed................: 0.00%   ✓ 0             ✗ 6173751
k6-1            |      http_req_receiving.............: avg=34.07µs min=4.26µs   med=13.05µs max=22.99ms  p(90)=21.31µs p(95)=25.51µs
k6-1            |      http_req_sending...............: avg=13.01µs min=1.29µs   med=4.55µs  max=27.56ms  p(90)=6.61µs  p(95)=7.63µs
k6-1            |      http_req_tls_handshaking.......: avg=0s      min=0s       med=0s      max=0s       p(90)=0s      p(95)=0s
k6-1            |      http_req_waiting...............: avg=1.79ms  min=125.93µs med=1.65ms  max=100.21ms p(90)=2.88ms  p(95)=3.39ms
k6-1            |      http_reqs......................: 6173751 102892.041328/s
k6-1            |      iteration_duration.............: avg=1.92ms  min=163.34µs med=1.72ms  max=109.95ms p(90)=3.02ms  p(95)=3.61ms
k6-1            |      iterations.....................: 6173751 102892.041328/s
k6-1            |      vus............................: 200     min=200         max=200
k6-1            |      vus_max........................: 200     min=200         max=200
k6-1            |
k6-1            |
k6-1            | running (1m00.0s), 000/200 VUs, 6173751 complete and 0 interrupted iterations
k6-1            | default ✓ [ 100% ] 200 VUs  1m0s
```


# Envoy Benchmark - overload_manager.resource_monitors.max_active_downstream_connections

## Parrern 9 [vus: 200]

circuit-break

| max_connections | max_pending_requests | max_requests | overload_manager.resource_monitors.max_active_downstream_connections |
| --------------- | -------------------- | ------------ | ------------ |
| 100 | 100 | 100 | 10000 | 10 |

result

* connection count: 100
* pending connections: 0
* request Rate: 62-93req/s
* pending requuests: 0req/s
* request pending overflow: 0
* circuit-breaker: cx_open
* status code 200/503: 69.1k/0

```
k6-1            | time="2024-07-08T15:42:17Z" level=warning msg="Request Failed" error="Get \"http://envoy:8080/weatherforecast\": read tcp 172.18.0.8:44142->172.18.0.4:8080: read: connection reset by peer"
k6-1            | time="2024-07-08T15:42:17Z" level=warning msg="Request Failed" error="Get \"http://envoy:8080/weatherforecast\": read tcp 172.18.0.8:43908->172.18.0.4:8080: read: connection reset by peer"
k6-1            | time="2024-07-08T15:42:17Z" level=warning msg="Request Failed" error="Get \"http://envoy:8080/weatherforecast\": read tcp 172.18.0.8:44128->172.18.0.4:8080: read: connection reset by peer"
k6-1            | time="2024-07-08T15:42:17Z" level=warning msg="Request Failed" error="Get \"http://envoy:8080/weatherforecast\": read tcp 172.18.0.8:41738->172.18.0.4:8080: read: connection reset by peer"
k6-1            | time="2024-07-08T15:42:17Z" level=warning msg="Request Failed" error="Get \"http://envoy:8080/weatherforecast\": read tcp 172.18.0.8:42158->172.18.0.4:8080: read: connection reset by peer"
k6-1            | time="2024-07-08T15:42:17Z" level=warning msg="Request Failed" error="Get \"http://envoy:8080/weatherforecast\": EOF"
k6-1            | time="2024-07-08T15:42:17Z" level=warning msg="Request Failed" error="Get \"http://envoy:8080/weatherforecast\": http: server closed idle connection"
k6-1
```

## Parrern 10 [vus: 200]

circuit-break

| max_connections | max_pending_requests | max_requests | overload_manager.resource_monitors.max_active_downstream_connections |
| --------------- | -------------------- | ------------ | ------------ |
| 100 | 100 | 100 | 10000 | 100 |

result

* connection count: 10
* pending connections: 0
* request Rate: 10req/s
* pending requuests: 0req/s
* request pending overflow: 0
* circuit-breaker: -
* status code 200/503: 21.5k/0

```
k6-1            |      ✗ is status 200
k6-1            |       ↳  99% — ✓ 4650889 / ✗ 32769
k6-1            |
k6-1            |      checks.........................: 99.30%  ✓ 4650889      ✗ 32769
k6-1            |      data_received..................: 2.9 GB  37 MB/s
k6-1            |      data_sent......................: 425 MB  5.5 MB/s
k6-1            |      http_req_blocked...............: avg=3.51µs  min=271ns    med=931ns   max=36.99ms  p(90)=1.47µs  p(95)=1.85µs
k6-1            |      http_req_connecting............: avg=1.63µs  min=0s       med=0s      max=36.97ms  p(90)=0s      p(95)=0s
k6-1            |      http_req_duration..............: avg=1.22ms  min=0s       med=1.11ms  max=112.92ms p(90)=1.88ms  p(95)=2.22ms
k6-1            |        { expected_response:true }...: avg=1.23ms  min=168.55µs med=1.11ms  max=112.92ms p(90)=1.88ms  p(95)=2.22ms
k6-1            |      http_req_failed................: 0.69%   ✓ 32769        ✗ 4650889
k6-1            |      http_req_receiving.............: avg=21.72µs min=0s       med=14.39µs max=28.48ms  p(90)=23.15µs p(95)=30.14µs
k6-1            |      http_req_sending...............: avg=7.69µs  min=0s       med=4.76µs  max=24.01ms  p(90)=6.94µs  p(95)=8.56µs
k6-1            |      http_req_tls_handshaking.......: avg=0s      min=0s       med=0s      max=0s       p(90)=0s      p(95)=0s
k6-1            |      http_req_waiting...............: avg=1.19ms  min=0s       med=1.08ms  max=111.94ms p(90)=1.85ms  p(95)=2.18ms
k6-1            |      http_reqs......................: 4683658 60684.809452/s
k6-1            |      iteration_duration.............: avg=2.91ms  min=124.39µs med=1.15ms  max=45.17s   p(90)=1.95ms  p(95)=2.32ms
k6-1            |      iterations.....................: 4683658 60684.809452/s
k6-1            |      vus............................: 100     min=100        max=200
k6-1            |      vus_max........................: 200     min=200        max=200
k6-1            |
k6-1            |
k6-1            | running (1m17.2s), 000/200 VUs, 4683658 complete and 0 interrupted iterations
k6-1            | default ✓ [ 100% ] 200 VUs  1m0s
```
