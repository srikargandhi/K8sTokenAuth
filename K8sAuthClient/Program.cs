// Copyright Koninklijke Philips N.V. 2024

string tokenFilePath = Environment.GetEnvironmentVariable("TOKEN_FILE_PATH")
    ?? "/var/run/secrets/kubernetes.io/serviceaccount/token";
string serverUrl = Environment.GetEnvironmentVariable("SERVER_URL") ?? "http://localhost:5055/secure";
string headerName = Environment.GetEnvironmentVariable("HEADER_NAME") ?? "X-Client-ID";

Console.WriteLine($"[Client] Using token file: {tokenFilePath}");
Console.WriteLine($"[Client] Server URL: {serverUrl}");
Console.WriteLine($"[Client] Header Name: {headerName}");

string token;
try {
    Console.WriteLine("Client App started");
    token = await File.ReadAllTextAsync(tokenFilePath);
    Console.WriteLine($"token : {token}");
} catch (Exception ex) {
    Console.WriteLine($"[Client] Failed to read token file: {ex.Message}");
    return;
}

if (string.IsNullOrWhiteSpace(token)) {
    Console.WriteLine("[Client] Token file empty");
    return;
}

using var httpClient = new HttpClient();
using var request = new HttpRequestMessage(HttpMethod.Get, serverUrl);
request.Headers.TryAddWithoutValidation(headerName, token.Trim());
Console.WriteLine("[Client] Calling /secure...");

try {
    var response = await httpClient.SendAsync(request);
    string body = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"[Client] HTTP {(int)response.StatusCode}");
    Console.WriteLine(body);
} catch (Exception ex) {
    Console.WriteLine($"[Client] Request failed: {ex.Message}");
}
