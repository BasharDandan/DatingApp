using Microsoft.EntityFrameworkCore;
using API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=datingapp.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapControllers();

app.Run();
