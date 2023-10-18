using CSBackend.Services;
using MongoDB.Driver;
using CSBackend.Configs;
using StackExchange.Redis;
using CSBackend.Middlewares;

// Load ENV
DotNetEnv.Env.Load();
// Create builder
var builder = WebApplication.CreateBuilder(args);

// Serialize JSON response
// Set PropertyNamingPolicy to null for remaining properties naming policy
// Can be set to CamelCase instead of null
builder.Services.AddControllers()
		.AddJsonOptions(
				options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Dependency injection
// Add mongodb client
builder.Services.AddSingleton<IMongoClient>(s => new MongoClient(Config.DB.URL));
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost"));
// Add user services
builder.Services.AddSingleton<UserService>();
builder.Services.AddDataProtection();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.MapGet("/", () => "Hello, world!");

app.UsePathBase(new PathString("/api"));

app.UseWhen(context => context.Request.Path == "/auth", appBuilder =>
{
	appBuilder.UseMiddleware<VerifyToken>();
});

app.MapControllers();

app.Run();