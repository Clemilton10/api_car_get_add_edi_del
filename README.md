# Api de Carro

| Método | Rota                  | Descrição                        |
| ------ | --------------------- | -------------------------------- |
| GET    | api/api               | Exibir todos os carros           |
| POST   | api/api               | Cadastro de um carro             |
| PUT    | api/api/{id}          | Edição de um carro               |
| DELETE | api/api/{id}          | Exclusão de um carro             |
| POST   | api/filter            | where, order, meaning, page, qtd |
| GET    | api/recreate_database | -                                |

### Criando o projeto

```sh
dotnet new sln -n api_car
dotnet new webapi -f net6.0 -n api_car
dotnet sln add api_car
```

### Instalando os pacotes

📝 api_car/api_car.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
	<PropertyGroup>
		<TargetFramework>net6.0</TargetFramework>
		<Nullable>enable</Nullable>
		<ImplicitUsings>enable</ImplicitUsings>
	</PropertyGroup>
	<ItemGroup>
		<PackageReference Include="Microsoft.EntityFrameworkCore" Version="7.0.13" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="7.0.13" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="2.0.3">
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
			<PrivateAssets>all</PrivateAssets>
		</PackageReference>
		<PackageReference Include="Microsoft.EntityFrameworkCore.Tools.DotNet" Version="2.0.3" />
		<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
		<PackageReference Include="Swashbuckle.AspNetCore.Annotations" Version="6.5.0" />
		<PackageReference Include="System.Linq.Dynamic.Core" Version="1.3.5" />
	</ItemGroup>
</Project>
```

### Configurando o servidor e portas

📝 api_car/Properties/launchSettings.json

```json
{
	"$schema": "https://json.schemastore.org/launchsettings.json",
	"profiles": {
		"backend005": {
			"commandName": "Project",
			"dotnetRunMessages": true,
			"launchBrowser": true,
			"launchUrl": "swagger",
			"applicationUrl": "https://localhost:5011;http://localhost:5010",
			"environmentVariables": {
				"ASPNETCORE_ENVIRONMENT": "Development"
			}
		}
	}
}
```

### Anotando o banco de dados no appsettings.json

📝 api_car/appsettings.json

```json
{
	"ConnectionStrings": {
		"ServerConnection": "Data Source=Cars.db"
	},
	"Logging": {
		"LogLevel": {
			"Default": "Information",
			"Microsoft.AspNetCore": "Warning"
		}
	},
	"AllowedHosts": "*"
}
```

### Criando a configuração da tabelas

```sh
cd api_car
mkdir Context
touch Context/AppDbContext.cs
```

📝 api_car/Context/AppDbContext.cs

```csharp
using Microsoft.EntityFrameworkCore;
using MyCar.Models;

namespace MyCar.Context;

public class CarDbContext : DbContext
{
	public CarDbContext(DbContextOptions<CarDbContext> options) : base(options)
	{
	}

