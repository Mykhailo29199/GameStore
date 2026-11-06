using BusinessLogicLayer.Catalog;
using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Pipelines;
using BusinessLogicLayer.Pipelines.Interfaces;
using BusinessLogicLayer.Pipelines.Steps;
using BusinessLogicLayer.Security;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Logging;
using DataAccessLayer.Mongo;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.Repositories.MongoDBRepositories;
using DataAccessLayer.Repositories.SQLRepositories;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.FileSystem;
using GameStore.Shared.Logging;
using GameStore.Shared.Options;
using Gamestore.WebAPI.Mapper;
using Gamestore.WebAPI.Middlewares;
using Gamestore.WebAPI.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MongoDB.Driver;
using IOrderDetailsSqlRepository = DataAccessLayer.Repositories.Interfaces.SQLInterfaces.IOrderDetailsSqlRepository;
using ISupplierSqlRepository = DataAccessLayer.Repositories.Interfaces.SQLInterfaces.ISupplierSqlRepository;
using OrderDetailsSqlRepository = DataAccessLayer.Repositories.SQLRepositories.OrderDetailsSqlRepository;
using OrderSqlRepository = DataAccessLayer.Repositories.SQLRepositories.OrderSqlRepository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<GameAutoMapping>());
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<PaymentServiceOptions>(
    builder.Configuration.GetSection("PaymentService"));

// Registering HttpClient with a base URL
builder.Services.AddHttpClient("PaymentService", (sp, client) =>
{
    var options = sp.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<PaymentServiceOptions>>().Value;

    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddScoped<ISqlServerUnitOfWork, SqlServerUnitOfWork>();

builder.Services.AddScoped<IMongoDBUnitOfWork, MongoDBUnitOfWork>();

builder.Services.AddScoped<IFileSystem, FileSystem>();
builder.Services.AddScoped<IFileLogWriter, FileLogWriter>();

builder.Services.AddScoped<IProductSqlRepository, ProductSqlRepository>();
builder.Services.AddScoped<IPlatformSqlRepository, PlatformSqlRepository>();
builder.Services.AddScoped<ICategorySqlRepository, CategorySqlRepository>();
builder.Services.AddScoped<IProductCategorySqlRepository, ProductCategorySqlRepository>();
builder.Services.AddScoped<IProductPlatformSqlRepository, ProductPlatformSqlRepository>();
builder.Services.AddScoped<IProductFileSqlRepository, ProductFileSqlRepository>();
builder.Services.AddScoped<ISupplierSqlRepository, SupplierSqlRepository>();
builder.Services.AddScoped<IOrderSqlRepository, OrderSqlRepository>();
builder.Services.AddScoped<IOrderDetailsSqlRepository, OrderDetailsSqlRepository>();
builder.Services.AddScoped<ICommentSqlRepository, CommentSqlRepository>();
builder.Services.AddScoped<IBanSqlRepository, BanSqlRepository>();

builder.Services.AddScoped<IEntityChangeLogRepository, EntityChangeLogRepository>();
builder.Services.AddScoped<IEntityChangeLogger, EntityChangeLogger>();
builder.Services.AddScoped<EntityChangeLoggingInterceptor>();

builder.Services.AddScoped<ICategoryCatalog, CategoryCatalog>();
builder.Services.AddScoped<ISupplierCatalog, SupplierCatalog>();
builder.Services.AddScoped<IProductCatalog, ProductCatalog>();
builder.Services.AddScoped<IStoreCatalog, StoreCatalog>();

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IPlatformService, PlatformService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IBanService, BanService>();
builder.Services.AddScoped<IShipperService, ShipperService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAccessService, AccessService>();
builder.Services.AddScoped<IExternalAuthClient, ExternalAuthClient>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddGameStoreAuthorization(builder.Configuration);

// External authorization microservice
builder.Services.AddHttpClient(ExternalAuthClient.HttpClientName, client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AuthService:BaseUrl"] ?? "https://localhost:5037/");
});

builder.Services.AddScoped<IGamePipelineStep, GenreFilterStep>();
builder.Services.AddScoped<IGamePipelineStep, PlatformFilterStep>();
builder.Services.AddScoped<IGamePipelineStep, PublisherFilterStep>();
builder.Services.AddScoped<IGamePipelineStep, PriceFilterStep>();
builder.Services.AddScoped<IGamePipelineStep, NameFilterStep>();
builder.Services.AddScoped<IGamePipelineStep, DateFilterStep>();
builder.Services.AddScoped<SortingStep>();
builder.Services.AddScoped<PaginationStep>();

builder.Services.AddScoped<GameFilterPipeline>();

builder.Logging.ClearProviders();

builder.Logging.AddProvider(new FileLoggerProvider());

// Northwind MongoDB connection; the database name can be overridden for temporary databases.
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDbConnection");
var mongoDatabaseName = builder.Configuration.GetConnectionString("MongoDatabaseName");

// Northwind data is damaged, so tolerant serialization must be configured before the first query.
NorthwindMongoConfiguration.Configure();

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(mongoConnectionString));

builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName ?? "Northwind");
});

// SQL Server context. Every saved change is logged into the Northwind database.
builder.Services.AddDbContext<GameStoreContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .AddInterceptors(sp.GetRequiredService<EntityChangeLoggingInterceptor>());

    // SQL text, parameter values and detailed errors are shown only during development.
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }
});

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

// Northwind products get their game keys once at startup, so GET requests never write into Northwind.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<IProductCatalog>().AssignMissingGameKeysAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowFrontend"); // This allows Angular to interact with the API without errors

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Path to Angular build (outside solution project).
// The build is not stored in the repository, so the API must also start when the folder is missing.
var angularPath = Path.Combine(
    Directory.GetCurrentDirectory(),
    "..",
    "gamestore-ui-app",
    "dist",
    "gamestore-ui-app",
    "browser");

var angularFiles = Directory.Exists(angularPath)
    ? new PhysicalFileProvider(angularPath)
    : null;

if (angularFiles is not null)
{
    // Serve default files (index.html)
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = angularFiles,
    });

    // Serve static files (js, css, assets)
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = angularFiles,
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (angularFiles is not null)
{
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = angularFiles,
    });
}

app.Run();
