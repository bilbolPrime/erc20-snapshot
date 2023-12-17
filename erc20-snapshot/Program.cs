using BilbolStack.Erc20Snapshot.Chain;
using BilbolStack.Erc20Snapshot.Repository;
using BilbolStack.ERC20Snapshot.Media;
using erc20_snapshot;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IERC20ContractScraper, ERC20ContractScraper>();
builder.Services.AddSingleton<IERC20Repository, ERC20FileRepository>();
builder.Services.AddSingleton<IMediaManager, MediaConsoleManager>();
builder.Services.AddOptions<ChainSettings>().BindConfiguration(ChainSettings.ConfigKey);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
