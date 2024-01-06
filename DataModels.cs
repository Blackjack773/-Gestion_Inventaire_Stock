// DataModels.cs
#nullable enable

public class Article
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public string Description{ get; set;}
    public float Price_HT { get; set;}
    public float Price_TTC { get; set;}
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set;}
    public string Address { get; set;}
    public int Cp { get; set;}
    public string City { get; set;}
    public int Phone { get; set;}
    public string Mail { get; set;}
    public string Password { get; set;}
}
