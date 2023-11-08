using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;
using MyCar.Context;

namespace MyCars.Controllers;

[Route("api/[controller]")]
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

[Route("recreate_database")]
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