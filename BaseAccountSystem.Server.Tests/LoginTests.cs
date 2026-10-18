using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using BaseAccountSystem.Server.DTOs.Account;
using BaseAccountSystem.Server.Models;
using BaseAccountSystem.Server.Tests.Helpers;
using BaseAccountSystem.Server.Tests.TestData;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi;
using Xunit;
using Xunit.Abstractions;

namespace BaseAccountSystem.Server.Tests
{
    public class LoginTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public LoginTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _client = factory.CreateClient();
            _output = output;
        }

        [Fact]
        public async Task Login_ValidCredentials_Returns200Ok()
        {
            var expectedStatusCode = HttpStatusCode.OK;                          

            LoginDto loginDto = new LoginDto()
            {
                UserName = TestUser.UserName,
                Password = TestUser.Password
            };

            var response = await _client.PostAsJsonAsync("/api/account/login", loginDto);
            string responseBody = await response.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == response.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Login",
                "Login with accurate credentials",
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
        public async Task Login_WrongCredentials_Returns401Unauthorized()
        {
            var expectedStatusCode = HttpStatusCode.Unauthorized;

            LoginDto loginDto = new LoginDto()
            {
                UserName = TestUser.UserName,
                Password = "Admin12345!"
            };

            var response = await _client.PostAsJsonAsync("/api/account/login", loginDto);
            string responseBody = await response.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == response.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Login",
                "Login with wrong credentials",
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
