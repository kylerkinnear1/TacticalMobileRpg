using System.Collections.Concurrent;
using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Server;
using Rpg.Mobile.Server.Battles;
using Rpg.Mobile.Server.Battles.Calculators;
using Rpg.Mobile.Server.Lobby;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddTransient<GameHub>();
builder.Services.AddSingleton<ConcurrentDictionary<string, GameContext>>();
builder.Services.AddSingleton<MapLoader>();
builder.Services.AddSingleton<IMapLoader>(provider => provider.GetRequiredService<MapLoader>());
builder.Services.AddSingleton<IPathCalculator, PathCalculator>();
builder.Services.AddSingleton<ISelectingAttackTargetCalculator, SelectingAttackTargetCalculator>();
builder.Services.AddSingleton<ISelectingMagicTargetCalculator, SelectingMagicTargetCalculator>();
builder.Services.AddSingleton<IMagicDamageCalculator, MagicDamageCalculator>();
builder.Services.AddSingleton<IAttackDamageCalculator, AttackDamageCalculator>();
builder.Services.AddSingleton<IBattleProvider, BattleProvider>();
builder.Services.AddSingleton<ILobbyProvider, LobbyProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<GameHub>("/game-hub");
app.Run();