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