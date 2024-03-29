#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
//using System.Threading.Tasks;
using MySql.Data.MySqlClient;


//Gestion des requêtes
public static class ApiRequestHandler
{
    private static List<Article> articles = new List<Article>();
    //private static List<User> users = new List<User>();

    public static void ProcessRequest(HttpListenerContext context)
    {
        //Appel des opé CRUD users et articles
        HttpListenerRequest request = context.Request;
        if (request != null)
        {
            string requestMethod = request.HttpMethod;
            string endpoint = request.Url.LocalPath;

            using (MySqlConnection connection = DatabaseManager.GetConnection())
            {
                switch (requestMethod)
                {
                    //Data
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
                    //Ajout
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
                    //Modifier
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
                    //Supprimer
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
                //Récupérer TOUS les articles depuis la DB
                List<Article> articlesFromDb = GetArticlesFromDatabase(connection);
                string articlesJson = JsonSerializer.Serialize(articlesFromDb);
                SendResponse(context, HttpStatusCode.OK, articlesJson);
            }
            else
            {
                //Récupérer article spécifique depuis la DB
                int articleId = GetArticleIdFromEndpoint(endpoint);
                string articleName = GetArticleNameFromEndpoint(endpoint);
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
                else if (articleName != null)
                {
                    Article? article = GetArticleByNameFromDatabase(connection, articleName);
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
                    SendResponse(context, HttpStatusCode.BadRequest, "No article Name or ID");
                }
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.NotFound, "Endpoint not found");
        }
    }

    private static List<Article> GetArticlesFromDatabase(MySqlConnection connection, string articleName = null)
    {
        //Récupérer TOUS les article via requête
        string query = "SELECT * FROM produit";
        using MySqlCommand cmd = new MySqlCommand(query, connection);

        if (!string.IsNullOrEmpty(articleName))
    {
        query += " WHERE prod = @ArticleName";
    }

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
        //Récupérer article spécifique de la DB via requête
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

    private static Article? GetArticleByNameFromDatabase(MySqlConnection connection, string articleName)
    {
        //Récupérer article spécifique de la DB via requête
        string query = "SELECT * FROM produit WHERE prod = @Name";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Name", articleName);

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
            //Ajout nouvel article à la DB
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
        //Ajout nouvel article à la DB via requête
        string query = "INSERT INTO produit (prod) VALUES (@Name)";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Name", article.Name);
        cmd.ExecuteNonQuery();
    }

    private static void HandlePutArticleRequest(HttpListenerContext context, string endpoint, MySqlConnection connection)
    {
        //Modifier un article de la DB
        int articleId = GetArticleIdFromEndpoint(endpoint);
        string articleName = GetArticleNameFromEndpoint(endpoint);
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
        else if (articleName != null)
        {
            Article? existingArticle = GetArticleByNameFromDatabase(connection, articleName);
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
            SendResponse(context, HttpStatusCode.BadRequest, "No article Name or ID");
        }
    }   

    private static void UpdateArticleInDatabase(MySqlConnection connection, Article existingArticle, Article updatedArticle)
    {
        //Modifier un article de la DB via requête
        string query = "UPDATE produit SET prod = @Name WHERE id_produit = @Id";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Name", updatedArticle.Name);
        cmd.Parameters.AddWithValue("@Id", existingArticle.Id);
        cmd.ExecuteNonQuery();
    }
    private static void HandleDeleteArticleRequest(HttpListenerContext context, string endpoint, MySqlConnection connection)
    {
        //Delete article de la DB
        int articleId = GetArticleIdFromEndpoint(endpoint);
        string articleName = GetArticleNameFromEndpoint(endpoint);
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
        else if (articleName != null)
        {
            Article? existingArticle = GetArticleByNameFromDatabase(connection, articleName);
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
            SendResponse(context, HttpStatusCode.BadRequest, "No article Name or ID");
        }
    }
    
    private static void RemoveArticleFromDatabase(MySqlConnection connection, Article existingArticle)
    {
        //Delete article de la DB via requête
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

    private static string GetArticleNameFromEndpoint(string endpoint)
{
    string[] segments = endpoint.Split('/');
    if (segments.Length >= 4)
    {
        return segments[3];
    }
    return null;
}


    

    //Users :    
    private static void HandleGetUsersRequest(HttpListenerContext context, string endpoint, MySqlConnection connection)
    {
        if (endpoint.StartsWith("/api/users"))
        {
            if (endpoint == "/api/users")
            {
                //Récup les users de la DB
                List<User> usersFromDb = GetUsersFromDatabase(connection);
                string usersJson = JsonSerializer.Serialize(usersFromDb);
                SendResponse(context, HttpStatusCode.OK, usersJson);
            }
            else
            {
                //Récup user spécifique de la DB
                int userId = GetUserIdFromEndpoint(endpoint);
                string userName = GetUserNameFromEndpoint(endpoint);
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
                else if (userName != null)
                {
                    User? user = GetUserByNameFromDatabase(connection, userName);
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
                    SendResponse(context, HttpStatusCode.BadRequest, "No user Name or ID");
                }
            }
        }
        else
        {
            SendResponse(context, HttpStatusCode.NotFound, "Endpoint not found");
        }
    }

    private static List<User> GetUsersFromDatabase(MySqlConnection connection, string userName = null)
    {
        //Récup les users de la DB via requête
        string query = "SELECT * FROM client";
        using MySqlCommand cmd = new MySqlCommand(query, connection);

        if (!string.IsNullOrEmpty(userName))
        {
            query += " WHERE client = @UserName";
        }

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
        //Récup user spécifique de la DB via requête
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

    private static User? GetUserByNameFromDatabase(MySqlConnection connection, string userName)
    {
        //Récup user spécifique de la DB via requête
        string query = "SELECT * FROM client WHERE nom = @Name";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Name", userName);

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
            //Ajout user dans la DB
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
        //Ajout user dans la DB via requête
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
        //Modifier user de la DB
        int userId = GetUserIdFromEndpoint(endpoint);
        string userName = GetUserNameFromEndpoint(endpoint);
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
        else if (userName != null)
        {
            User? existingUser = GetUserByNameFromDatabase(connection, userName);
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
            SendResponse(context, HttpStatusCode.BadRequest, "No user Name or ID");
        }
    }

    private static void UpdateUserInDatabase(MySqlConnection connection, User existingUser, User updatedUser)
    {
        //Modifier user de la DB via requête
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
        //Delete user de la DB
        int userId = GetUserIdFromEndpoint(endpoint);
        string userName = GetUserNameFromEndpoint(endpoint);
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
        else if (userId != null)
        {
            User? existingUser = GetUserByNameFromDatabase(connection, userName);
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
            SendResponse(context, HttpStatusCode.BadRequest, "No user name or ID");
        }
    }

    private static void RemoveUserFromDatabase(MySqlConnection connection, User existingUser)
    {
        //Delete user de la DB via requête
        string query = "DELETE FROM client WHERE id_client = @Id";
        using MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Id", existingUser.Id);

        cmd.ExecuteNonQuery();
    }

//    private static string GetUsersJson()
//    {
//        return "";
//    }
//
    private static string GetUserJson(User user)
    {
        //Conversion data User -> JSON
        return JsonSerializer.Serialize(user);
    }

    private static User? ParseUserFromBody(HttpListenerRequest request)
    {
        //Conversion Json -> data User
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
        //Ciblage ID par l'url (endpoint)
        string[] segments = endpoint.Split('/');
        if (segments.Length >= 3 && int.TryParse(segments[3], out int userId))
        {
            return userId;
        }
        return -1;
    }

    private static string GetUserNameFromEndpoint(string endpoint)
{
    //Ciblage du nom par l'url (endpoint)
    string[] segments = endpoint.Split('/');
    if (segments.Length >= 4)
    {
        return segments[3];
    }
    return null;
}


}
