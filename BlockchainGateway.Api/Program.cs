using BlockchainGateway.Application.Contracts;
using BlockchainGateway.Application.Models;
using BlockchainGateway.Infrastructure.Services;
using BlockchainGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<SolanaHotWalletOptions>(
    builder.Configuration.GetSection("SolanaHotWallet"));

builder.Services.AddDbContext<BlockchainDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("BlockchainDb"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "blockchain");
        });
});

builder.Services.AddScoped<ISolanaTransactionService, SolanaTransactionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
