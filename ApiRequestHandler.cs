#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;

public static class ApiRequestHandler
{
    private static List<Article> articles = new List<Article>();

    public static void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        if (request != null)
        {
            string requestMethod = request.HttpMethod;
            string endpoint = request.Url.LocalPath;

            if (endpoint.StartsWith("/api/articles"))
            {
                switch (requestMethod)
                {
                    case "GET":
                        HandleGetRequest(context, endpoint);
                        break;
                    case "POST":
                        HandlePostRequest(context, endpoint);
                        break;
                    case "PUT":
                        HandlePutRequest(context, endpoint);
                        break;
                    case "DELETE":
                        HandleDeleteRequest(context, endpoint);
                        break;
                    case "GETNAME":
                        HandleGetArticleByName(context, endpoint);
                        break;
                    default:
                        SendResponse(context, HttpStatusCode.MethodNotAllowed, "Method not allowed");
                        break;
                }
            }
            else if (endpoint.StartsWith("/api/users"))
            {
            // Si voulu (mais non requis dans les tâches --> articles requis only), possibilité d'ajouter des methodes pour les users
                SendResponse(context, HttpStatusCode.OK, "Not able ////");

            }
            else if (endpoint.StartsWith("/api"))
            {
                var userLink = "http://localhost:8080/api/users";
                var articleLink = "http://localhost:8080/api/articles";
                SendResponse(context, HttpStatusCode.OK, "Api utilisateurs : " + userLink + "  ||||  Api Articles : " + articleLink);
            }
            else
            {
                SendResponse(context, HttpStatusCode.NotFound, "Wrong link ?");
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.BadRequest, "Bad request");
        }
    }



    private static void HandleGetRequest(HttpListenerContext context, string endpoint)
    {
        if (endpoint.StartsWith("/api/articles"))
        {
            if (endpoint == "/api/articles")
            {
                // Récupérer tous les articles
                string articlesJson = GetArticlesJson();
                SendResponse(context, HttpStatusCode.OK, articlesJson);
            }
            else
            {
                // Récupérer un article spécifique
                int articleId = GetArticleIdFromEndpoint(endpoint);
                if (articleId != -1)
                {
                    Article? article = GetArticleById(articleId);
                    if (article != null)
                    {
                        SendResponse(context, HttpStatusCode.OK, "Détails de l'article: " + GetArticleJson(article));
                    }
                    else
                    {
                        SendResponse(context, HttpStatusCode.NotFound, "Article not found");
                    }
                }
                else
                {
                    SendResponse(context, HttpStatusCode.BadRequest, "Invalid article ID");
                }
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.NotFound, "Endpoint not found");
        }
    }

    private static void HandlePostRequest(HttpListenerContext context, string endpoint)
    {
        if (endpoint == "/api/articles")
        {
            // Ajouter un nouvel article
            Article newArticle = ParseArticleFromBody(context.Request);
            if (newArticle != null)
            {
                AddArticle(newArticle);
                SendResponse(context, HttpStatusCode.Created, "Article ajouté avec succès");
            }
            else
            {
                SendResponse(context, HttpStatusCode.BadRequest, "Invalid article data");
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.NotFound, "Endpoint not found");
        }
    }

    private static void HandlePutRequest(HttpListenerContext context, string endpoint)
    {
        if (endpoint.StartsWith("/api/articles"))
        {
            // Mettre à jour un article existant
            int articleId = GetArticleIdFromEndpoint(endpoint);
            if (articleId != -1)
            {
                Article? existingArticle = GetArticleById(articleId);
                if (existingArticle != null)
                {
                    Article updatedArticle = ParseArticleFromBody(context.Request);
                    if (updatedArticle != null)
                    {
                        UpdateArticle(existingArticle, updatedArticle);
                        SendResponse(context, HttpStatusCode.OK, "Article mis à jour avec succès");
                    }
                    else
                    {
                        SendResponse(context, HttpStatusCode.BadRequest, "Invalid article data");
                    }
                }
                else
                {
                    SendResponse(context, HttpStatusCode.NotFound, "Article not found");
                }
            }
            else
            {
                SendResponse(context, HttpStatusCode.BadRequest, "Invalid article ID");
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.NotFound, "Endpoint not found");
        }
    }

    private static void HandleDeleteRequest(HttpListenerContext context, string endpoint)
    {
        if (endpoint.StartsWith("/api/articles"))
        {
            // Supprimer un article
            int articleId = GetArticleIdFromEndpoint(endpoint);
            if (articleId != -1)
            {
                Article? existingArticle = GetArticleById(articleId);
                if (existingArticle != null)
                {
                    RemoveArticle(existingArticle);
                    SendResponse(context, HttpStatusCode.OK, "Article supprimé avec succès");
                }
                else
                {
                    SendResponse(context, HttpStatusCode.NotFound, "Article not found");
                }
            }
            else
            {
                SendResponse(context, HttpStatusCode.BadRequest, "Invalid article ID");
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.NotFound, "Endpoint not found");
        }
    }

    private static void SendResponse(HttpListenerContext context, HttpStatusCode statusCode, string responseContent)
    {
        context.Response.StatusCode = (int)statusCode;
        byte[] buffer = Encoding.UTF8.GetBytes(responseContent);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.Close();
    }

    private static string GetArticlesJson()
    {
        return JsonSerializer.Serialize(articles);
    }

    private static string GetArticleJson(Article article)
    {
        return JsonSerializer.Serialize(article);
    }

    private static Article? ParseArticleFromBody(HttpListenerRequest request)
{
    try
    {
        using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
        {
            string requestBody = reader.ReadToEnd();
            return JsonSerializer.Deserialize<Article>(requestBody);
        }
    }
    catch (JsonException)
    {
        return null;
    }
}

