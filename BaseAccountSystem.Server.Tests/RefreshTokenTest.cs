using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using BaseAccountSystem.Server.DTOs.Account;
using BaseAccountSystem.Server.Tests.Helpers;
using BaseAccountSystem.Server.Tests.TestData;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace BaseAccountSystem.Server.Tests
{
    public class RefreshTokenTest : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;
        private readonly CustomWebApplicationFactory<Program> _factory;


        public RefreshTokenTest(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
            _output = output;
            _factory = factory;
        }

        private async Task<HttpResponseMessage> HandleLogin(string username, string password, string fromTest)
        {
            var expectedStatusCode = HttpStatusCode.OK;

            LoginDto loginPayload = new LoginDto()
            {
                UserName = username,
                Password = password
            };

            var response = await _client.PostAsJsonAsync("/api/account/login", loginPayload);

            if (!response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                string testResult = expectedStatusCode == response.StatusCode ? "Passed" : "Failed";

                string output = EntryLogHandler.GetEntryString(
                "Login to update account",
                $"Attempted login from test {fromTest}",
                $"{(int)expectedStatusCode} {expectedStatusCode}",
                $"{(int)response.StatusCode} {response.StatusCode}",
                $"{responseBody}",
                $"{testResult}"
                );
            }

            response.EnsureSuccessStatusCode();
            return response;
        }

        [Fact]
        public async Task RefreshToken_WithRefreshToken_Returns200Ok()
        {
            var expectedStatusCode = HttpStatusCode.OK;

            await HandleLogin(TestUser.UserName, TestUser.Password, "RefreshToken_WithRefreshToken");                       

            var response = await _client.PostAsync("/api/account/refresh", null);
            string responseBody = await response.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == response.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Refresh Token",
                "Get new auth token with valid refresh token",
                $"{(int)expectedStatusCode} {expectedStatusCode}",
                $"{(int)response.StatusCode} {response.StatusCode}",
                $"{responseBody}",
                $"{testResult}"
                );

            WriteTestResults.AppendData(output);

            _output.WriteLine(output);

            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WithOldToken_Returns401Unauthorized()
        {
            var expectedStatusCode = HttpStatusCode.Unauthorized;

            var loginResponse = await HandleLogin(TestUser.UserName, TestUser.Password, "RefreshToken_WithOldToken");

            //Store first refrsh token
            var setCookieHeaders = loginResponse.Headers.GetValues("Set-Cookie");
            string originalRefreshCookie = setCookieHeaders.FirstOrDefault(c => c.StartsWith("BASRefreshAuth="));

            //Get new refresh token
            var firstResponse = await _client.PostAsync("/api/account/refresh", null);
            firstResponse.EnsureSuccessStatusCode();

            //New client
            var cleanClient = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

            //Build new payload with original cookie
            var reuseRequest = new HttpRequestMessage(HttpMethod.Post, "/api/account/refresh");
            reuseRequest.Headers.Add("Cookie", originalRefreshCookie);

            //Attempt request with original cookie
            var reuseResponse = await cleanClient.SendAsync(reuseRequest);
            string responseBody = await reuseResponse.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == reuseResponse.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Refresh Token",
                "Attempt get new token with old refresh token",
                $"{(int)expectedStatusCode} {expectedStatusCode}",
                $"{(int)reuseResponse.StatusCode} {reuseResponse.StatusCode}",
                $"{responseBody}",
                $"{testResult}"
                );

            WriteTestResults.AppendData(output);

            _output.WriteLine(output);

            Assert.Equal(expectedStatusCode, reuseResponse.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_MissingCookie_Returns401Unauthorized()
        {
            var expectedStatusCode = HttpStatusCode.Unauthorized;

            var unauthenticatedClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

            var response = await unauthenticatedClient.PostAsync("/api/account/refresh", null);
            string responseBody = await response.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == response.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Refresh Token",
                "Refresh without refresh cookie",
                $"{(int)expectedStatusCode} {expectedStatusCode}",
                $"{(int)response.StatusCode} {response.StatusCode}",
                $"{responseBody}",
                $"{testResult}"
            );

            WriteTestResults.AppendData(output);
            _output.WriteLine(output);

            Assert.Equal(expectedStatusCode, response.StatusCode);
        }
    }
}
