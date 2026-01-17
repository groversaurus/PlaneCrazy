using Microsoft.Extensions.DependencyInjection;
using PlaneCrazy.Console;
using PlaneCrazy.Core.Services;
using Spectre.Console;

// Setup dependency injection
var services = new ServiceCollection();
services.AddSingleton<IAircraftDataService, InMemoryAircraftDataService>();
services.AddSingleton<Application>();

var serviceProvider = services.BuildServiceProvider();

// Run the application
var app = serviceProvider.GetRequiredService<Application>();
await app.RunAsync();
