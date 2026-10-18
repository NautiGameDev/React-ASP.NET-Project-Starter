using System;
using System.Collections.Generic;
using System.Text;
using BaseAccountSystem.Server.Data;
using BaseAccountSystem.Server.Models;
using BaseAccountSystem.Server.Tests.Helpers;
using BaseAccountSystem.Server.Tests.TestData;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BaseAccountSystem.Server.Tests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private readonly string _dbName = "TestDatabase_" + Guid.NewGuid().ToString();


        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(ApplicationDbContext) ||
                    d.ServiceType.Name.Contains("DbContextOptionsConfiguration")
                ).ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName)
                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();

                //Setup file writing
                WriteTestResults.SetDbName(_dbName);

                //Seed data
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();

                var defaultUser = new Account
                {
                    UserName = TestUser.UserName,
                    Email = TestUser.Email
                };

                var secondUser = new Account
                {
                    UserName = TestUserTwo.UserName,
                    Email = TestUserTwo.Email
                };

                var thirdUser = new Account
                {
                    UserName = TestUserThree.UserName,
                    Email = TestUserThree.Email
                };

                userManager.CreateAsync(defaultUser, TestUser.Password).GetAwaiter().GetResult();
                userManager.CreateAsync(secondUser, TestUserTwo.Password).GetAwaiter().GetResult();
                userManager.CreateAsync(thirdUser, TestUserThree.Password).GetAwaiter().GetResult();
            });
        }
    }
}
