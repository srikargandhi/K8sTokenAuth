# Kubernetes ServiceAccount Token Authentication PoC

This Proof of Concept demonstrates **pod-to-pod authentication** inside a Kubernetes (EKS) cluster using a ServiceAccount token and the Kubernetes **TokenReview** API — intentionally minimal (no caching, no retries, no proprietary code).

## Components
- `K8sAuthServer` (Pod B): Minimal ASP.NET Core API offering a protected endpoint `/secure`.
- `K8sAuthClient` (Pod A): Console app that reads its mounted ServiceAccount token and calls the server with header `X-Client-ID`.

## Flow
1. Client reads token from `/var/run/secrets/kubernetes.io/serviceaccount/token`.
2. Sends HTTP GET to `http://k8s-auth-server/secure` (service DNS) with `X-Client-ID: <token>`.
3. Server extracts token and performs a **TokenReview** call using in-cluster credentials.
4. If authenticated returns 200 + user info; else 401.

## Header
`X-Client-ID` is used (custom header). You can switch to `Authorization: Bearer <token>` by adjusting both apps.

## Build
```powershell
# From repository root
cd PoC/K8sTokenAuth/K8sAuthServer
dotnet build
cd ../K8sAuthClient
dotnet build
```

## Local Run (non-cluster simulation)
```powershell
$env:SERVER_URL = "http://localhost:5055/secure"
$dummy = "dummy-token-value"
# Start server
cd PoC/K8sTokenAuth/K8sAuthServer
dotnet run --urls http://localhost:5055
# New terminal for client
cd ..\K8sAuthClient
echo $dummy > token.txt
$env:TOKEN_FILE_PATH = "$(Get-Location)\token.txt"
dotnet run
```

## Kubernetes Deployment
```powershell
kubectl apply -f k8s/poc-namespace.yaml
kubectl apply -f k8s/server-rbac.yaml
kubectl apply -f k8s/server.yaml
kubectl apply -f k8s/client.yaml
kubectl get pods -n poc-auth
kubectl exec -n poc-auth deploy/k8s-auth-client -- dotnet K8sAuthClient.dll
```

## Expected Output
```
[Client] Calling /secure...
[Client] HTTP 200
{"authenticated":true,"username":"system:serviceaccount:poc-auth:client-sa"}
```
Tampered header / missing token => HTTP 401 `{"authenticated":false}`.

## Security Notes
- Raw token never logged.
- RBAC grants only `create` on `tokenreviews` to server ServiceAccount.

## Extending Later
- Add response caching.
- Introduce retry/backoff.
- Map Kubernetes groups to app roles.

## License
Add appropriate license before publishing publicly.
