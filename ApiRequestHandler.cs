#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
//using System.Threading.Tasks;
using MySql.Data.MySqlClient;

public static class ApiRequestHandler
{
    private static List<Article> articles = new List<Article>();
    //private static List<User> users = new List<User>();

    public static void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        if (request != null)
        {
            string requestMethod = request.HttpMethod;
            string endpoint = request.Url.LocalPath;

            using (MySqlConnection connection = DatabaseManager.GetConnection())
            {
                switch (requestMethod)
                {
                    case "GET":
                        if (endpoint.StartsWith("/api/articles"))
                        {
                            HandleGetArticlesRequest(context, endpoint, connection);
                        }
                        else if (endpoint.StartsWith("/api/users"))
                        {
                            HandleGetUsersRequest(context, endpoint, connection);
                        }
                        else
                        {
                            SendResponse(context, HttpStatusCode.NotFound, "API users : http://localhost:8080/api/users  |||||  API articles : http://localhost:8080/api/articles");
                        }
                        break;
                    case "POST":
                        if (endpoint.StartsWith("/api/articles"))
                        {
                            HandlePostArticleRequest(context, endpoint, connection);
                        }
                        else if (endpoint.StartsWith("/api/users"))
                        {
                            HandlePostUserRequest(context, endpoint, connection);
                        }
                        else
                        {
                            SendResponse(context, HttpStatusCode.NotFound, "API users : http://localhost:8080/api/users  |||||  API articles : http://localhost:8080/api/articles");
                        }
                        break;
                    case "PUT":
                        if (endpoint.StartsWith("/api/articles"))
                        {
                            HandlePutArticleRequest(context, endpoint, connection);
                        }
                        else if (endpoint.StartsWith("/api/users"))
                        {
                            HandlePutUserRequest(context, endpoint, connection);
                        }
                        else
                        {
                            SendResponse(context, HttpStatusCode.NotFound, "API users : http://localhost:8080/api/users  |||||  API articles : http://localhost:8080/api/articles");
                        }
                        break;
                    case "DELETE":
                        if (endpoint.StartsWith("/api/articles"))
                        {
                            HandleDeleteArticleRequest(context, endpoint, connection);
                        }
                        else if (endpoint.StartsWith("/api/users"))
                        {
                            HandleDeleteUserRequest(context, endpoint, connection);
                        }
                        else
                        {
                            SendResponse(context, HttpStatusCode.NotFound, "API users : http://localhost:8080/api/users  |||||  API articles : http://localhost:8080/api/articles");
                        }
                        break;
                    default:
                        SendResponse(context, HttpStatusCode.MethodNotAllowed, "Method not allowed");
                        break;
                }
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.BadRequest, "Bad request");
        }
    }
    private static void HandleGetArticlesRequest(HttpListenerContext context, string endpoint, MySqlConnection connection)
    {
        if (endpoint.StartsWith("/api/articles"))
        {
            if (endpoint == "/api/articles")
            {
                // Récupérer tous les articles depuis la base de données
                List<Article> articlesFromDb = GetArticlesFromDatabase(connection);
                string articlesJson = JsonSerializer.Serialize(articlesFromDb);
                SendResponse(context, HttpStatusCode.OK, articlesJson);
            }
            else
            {
                // Récupérer un article spécifique depuis la base de données
                int articleId = GetArticleIdFromEndpoint(endpoint);
                if (articleId != -1)
                {
                    Article? article = GetArticleByIdFromDatabase(connection, articleId);
                    if (article != null)
                    {
                        SendResponse(context, HttpStatusCode.OK, GetArticleJson(article));
                    }
                    else
                    {
                        SendResponse(context, HttpStatusCode.NotFound, "Article not found");
                    }
                }
                else
                {
                    SendResponse(context, HttpStatusCode.BadRequest, "No article ID");
                }
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.NotFound, "Endpoint not found");
        }
    }

    private static List<Article> GetArticlesFromDatabase(MySqlConnection connection)
    {
        // Utilise une requête SQL pour récupérer tous les articles depuis la base de données
        string query = "SELECT * FROM produit";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        using MySqlDataReader reader = cmd.ExecuteReader();

        List<Article> articlesFromDb = new List<Article>();
        while (reader.Read())
        {
            Article article = new Article
            {
                Id = reader.GetInt32("id_produit"),
                Name = reader.GetString("prod"),
                Description = reader.GetString("description"),
                Price_HT = reader.GetFloat("prix_ht"),
                Price_TTC = reader.GetFloat("prix_ttc"),
            };
            articlesFromDb.Add(article);
        }

        return articlesFromDb;
    }

    private static Article? GetArticleByIdFromDatabase(MySqlConnection connection, int articleId)
    {
        // Utilise une requête SQL pour récupérer un article spécifique depuis la base de données
        string query = "SELECT * FROM produit WHERE id_produit = @Id";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Id", articleId);

        using MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            Article article = new Article
            {
                Id = reader.GetInt32("id_produit"),
                Name = reader.GetString("prod"),
                Description = reader.GetString("description"),
                Price_HT = reader.GetFloat("prix_ht"),
                Price_TTC = reader.GetFloat("prix_ttc"),
            };
            return article;
        }
        return null;
    }

    private static void HandlePostArticleRequest(HttpListenerContext context, string endpoint, MySqlConnection connection)
    {
        if (endpoint == "/api/articles")
        {
            // Ajouter un nouvel article à la base de données
            Article newArticle = ParseArticleFromBody(context.Request);
            if (newArticle != null)
            {
                AddArticleToDatabase(connection, newArticle);
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

    private static void AddArticleToDatabase(MySqlConnection connection, Article article)
    {
        // Utilise une requête SQL pour ajouter un nouvel article à la base de données
        string query = "INSERT INTO produit (prod) VALUES (@Name)";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Name", article.Name);
        // Ajoute les autres paramètres en fonction de ta structure de base de données

        cmd.ExecuteNonQuery();
    }

    private static void HandlePutArticleRequest(HttpListenerContext context, string endpoint, MySqlConnection connection)
    {
        // Mettre à jour un article existant dans la base de données
        int articleId = GetArticleIdFromEndpoint(endpoint);
        if (articleId != -1)
        {
            Article? existingArticle = GetArticleByIdFromDatabase(connection, articleId);
            if (existingArticle != null)
            {
                Article updatedArticle = ParseArticleFromBody(context.Request);
                if (updatedArticle != null)
                {
                    UpdateArticleInDatabase(connection, existingArticle, updatedArticle);
                    SendResponse(context, HttpStatusCode.OK, "Article mis à jour.");
                }
                else
                {
                    SendResponse(context, HttpStatusCode.BadRequest, "No article data");
                }
            }
            else
            {
                SendResponse(context, HttpStatusCode.NotFound, "Article not found");
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.BadRequest, "No article ID");
        }
    }   

    private static void UpdateArticleInDatabase(MySqlConnection connection, Article existingArticle, Article updatedArticle)
    {
        // Utilise une requête SQL pour mettre à jour un article dans la base de données
        string query = "UPDATE produit SET prod = @Name WHERE id_produit = @Id";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Name", updatedArticle.Name);
        cmd.Parameters.AddWithValue("@Id", existingArticle.Id);
    
        cmd.ExecuteNonQuery();
    }
    private static void HandleDeleteArticleRequest(HttpListenerContext context, string endpoint, MySqlConnection connection)
    {
        // Supprimer un article de la base de données
        int articleId = GetArticleIdFromEndpoint(endpoint);
        if (articleId != -1)
        {
            Article? existingArticle = GetArticleByIdFromDatabase(connection, articleId);
            if (existingArticle != null)
            {
                RemoveArticleFromDatabase(connection, existingArticle);
                SendResponse(context, HttpStatusCode.OK, "Deleted.");
            }
            else
            {
                SendResponse(context, HttpStatusCode.NotFound, "Article not found");
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.BadRequest, "No article ID");
        }
    }
    
    private static void RemoveArticleFromDatabase(MySqlConnection connection, Article existingArticle)
    {
        // Utilise une requête SQL pour supprimer un article de la base de données
        string query = "DELETE FROM produit WHERE id_produit = @Id";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Id", existingArticle.Id);

        cmd.ExecuteNonQuery();
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

    //Users :    
    private static void HandleGetUsersRequest(HttpListenerContext context, string endpoint, MySqlConnection connection)
    {
        if (endpoint.StartsWith("/api/users"))
        {
            if (endpoint == "/api/users")
            {
                // Récupérer tous les utilisateurs depuis la base de données
                List<User> usersFromDb = GetUsersFromDatabase(connection);
                string usersJson = JsonSerializer.Serialize(usersFromDb);
                SendResponse(context, HttpStatusCode.OK, usersJson);
            }
            else
            {
                // Récupérer un utilisateur spécifique depuis la base de données
                int userId = GetUserIdFromEndpoint(endpoint);
                if (userId != -1)
                {
                    User? user = GetUserByIdFromDatabase(connection, userId);
                    if (user != null)
                    {
                        SendResponse(context, HttpStatusCode.OK, GetUserJson(user));
                    }
                    else
                    {
                        SendResponse(context, HttpStatusCode.NotFound, "User not found");
                    }
                }
                else
                {
                    SendResponse(context, HttpStatusCode.BadRequest, "No user ID");
                }
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.NotFound, "Endpoint not found");
        }
    }

    private static List<User> GetUsersFromDatabase(MySqlConnection connection)
    {
        string query = "SELECT * FROM client";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        using MySqlDataReader reader = cmd.ExecuteReader();

        List<User> usersFromDb = new List<User>();
        while (reader.Read())
        {
            User user = new User
            {
                Id = reader.GetInt32("id_client"),
                Name = reader.GetString("nom"),
                Surname = reader.GetString("prénom"),
                Address = reader.GetString("adresse1"),
                Cp = reader.GetInt32("cp"),
                City = reader.GetString("ville"),
                Phone = reader.GetInt32("tel"),
                Mail = reader.GetString("mail"),
                Password = reader.GetString("mdp"),
            };
            usersFromDb.Add(user);
        }

        return usersFromDb;
    }

    private static User? GetUserByIdFromDatabase(MySqlConnection connection, int userId)
    {
        string query = "SELECT * FROM client WHERE id_client = @Id";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Id", userId);

        using MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            User user = new User
            {
                Id = reader.GetInt32("id_client"),
                Name = reader.GetString("nom"),
                Surname = reader.GetString("prénom"),
                Address = reader.GetString("adresse1"),
                Cp = reader.GetInt32("cp"),
                City = reader.GetString("ville"),
                Phone = reader.GetInt32("tel"),
                Mail = reader.GetString("mail"),
                Password = reader.GetString("mdp"),
            };
            return user;
        }
        return null;
    }

    private static void HandlePostUserRequest(HttpListenerContext context, string endpoint, MySqlConnection connection)
    {
        if (endpoint == "/api/users")
        {
            // Ajouter un nouvel utilisateur à la base de données
            User newUser = ParseUserFromBody(context.Request);
            if (newUser != null)
            {
                AddUserToDatabase(connection, newUser);
                SendResponse(context, HttpStatusCode.Created, "User added");
            }
            else
            {
                SendResponse(context, HttpStatusCode.BadRequest, "No user data");
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.NotFound, "Endpoint not found");
        }
    }

    private static void AddUserToDatabase(MySqlConnection connection, User user)
    {
        // Utilise une requête SQL pour ajouter un nouvel utilisateur à la base de données
        string query = "INSERT INTO client (nom, prénom, adresse1, cp, ville, tel, mail, mdp) " +
                       "VALUES (@Name, @Surname, @Address, @Cp, @City, @Phone, @Mail, @Password)";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Name", user.Name);
        cmd.Parameters.AddWithValue("@Surname", user.Surname);
        cmd.Parameters.AddWithValue("@Address", user.Address);
        cmd.Parameters.AddWithValue("@Cp", user.Cp);
        cmd.Parameters.AddWithValue("@City", user.City);
        cmd.Parameters.AddWithValue("@Phone", user.Phone);
        cmd.Parameters.AddWithValue("@Mail", user.Mail);
        cmd.Parameters.AddWithValue("@Password", user.Password);

        cmd.ExecuteNonQuery();
    }

    private static void HandlePutUserRequest(HttpListenerContext context, string endpoint, MySqlConnection connection)
    {
        // Mettre à jour un utilisateur existant dans la base de données
        int userId = GetUserIdFromEndpoint(endpoint);
        if (userId != -1)
        {
            User? existingUser = GetUserByIdFromDatabase(connection, userId);
            if (existingUser != null)
            {
                User updatedUser = ParseUserFromBody(context.Request);
                if (updatedUser != null)
                {
                    UpdateUserInDatabase(connection, existingUser, updatedUser);
                    SendResponse(context, HttpStatusCode.OK, "User updated");
                }
                else
                {
                    SendResponse(context, HttpStatusCode.BadRequest, "No user data");
                }
            }
            else
            {
                SendResponse(context, HttpStatusCode.NotFound, "User not found");
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.BadRequest, "No user ID");
        }
    }

    private static void UpdateUserInDatabase(MySqlConnection connection, User existingUser, User updatedUser)
    {
        // Utilise une requête SQL pour mettre à jour un utilisateur dans la base de données
        string query = "UPDATE client SET nom = @Name, prénom = @Surname, adresse1 = @Address, " +
                       "cp = @Cp, ville = @City, tel = @Phone, mail = @Mail, mdp = @Password " +
                       "WHERE id_client = @Id";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Name", updatedUser.Name);
        cmd.Parameters.AddWithValue("@Surname", updatedUser.Surname);
        cmd.Parameters.AddWithValue("@Address", updatedUser.Address);
        cmd.Parameters.AddWithValue("@Cp", updatedUser.Cp);
        cmd.Parameters.AddWithValue("@City", updatedUser.City);
        cmd.Parameters.AddWithValue("@Phone", updatedUser.Phone);
        cmd.Parameters.AddWithValue("@Mail", updatedUser.Mail);
        cmd.Parameters.AddWithValue("@Password", updatedUser.Password);
        cmd.Parameters.AddWithValue("@Id", existingUser.Id);

        cmd.ExecuteNonQuery();
    }

    private static void HandleDeleteUserRequest(HttpListenerContext context, string endpoint, MySqlConnection connection)
    {
        // Supprimer un utilisateur de la base de données
        int userId = GetUserIdFromEndpoint(endpoint);
        if (userId != -1)
        {
            User? existingUser = GetUserByIdFromDatabase(connection, userId);
            if (existingUser != null)
            {
                RemoveUserFromDatabase(connection, existingUser);
                SendResponse(context, HttpStatusCode.OK, "User deleted");
            }
            else
            {
                SendResponse(context, HttpStatusCode.NotFound, "User not found");
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.BadRequest, "No user ID");
        }
    }

    private static void RemoveUserFromDatabase(MySqlConnection connection, User existingUser)
    {
        // Utilise une requête SQL pour supprimer un utilisateur de la base de données
        string query = "DELETE FROM client WHERE id_client = @Id";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Id", existingUser.Id);

        cmd.ExecuteNonQuery();
    }

    private static string GetUsersJson()
    {
        // Implémente cette méthode si tu veux retourner une liste de tous les utilisateurs au format JSON
        // return JsonSerializer.Serialize(users);
        return "";
    }

    private static string GetUserJson(User user)
    {
        return JsonSerializer.Serialize(user);
    }

    private static User? ParseUserFromBody(HttpListenerRequest request)
    {
        try
        {
            using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
            {
                string requestBody = reader.ReadToEnd();
                return JsonSerializer.Deserialize<User>(requestBody);
            }
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static int GetUserIdFromEndpoint(string endpoint)
    {
        string[] segments = endpoint.Split('/');
        if (segments.Length >= 3 && int.TryParse(segments[3], out int userId))
        {
            return userId;
        }
        return -1;
    }

}
