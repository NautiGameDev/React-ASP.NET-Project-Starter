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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace BaseAccountSystem.Server.Tests
{
    public class ResetPasswordTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ResetPasswordTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _client = factory.CreateClient();
            _output = output;
            _factory = factory;
        }

        [Fact]
        public async Task ResetPassword_ValidCode_Returns200Ok()
        {
            var expectedStatusCode = HttpStatusCode.OK;

            NewAccountDto newUser = new NewAccountDto()
            {
                UserName = "JimDoe",
                Password = "JimDoe123!",
                Email = "jimdoe@example.com"
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
                string rawToken = await userManager.GeneratePasswordResetTokenAsync(user);
                confirmationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
            }            

            PasswordResetDto passwordResetDto = new PasswordResetDto()
            {
                UserId = userId,
                Code = confirmationCode,
                NewPassword = "Admin123!"
            };

            var verificationResponse = await _client.PostAsJsonAsync("/api/account/confirm-reset-password", passwordResetDto);
            string responseBody = await verificationResponse.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == verificationResponse.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Reset Password",
                "Reset account password with correct code",
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
        public async Task ResetPassword_InvalidCode_Returns400BadRequest()
        {
            var expectedStatusCode = HttpStatusCode.BadRequest;

            NewAccountDto newUser = new NewAccountDto()
            {
                UserName = "JanisDoe",
                Password = "JanisDoe123!",
                Email = "janisdoe@example.com"
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

            string confirmationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("this-is-an-invalid-token"));

            PasswordResetDto passwordResetDto = new PasswordResetDto()
            {
                UserId = userId,
                Code = confirmationCode,
                NewPassword = "Admin123!"
            };

            var verificationResponse = await _client.PostAsJsonAsync("/api/account/confirm-reset-password", passwordResetDto);
            string responseBody = await verificationResponse.Content.ReadAsStringAsync();

            string testResult = expectedStatusCode == verificationResponse.StatusCode ? "Passed" : "Failed";

            string output = EntryLogHandler.GetEntryString(
                "Reset Password",
                "Reset password with invalid code",
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
