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