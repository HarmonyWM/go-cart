using MaliMove.Api.Interfaces;
using MaliMove.Api.Repositories;
using MaliMove.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("MaliMoveFrontend", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Register services
builder.Services.AddSingleton<IMockDataRepository, MockDataRepository>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IDealService, DealService>();
builder.Services.AddScoped<ITravelCostService, TravelCostService>();
builder.Services.AddScoped<IShoppingOptimisationService, ShoppingOptimisationService>();

var app = builder.Build();

app.UseCors("MaliMoveFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
