using System;
using System.IO;
using System.Net;

class SimpleApi
{
    static void Main()
    {
        // Set up the listener on port 8080
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("Listening on http://localhost:8080/");

        while (true)
        {
            // Wait for a request to come in
            var context = listener.GetContext();

            // Get the request method (GET, POST, etc.)
            var requestMethod = context.Request.HttpMethod;

            // Get the request URL
            var requestUrl = context.Request.Url;

            Console.WriteLine($"Request received: {requestMethod} {requestUrl}");

            // Handle the request based on the method
            if (requestMethod == "GET")
            {
                // Handle GET request
                HandleGetRequest(context);
            }
            else if (requestMethod == "POST")
            {
                // Handle POST request
                HandlePostRequest(context);
            }
            else
            {
                // Handle other request methods as needed
                context.Response.StatusCode = 405; // Method Not Allowed
                context.Response.Close();
            }
        }
    }
    static void HandleGetRequest(HttpListenerContext context)
    {
        // Send a simple response for GET requests
        string response = "Hello, this is a GET request!";
        byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(response);

        // Set the content type and length
        context.Response.ContentType = "text/plain";
        context.Response.ContentLength64 = responseBytes.Length;

        // Write the response to the output stream
        context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);

        // Close the output stream
        context.Response.Close();
    }

    static void HandlePostRequest(HttpListenerContext context)
    {
        // Read the request body for POST requests
        using (var reader = new StreamReader(context.Request.InputStream))
        {
            string requestBody = reader.ReadToEnd();
            Console.WriteLine($"POST request body: {requestBody}");

            // Handle the request body as needed

            // Send a simple response
            string response = "Hello, this is a POST request!";
            byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(response);

            // Set the content type and length
            context.Response.ContentType = "text/plain";
            context.Response.ContentLength64 = responseBytes.Length;

            // Write the response to the output stream
            context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);

            // Close the output stream
            context.Response.Close();
        }
    }
}