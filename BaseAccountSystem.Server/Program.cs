using BaseAccountSystem.Server.Data;
using BaseAccountSystem.Server.Interfaces.Repos;
using BaseAccountSystem.Server.Interfaces.Services;
using BaseAccountSystem.Server.Models;
using BaseAccountSystem.Server.Repos;
using BaseAccountSystem.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<Account, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])
        )
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            string cookieName = builder.Configuration["JWT:CookieName"] ?? "BASAuth";
            var token = context.Request.Cookies[cookieName];
            context.Token = token;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
        .WithOrigins("https://localhost:58220", "https://10.160.0.246:58220")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

//Dependency Injection

//Settings
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

//Accounts
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountRepo, AccountRepo>();

//Activity Logs
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IActivityLogRepo, ActivityLogRepo>();

//Email
builder.Services.AddTransient<IEmailService, EmailService>();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//Seed first account
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string username = builder.Configuration["AdminConfig:Username"] ?? "AdminUser";
    string email = builder.Configuration["AdminConfig:Email"] ?? "admin@example.com";
    string password = builder.Configuration["AdminConfig:Password"] ?? "Admin123!";

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    if (await userManager.FindByNameAsync(username) == null)
    {
        var adminUser = new Account
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

app.MapFallbackToFile("/index.html");

app.Run();
