using k8s;
using k8s.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Health endpoint for readiness
app.MapGet("/healthz", () => Results.Ok("ok"));

app.MapGet("/secure", async (HttpRequest request) => {
    var headerName = Environment.GetEnvironmentVariable("HEADER_NAME") ?? "X-Client-ID";
    if (!request.Headers.TryGetValue(headerName, out var tokenValues)) {
        return Results.Json(new { authenticated = false, error = "missing header" }, statusCode: 401);
    }

    var token = tokenValues.FirstOrDefault();
    if (string.IsNullOrWhiteSpace(token)) {
        return Results.Json(new { authenticated = false, error = "empty token" }, statusCode: 401);
    }

    try {
        using var kubeClient = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
        var review = new V1TokenReview {
            ApiVersion = "authentication.k8s.io/v1",
            Kind = "TokenReview",
            Spec = new V1TokenReviewSpec { Token = token }
        };

        var tokenReviewResult = await kubeClient.AuthenticationV1.CreateTokenReviewWithHttpMessagesAsync(review);
        bool status = tokenReviewResult?.Body?.Status?.Authenticated.HasValue == true &&
            tokenReviewResult?.Body?.Status?.Authenticated == true;

        return !status
            ? Results.Json(new { authenticated = false }, statusCode: 401)
            : Results.Json(new { authenticated = true }, statusCode: 200);
    } catch (Exception ex) {
        // Do not leak token; minimal error response
        return Results.Json(new { authenticated = false, error = ex.GetType().Name }, statusCode: 500);
    }
});

app.Run();
