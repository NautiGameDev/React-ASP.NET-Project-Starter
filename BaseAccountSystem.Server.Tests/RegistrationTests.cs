using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using BaseAccountSystem.Server.Data;
using BaseAccountSystem.Server.DTOs.Account;
using BaseAccountSystem.Server.DTOs.Email;
using BaseAccountSystem.Server.Models;
using BaseAccountSystem.Server.Tests.Helpers;
using BaseAccountSystem.Server.Tests.TestData;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BaseAccountSystem.Server.Tests
{
    public class RegistrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public RegistrationTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _client = factory.CreateClient();
            _output = output;
            _factory = factory;
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

        [Fact]
        public async Task ConfirmAccount_ValidCode_Returns200Ok()
        {
            var expectedStatusCode = HttpStatusCode.OK;

            NewAccountDto newUser = new NewAccountDto()
            {
                UserName = "JaneDoe",
                Password = "JaneDoe123!",
                Email = "janedoe@example.com"
            };

            var response = await _client.PostAsJsonAsync("/api/account/register", newUser);
            response.EnsureSuccessStatusCode();

            string userId;
            string confirmationCode;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = await db.Users.FirstAsync(u => u.Email == newUser.Email);
                userId = user.Id;

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();
                string rawToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                confirmationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
            }

            ConfirmEmailDto confirmEmailDto = new ConfirmEmailDto()
            {
                UserId = userId,
                ConfirmationCode = confirmationCode,
            };

            var verificationResponse = await _client.PostAsJsonAsync("/api/account/confirm-email", confirmEmailDto);
            string responseBody = await verificationResponse.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == verificationResponse.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Confirm Email",
                "Confirm account with confirmation code",
                $"{(int)expectedStatusCode} {expectedStatusCode}",
                $"{(int)verificationResponse.StatusCode} {verificationResponse.StatusCode}",
                $"{responseBody}",
                $"{testResult}"
                );

            WriteTestResults.AppendData(output);

            _output.WriteLine(output);

            Assert.Equal(expectedStatusCode, verificationResponse.StatusCode);

        }

        [Fact]
        public async Task ConfirmAccount_InvalidCode_Returns400BadRequest()
        {
            var expectedStatusCode = HttpStatusCode.BadRequest;

            NewAccountDto newUser = new NewAccountDto()
            {
                UserName = "fakeuser",
                Password = "Fakeuser123!",
                Email = "fakeuser@example.com"
            };

            var response = await _client.PostAsJsonAsync("/api/account/register", newUser);
            response.EnsureSuccessStatusCode();

            string userId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = await db.Users.FirstAsync(u => u.Email == newUser.Email);
                userId = user.Id;
            }

            string invalidConfirmationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("this-is-an-invalid-token"));

            ConfirmEmailDto confirmEmailDto = new ConfirmEmailDto()
            {
                UserId = userId,
                ConfirmationCode = invalidConfirmationCode,
            };

            var verificationResponse = await _client.PostAsJsonAsync("/api/account/confirm-email", confirmEmailDto);
            string responseBody = await verificationResponse.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == verificationResponse.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Confirm Email",
                "Attempt confirm account with incorrect code",
                $"{(int)expectedStatusCode} {expectedStatusCode}",
                $"{(int)verificationResponse.StatusCode} {verificationResponse.StatusCode}",
                $"{responseBody}",
                $"{testResult}"
                );

            WriteTestResults.AppendData(output);

            _output.WriteLine(output);

            Assert.Equal(expectedStatusCode, verificationResponse.StatusCode);
        }
    }
}