	public DbSet<Car> Cars { get; set; }
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		IConfiguration configuration = new ConfigurationBuilder()
		.SetBasePath(Directory.GetCurrentDirectory())
		.AddJsonFile("appsettings.json", true)
		.Build();
		optionsBuilder.UseSqlite(configuration.GetConnectionString("ServerConnection"));
	}
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Car>()
			.ToTable("Car");

		modelBuilder.Entity<Car>()
			.HasKey(c => c.Id);

		modelBuilder.Entity<Car>()
			.Property(c => c.Plate)
			.HasColumnType("TEXT")
			.IsRequired()
			.HasDefaultValue("");

		modelBuilder.Entity<Car>()
			.Property(c => c.Brand)
			.HasColumnType("TEXT")
			.IsRequired()
			.HasDefaultValue("");

		modelBuilder.Entity<Car>()
			.Property(c => c.Price)
			.HasColumnType("REAL")
			.IsRequired()
			.HasDefaultValue(0.0m);
	}
}
```

### Configurando as rotas (Controllers)

```sh
mkdir Controllers
touch Controllers/CarsController.cs
```

📝 api_car/Controllers/CarsController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCar.Context;
using System.Linq.Dynamic.Core;

namespace MyCars.Controllers;

[Route("[controller]")]
[ApiController]
public class apiController : ControllerBase
{
	private readonly CarDbContext db;

	public apiController(CarDbContext _db)
	{
		db = _db;
	}

	[HttpGet]
	public async Task<IActionResult> GetCars()
	{
		try
		{
			return Ok(new
			{
				success = true,
				data = await db.Cars.ToListAsync()
			});
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Houve algum erro: {ex.GetType}\n\n\n\n{ex.StackTrace}\n\n\n\n{ex.StackTrace}");
			return StatusCode(202, $"Houve algum erro: {ex.GetType}\n\n\n\n{ex.StackTrace}\n\n\n\n{ex.StackTrace}");
		}
	}

	[HttpPost]
	public async Task<IActionResult> PostCar([FromBody] MyCar.Models.CarPost data)
	{
		if (
			data == null ||
			string.IsNullOrEmpty(data.Plate) ||
			data.Plate == "string" ||
			string.IsNullOrEmpty(data.Brand) ||
			data.Brand == "string" ||
			!decimal.TryParse(data.Price.ToString(), out var a)
		)
		{
			return BadRequest(
				new
				{
					status_id = 0,
					status = "Algum campo está incorreto",
					data = new List<MyCar.Models.Car>()// lista vazia
				}
			);
		}
		var o = new MyCar.Models.Car
		{
			Plate = data.Plate,
			Brand = data.Brand,
			Price = data.Price
		};
		db.Cars.Add(o);
		await db.SaveChangesAsync();
		return Ok(
			new
			{
				status_id = 1,
				status = "success",
				data = new List<MyCar.Models.Car> { o }//colocando a var o em uma lista
			}
		);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> PutCar(int id, [FromBody] MyCar.Models.CarPost data)
	{
		if (
			data == null ||
			string.IsNullOrEmpty(data.Plate) ||
			data.Plate == "string" ||
			string.IsNullOrEmpty(data.Brand) ||
			data.Brand == "string" ||
			!decimal.TryParse(data.Price.ToString(), out var a)
		)
		{
			return BadRequest(
				new
				{
					status_id = 0,
					status = "Algum campo está incorreto",
					data = new List<MyCar.Models.Car>()// lista vazia
				}
			);
		}
		var rs = db.Cars.FirstOrDefault(c => c.Id == id);
		if (rs == null)
		{
			return NotFound(
				new
				{
					status_id = -1,
					status = "Registro não encontrado",
					data = new List<MyCar.Models.Car>()// lista vazia
				}
			);
		}
		rs.Plate = data.Plate;
		rs.Brand = data.Brand;
		rs.Price = data.Price;
		await db.SaveChangesAsync();
		return Ok(
			new
			{
				status_id = 1,
				status = "success",
				data = new List<MyCar.Models.Car> { rs }//colocando a var o em uma lista
			}
		);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteCar(int id)
	{
		var rs = db.Cars.FirstOrDefault(c => c.Id == id);
		if (rs == null)
		{
			return NotFound(
				new
				{
					status_id = -1,
					status = "Registro não encontrado",
					data = new List<MyCar.Models.Car>()// lista vazia
				}
			);
		}
		db.Cars.Remove(rs);
		await db.SaveChangesAsync();
		return Ok(
			new
			{
				status_id = 1,
				status = "success",
				data = new List<MyCar.Models.Car>()// lista vazia
			}
		);
	}
}

[Route("api/filter")]
public class apiFilter : ControllerBase
{
	private readonly CarDbContext db;

	public apiFilter(CarDbContext _db)
	{
		db = _db;
	}

	[HttpPost]
	public async Task<IActionResult> PostFilter([FromBody] MyCar.Models.CarFilter data)
	{
		/*if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}*/
		try
		{
			string where = "";
			if (
				data != null &&
				!string.IsNullOrEmpty(data.where) &&
				data.where != "string"
			)
			{
				where = $"WHERE {data.where}";
			}

			string order = "ORDER BY LivroId ASC";
			if (
				data != null &&
				!string.IsNullOrEmpty(data.order) &&
				data.order != "string" &&
				!string.IsNullOrEmpty(data.meaning) &&
				data.meaning != "string"
			)
			{
				order = $"ORDER BY {data.order} {data.meaning}";
			}

			int qtd = 0;
			int qtd_pages = 1;
			int page = 1;
			int count = 0;

			string limit = "";
			if (
				data != null &&
				!string.IsNullOrEmpty(data.order) &&
				data.order != "string" &&
				!string.IsNullOrEmpty(data.meaning) &&
				data.meaning != "string" &&
				data.page != null &&
				int.TryParse(data.page.ToString(), out var p) &&
				Convert.ToInt32(data.page.ToString()) > 0
			)
			{
				qtd = Convert.ToInt32(data.qtd.ToString());
				page = Convert.ToInt32(data.page.ToString());

				var query = db.Cars.AsQueryable();
				if (
					data != null &&
					!string.IsNullOrEmpty(data.where) &&
					data.where != "string"
				)
				{
					query = query.Where(data.where);
				}
				count = await query.CountAsync();
				if (count > 0 && qtd > 0)
				{
					float div = (float)count / qtd;
					qtd_pages = Convert.ToInt32(Math.Ceiling(div).ToString());
				}

				int offset = (page - 1) * qtd;
				limit = $"LIMIT {qtd} OFFSET {offset}";
			}

			var cars = await db.Cars
			.FromSqlRaw($"SELECT * FROM Car {where} {order} {limit}")
			.ToListAsync();
			return Ok(
				new
				{
					status_id = 1,
					status = "success",
					qtd = qtd,
					page = page,
					count = count,
					qtd_pages = qtd_pages,
					data = cars
				}
			);
		}
		catch (Exception ex)
		{
			return StatusCode(
				StatusCodes.Status500InternalServerError,
				ex.Message
			);
		}
	}
}

[Route("api/recreate_database")]
public class recreateDatabase : ControllerBase
{
	private readonly CarDbContext db;

	public recreateDatabase(CarDbContext _db)
	{
		db = _db;
	}

	[HttpGet]
	public IActionResult GetRecreateDatabase()
	{
		db.Database.EnsureDeleted();
		db.Database.EnsureCreated();
		return Ok(new { success = true });
	}
}
```

### Configurando o modelo (Colunas)

```sh
mkdir Models
touch Models/CarModel.cs
```

📝 api_car/Controllers/CarModel.cs

```csharp
namespace MyCar.Models;

public class Car
{
	public int? Id { get; set; }
	public string? Plate { get; set; } = string.Empty;
	public string? Brand { get; set; } = string.Empty;
	public decimal? Price { get; set; } = 0.0m;
}
public class CarPost
{
	public string? Plate { get; set; } = string.Empty;
	public string? Brand { get; set; } = string.Empty;
	public decimal? Price { get; set; } = 0.0m;
}

public class CarFilter
{
	public string? where { get; set; } = string.Empty;
	public string? order { get; set; } = "Id";
	public string? meaning { get; set; } = "ASC";
	public int? page { get; set; } = 1;
	public int? qtd { get; set; } = 5;
}
```

### Configurando a Program.cs

📝 api_car/Program.cs

```csharp
// pegando a informacao do banco de dados do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("ServerConnection");

// Criando o contexto do banco de dados
builder.Services
	.AddDbContext<CarDbContext>(
		options => options.UseSqlite(connectionString));
```
