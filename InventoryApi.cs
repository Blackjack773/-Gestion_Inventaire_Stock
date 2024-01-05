// InventoryApi.cs
using System.Net;

public static class InventoryApi
{
    private static readonly HttpListener listener = new HttpListener();

    public static void Start()
    {
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();

        Console.WriteLine("API : http://localhost:8080/api");

        while (true)
        {
            HttpListenerContext context = listener.GetContext();
            ApiRequestHandler.ProcessRequest(context);
        }
    }
}
