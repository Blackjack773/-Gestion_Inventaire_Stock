#nullable enable
using MySql.Data.MySqlClient;
using System;

public static class DatabaseManager
{
    //Connection à la DB
    private static readonly string ConnectionString = "Server=localhost;Database=ecommerce;User Id=Dams;Password=Dams;";
    
    public static MySqlConnection GetConnection()
    {
        MySqlConnection connection = new MySqlConnection(ConnectionString);
        try
        {
            connection.Open();
            return connection;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
            throw;
        }
    }
}
