using Serilog;
using ZaloOAWebhook.Context;
using ZaloOAWebhook.IRepository;
using ZaloOAWebhook.Middleware;
using ZaloOAWebhook.Repository;

var builder = WebApplication.CreateBuilder(args);

//config log
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt",
                    rollingInterval: RollingInterval.Day,// Mỗi ngày tạo file mới
                    rollOnFileSizeLimit: true,  // Tạo file mới khi file hiện tại đạt kích thước 
                    retainedFileCountLimit: 30) // Giới hạn số lượng file lưu trữ
    .CreateBootstrapLogger();

builder.Host.UseSerilog(); // Sử dụng Serilog làm logger

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ZaloOatestContext>();
builder.Services.AddSingleton<StoreDBContext>();

builder.Services.AddScoped<IZaloOAAccountRepository, ZaloOAAccountRepository>();
builder.Services.AddScoped<ICustomerRepoitory, CustomerRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
