using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace MyCars.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CarsController : ControllerBase
{
	private readonly MyCar.Context.AppDbContext db;

	public CarsController()
	{
		db = new MyCar.Context.AppDbContext();
	}

	[HttpGet]
	public async Task<IActionResult> GetCars()
	{
		return Ok(new
		{
			success = true,
			data = await db.Cars.ToListAsync()
		});
	}

	[HttpPost]
	public async Task<IActionResult> PostCars([FromBody] MyCar.Models.CarPost data)
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
		db.SaveChanges();
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
	public async Task<IActionResult> PutCars(int id, [FromBody] MyCar.Models.CarPost data)
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
		db.SaveChanges();
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
	public async Task<IActionResult> DeleteCars(int id)
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
		db.SaveChanges();
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
[Route("recreate_database")]
public class recreateDatabase : ControllerBase
{
	private readonly MyCar.Context.AppDbContext db;

	public recreateDatabase()
	{
		db = new MyCar.Context.AppDbContext();
	}

	[HttpGet]
	public IActionResult GetRecreateDatabase()
	{
		db.Database.EnsureDeleted();
		db.Database.EnsureCreated();
		return Ok(new { success = true });
	}
}