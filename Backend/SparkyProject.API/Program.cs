using Microsoft.EntityFrameworkCore;
using SparkyProject.API.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO: Register AppDbContext with connection string
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// TODO: Register JWT authentication
// builder.Services.AddAuthentication(...).AddJwtBearer(...);
// builder.Services.AddAuthorization();

// TODO: register EmailService (see Services/EmailService.cs) for DI

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// TODO: app.UseAuthentication();
// TODO: app.UseAuthorization();

app.MapControllers();

app.Run();