private static int GetArticleIdFromEndpoint(string endpoint)
{
    string[] segments = endpoint.Split('/');
    if (segments.Length >= 3 && int.TryParse(segments[3], out int articleId))
    {
        return articleId;
    }
    return -1;
}

private static void HandleGetArticleByName(HttpListenerContext context, string endpoint)
{
    if (endpoint.StartsWith("/api/articles/name"))
    {
        // Récupérer un article par son nom
        string articleName = GetArticleNameFromEndpoint(endpoint);
        Console.WriteLine($"Recherche de l'article par nom : {articleName}");
        
        if (!string.IsNullOrEmpty(articleName))
        {
            Article? article = GetArticleByName(articleName);
            if (article != null)
            {
                SendResponse(context, HttpStatusCode.OK, "Détails de l'article: " + GetArticleJson(article));
            }
            else
            {
                SendResponse(context, HttpStatusCode.NotFound, "Article not found");
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.BadRequest, "Invalid article name");
        }
    }
    else
    {
        SendResponse(context, HttpStatusCode.NotFound, "Endpoint not found");
    }
}

private static string GetArticleNameFromEndpoint(string endpoint)
{
    string[] segments = endpoint.Split('/');
    if (segments.Length >= 4)
    {
        return segments[3];
    }
    return string.Empty;
}

private static Article? GetArticleByName(string articleName)
{
    return articles.FirstOrDefault(article => article.Name.Equals(articleName, StringComparison.OrdinalIgnoreCase));
}



    private static void AddArticle(Article article)
    {
        article.Id = articles.Count + 1;
        articles.Add(article);
    }

    private static void UpdateArticle(Article existingArticle, Article updatedArticle)
    {
        existingArticle.Name = updatedArticle.Name;
        existingArticle.Quantity = updatedArticle.Quantity;
    }

    private static void RemoveArticle(Article article)
    {
        articles.Remove(article);
    }

    private static Article? GetArticleById(int articleId)
    {
        return articles.FirstOrDefault(article => article.Id == articleId);
    }
}


