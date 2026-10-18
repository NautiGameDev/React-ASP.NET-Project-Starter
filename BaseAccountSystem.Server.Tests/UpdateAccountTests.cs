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
    public class UpdateAccountTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public UpdateAccountTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
            _output = output;
        }

        private async Task HandleLogin(string username, string password, string fromTest)
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
        }

        [Fact]
        public async Task UpdatePassword_ValidCredentials_Returns200Ok()
        {
            var expectedStatusCode = HttpStatusCode.OK;

            await HandleLogin(TestUserTwo.UserName, TestUserTwo.Password, "UpdatePassword_ValidCredentials");

            UpdateAccountDto updatePayload = new UpdateAccountDto()
            {                
                CurrentPassword = TestUserTwo.Password,
                NewPassword = TestUserTwo.NewPassword
            };

            var response = await _client.PutAsJsonAsync("/api/account/update", updatePayload);
            string responseBody = await response.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == response.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Update Account",
                "Update password with valid credentials",
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
        public async Task UpdateEmail_ValidCredentials_Returns200Ok()
        {
            var expectedStatusCode = HttpStatusCode.OK;

            await HandleLogin(TestUserThree.UserName, TestUserThree.Password, "UpdateEmail_ValidCredentials");

            UpdateAccountDto updatePayload = new UpdateAccountDto()
            {
                CurrentPassword = TestUserThree.Password,
                Email = TestUserThree.NewEmail
            };

            var response = await _client.PutAsJsonAsync("/api/account/update", updatePayload);
            string responseBody = await response.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == response.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Update Account",
                "Update email with valid credentials",
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
        public async Task UpdateAccount_InvalidCredentials_Returns400BadRequest()
        {
            var expectedStatusCode = HttpStatusCode.BadRequest;

            await HandleLogin(TestUser.UserName, TestUser.Password, "UpdateAccount_InvalidCredentials");

            UpdateAccountDto updatePayload = new UpdateAccountDto()
            {
                CurrentPassword = "Wrongpassword123!",
                Email = "newemail@example.com",
                NewPassword = "NewPassword123!"
            };

            var response = await _client.PutAsJsonAsync("/api/account/update", updatePayload);
            string responseBody = await response.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == response.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Update Account",
                "Update account with invalid credentials",
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
