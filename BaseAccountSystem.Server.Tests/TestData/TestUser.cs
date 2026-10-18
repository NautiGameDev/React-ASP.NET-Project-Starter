using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Identity.Client;

namespace BaseAccountSystem.Server.Tests.TestData
{
    public static class TestUser
    {
        public static string UserName { get; set; } = "testuser";
        public static string Email { get; set; } = "testuser@example.com";
        public static string Password { get; set; } = "Testuser123!";
        public static string NewPassword { get; set; } = "Admin123!";
    }
}
