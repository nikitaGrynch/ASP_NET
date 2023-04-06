namespace ASP_NET.Models.Home;

public class Model
{
    public String Header { get; set; } = null!;
    public String Title { get; set; } = null!;
    public List<String> Departments { get; set; } = null!;
    public List<Product> Products { get; set; } = null!;
}

public class Product
{
    public String Name { get; set; } = null!;
    public double Price { get; set; }
    
}