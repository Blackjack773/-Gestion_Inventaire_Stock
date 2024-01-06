//Main
using System;

class Program
{
    static void Main()
    {
        InventoryApi.Start();
    }
}










//// Program.cs
//#nullable disable
//using System;
//using System.Net;
//using System.Threading;
//
//class Program
//{
//    static void Main()
//    {
//        string baseUri = "http://localhost:8080/";
//        HttpListener listener = new HttpListener();
//        listener.Prefixes.Add(baseUri);
//        listener.Start();
//        Console.WriteLine($"Serveur en cours d'écoute sur {baseUri}");
//
//        while (true)
//        {
//            HttpListenerContext context = listener.GetContext();
//            ThreadPool.QueueUserWorkItem((o) =>
//            {
//                RequestHandler.HandleRequest(context);
//            });
//        }
//    }
//}
