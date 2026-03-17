using BusinessLogicLayer.Services;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Repositories;
using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.FileSystem;
using GameStore.Shared.Logging;
using GameStore.Shared.Options;
using Gamestore.WebAPI.Mapper;
using Gamestore.WebAPI.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<GameAutoMapping>());
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<PaymentServiceOptions>(
    builder.Configuration.GetSection("PaymentService"));

// 2. Реєстрація HttpClient з базовою адресою
builder.Services.AddHttpClient("PaymentService", (sp, client) =>
{
    var options = sp.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<PaymentServiceOptions>>().Value;

    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddScoped<IFileSystem, FileSystem>();
builder.Services.AddScoped<IFileLogWriter, FileLogWriter>();

builder.Services.AddScoped<IUnitOfWork<GameStoreContext>, UnitOfWork>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IGameGenreRepository, GameGenreRepository>();
builder.Services.AddScoped<IGamePlatformRepository, GamePlatformRepository>();
builder.Services.AddScoped<IGameFileRepository, GameFileRepository>();
builder.Services.AddScoped<IPublisherRepository, PublisherRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderGameRepository, OrderGameRepository>();

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IPlatformService, PlatformService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Logging.ClearProviders();

builder.Logging.AddProvider(new FileLoggerProvider());

builder.Services.AddDbContext<GameStoreContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
     {
         policy
             .AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader()
             .WithExposedHeaders("x-total-numbers-of-games");
     });
});

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowFrontend"); // Це дозволить Angular взаємодіяти з API без помилок

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseMiddleware<RequestResponseLoggingMiddleware>();

//app.UseMiddleware<TotalGamesCountMiddleware>();

// Path to Angular build (outside solution project)
var angularPath = Path.Combine(
    Directory.GetCurrentDirectory(),
    "..",
    "gamestore-ui-app");

// Serve default files (index.html)
app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new PhysicalFileProvider(angularPath),
});

// Serve static files (js, css, assets)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(angularPath),
});

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html", new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(angularPath),
});

app.Run();
