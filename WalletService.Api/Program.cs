using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WalletService.Application.Contracts;
using WalletService.Application.Models;
using WalletService.Infrastructure.Data;
using WalletService.Infrastructure.Services;


var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Wallet Service API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter your JWT token. Example: 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// DbContext
builder.Services.AddDbContext<WalletDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("WalletDb"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "wallet"));
});

// JWT config – must match Identity service Issuer/Audience/SecretKey
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey
        };
    });

builder.Services.Configure<BlockchainGatewayOptions>(
    builder.Configuration.GetSection("BlockchainGateway"));

// HttpClient for BlockchainGateway
builder.Services.AddHttpClient<IBlockchainGatewayClient, BlockchainGatewayClient>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<BlockchainGatewayOptions>>().Value;

    if (string.IsNullOrWhiteSpace(opts.BaseUrl))
        throw new InvalidOperationException("BlockchainGateway:BaseUrl is not configured.");

    client.BaseAddress = new Uri(opts.BaseUrl);
});

builder.Services.AddAuthorization();

// Application services
builder.Services.AddScoped<IWalletService, WalletService.Infrastructure.Services.WalletService>();

var app = builder.Build();

// Auto-migrate on startup (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
