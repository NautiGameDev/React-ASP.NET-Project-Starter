using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using BaseAccountSystem.Server.DTOs.Account;
using BaseAccountSystem.Server.Tests.Helpers;
using BaseAccountSystem.Server.Tests.TestData;
using Xunit.Abstractions;

namespace BaseAccountSystem.Server.Tests
{
    public class RegistrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public RegistrationTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _client = factory.CreateClient();
            _output = output;
        }

        [Fact]
        public async Task Register_ValidCredentials_Returns201Created()
        {
            var expectedStatusCode = HttpStatusCode.Created;

            NewAccountDto newUser = new NewAccountDto()
            {
                UserName = "JohnDoe",
                Password = "JohnDoe123!",
                Email = "Johndoe@example.com"
            };

            var response = await _client.PostAsJsonAsync("/api/account/register", newUser);
            string responseBody = await response.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == response.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Registration",
                "Register with valid credentials",
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
        public async Task Register_WeakCredentials_Returns409Conflict()
        {
            var expectedStatusCode = HttpStatusCode.Conflict;

            NewAccountDto newUser = new NewAccountDto()
            {
                UserName = "JaneDoe",
                Password = "123!",
                Email = "Janedoe@example.com"
            };

            var response = await _client.PostAsJsonAsync("/api/account/register", newUser);
            string responseBody = await response.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == response.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Registration",
                "Register with weak credentials",
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
        public async Task Register_UniqueEmail_Returns409Conflict()
        {
            var expectedStatusCode = HttpStatusCode.Conflict;

            NewAccountDto newUser = new NewAccountDto()
            {
                UserName = $"{TestUser.UserName}123",
                Password = TestUser.Password,
                Email = TestUser.Email
            };

            var response = await _client.PostAsJsonAsync("/api/account/register", newUser);
            string responseBody = await response.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == response.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Registration",
                "Attempt register with existing email",
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
        public async Task Register_UniqueUsername_Returns409Conflict()
        {
            var expectedStatusCode = HttpStatusCode.Conflict;

            NewAccountDto newUser = new NewAccountDto()
            {
                UserName = TestUser.UserName,
                Password = TestUser.Password,
                Email = "testuser123@example.com"
            };

            var response = await _client.PostAsJsonAsync("/api/account/register", newUser);
            string responseBody = await response.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == response.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Registration",
                "Attempt register with existing username",
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
