using Microsoft.EntityFrameworkCore;
using MyCar.Context;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// pegando a informacao do banco de dados do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("ServerConnection");

// Criando o contexto do banco de dados
builder.Services
	.AddDbContext<CarDbContext>(
		options => options.UseSqlite(connectionString));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
