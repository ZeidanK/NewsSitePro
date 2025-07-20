using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using NewsSite.BL;
using System.ComponentModel.Design;

/// <summary>
/// DBServices is a class created by me to provides some DataBase Services
/// </summary>
public class DBservices
{
    private string connectionString;

    public DBservices()
    {
        //
        // TODO: Add constructor logic here
        //
    }


    //---------------------------------------------------------------------------------
    // Create the SqlCommand
    //---------------------------------------------------------------------------------
    private SqlCommand CreateCommandWithStoredProcedureGeneral(String spName, SqlConnection con, Dictionary<string, object> paramDic)
    {

        SqlCommand cmd = new SqlCommand(); // create the command object

        cmd.Connection = con;              // assign the connection to the command object

        cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

        cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

        cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

        if (paramDic != null)
            foreach (KeyValuePair<string, object> param in paramDic)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);

            }


        return cmd;
    }
    //--------------------------------------------------------------------------------------------------
    // This method creates a connection to the database according to the connectionString name in the appsettings.json 
    //--------------------------------------------------------------------------------------------------
    public SqlConnection connect(String conString)
    {

        // read the connection string from the configuration file
        IConfigurationRoot configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json").Build();
        string cStr = configuration.GetConnectionString("myProjDB");
        SqlConnection con = new SqlConnection(cStr);
        con.Open();
        return con;
    }


    //--------------------------------------------------------------------------------------------------  
    // This method retrieves user information based on email, id, or username  
    //--------------------------------------------------------------------------------------------------  
    // public User GetUser(string email = null, int? id = null, string name = null)
    // {
    //     SqlConnection con = null;
    //     SqlCommand cmd = null;
    //     SqlDataReader reader = null;
    //     User user = null;

    //     try
    //     {
    //         con = connect("myProjDB"); // create the connection  

    //         var paramDic = new Dictionary<string, object>
    //         {
    //             { "@id", id.HasValue ? id.Value : DBNull.Value },
    //             { "@name", string.IsNullOrEmpty(name) ? DBNull.Value : name },
    //             { "@Email", string.IsNullOrEmpty(email) ? DBNull.Value : email }
    //         };

    //         cmd = CreateCommandWithStoredProcedureGeneral("sp_Users2025Pro_Get", con, paramDic); // create the command  

    //         reader = cmd.ExecuteReader();

    //         if (reader.Read())
    //         {
    //             user = new User
    //             {
    //                 // Map to your User class properties
    //                 Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
    //                 Name = reader["name"]?.ToString(),
    //                 Email = reader["email"]?.ToString(),
    //                 PasswordHash = reader["passwordHash"]?.ToString(),
    //                 IsAdmin = reader["isAdmin"] != DBNull.Value && Convert.ToBoolean(reader["isAdmin"]),
    //                 IsLocked = reader["isLocked"] != DBNull.Value && Convert.ToBoolean(reader["isLocked"])
    //             };
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         // Optionally log the exception here
    //         throw;
    //     }
    //     finally
    //     {
    //         reader?.Close();
    //         con?.Close();
    //     }

    //     return user;
    // }


    //--------------------------------------------------------------------------------------------------
    // This method creates a new user in the database using a stored procedure
    //--------------------------------------------------------------------------------------------------
    // public bool CreateUser(User user)
    // {
    //     SqlConnection con = null;
    //     SqlCommand cmd = null;
    //     try
    //     {
    //         con = connect("myProjDB");
    //         var paramDic = new Dictionary<string, object>
    //     {
    //         { "@name", user.Name },
    //         { "@Email", user.Email },
    //         { "@passwordHash", user.PasswordHash },
    //         { "@isAdmin", user.IsAdmin },
    //         { "@isLocked", user.IsLocked }
    //     };

    //         cmd = CreateCommandWithStoredProcedureGeneral("sp_Users2025Pro_Insert", con, paramDic);
    //         int affectedRows = cmd.ExecuteNonQuery();
    //         return affectedRows > 0;
    //     }
    //     catch (Exception)
    //     {
    //         throw;
    //     }
    //     finally
    //     {
    //         con?.Close();
    //     }
    // }

    //--------------------------------------------------------------------------------------------------
    // This method retrieves a user by their ID using a stored procedure
    //--------------------------------------------------------------------------------------------------
    public User GetUserById(int id)
    {
        return GetUser(null, id, null);
    }

    //--------------------------------------------------------------------------------------------------
    // This method updates user details in the database using a stored procedure
    //--------------------------------------------------------------------------------------------------
    // public bool UpdateUser(User user)
    // {
    //     SqlConnection con = null;
    //     SqlCommand cmd = null;
    //     try
    //     {
    //         con = connect("myProjDB");
    //         var paramDic = new Dictionary<string, object>
    //     {
    //         { "@id", user.Id },
    //         { "@name", user.Name },
    //         { "@passwordHash", user.PasswordHash },
    //         { "@isAdmin", user.IsAdmin },
    //         { "@isLocked", user.IsLocked }
    //     };

    //         cmd = CreateCommandWithStoredProcedureGeneral("sp_Users2025Pro_Update", con, paramDic);
    //         int affectedRows = cmd.ExecuteNonQuery();
    //         return affectedRows > 0;
    //     }
    //     catch (Exception)
    //     {
    //         throw;
    //     }
    //     finally
    //     {
    //         con?.Close();
    //     }
    // }
    


    
    //public User GetUser(string username=null,string email=null)





    public User GetUser(string email = null, int? id = null, string name = null)
    {
        SqlConnection con = null;
        SqlCommand cmd = null;
        SqlDataReader reader = null;
        User user = null;

        try
        {
            con = connect("myProjDB"); // create the connection  

            var paramDic = new Dictionary<string, object>
        {
            { "@UserID", id.HasValue ? id.Value : DBNull.Value },
            { "@Username", string.IsNullOrEmpty(name) ? DBNull.Value : name },
            { "@Email", string.IsNullOrEmpty(email) ? DBNull.Value : email }
        };

            cmd = CreateCommandWithStoredProcedureGeneral("sp_Users_News_Get", con, paramDic); // create the command  

            reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                user = new User
                {
                    Id = reader["UserID"] != DBNull.Value ? Convert.ToInt32(reader["UserID"]) : 0,
                    Name = reader["Username"]?.ToString(),
                    Email = reader["Email"]?.ToString(),
                    PasswordHash = reader["PasswordHash"]?.ToString(),
                    IsAdmin = reader["IsAdmin"] != DBNull.Value && Convert.ToBoolean(reader["IsAdmin"]),
                    IsLocked = reader["IsLocked"] != DBNull.Value && Convert.ToBoolean(reader["IsLocked"]),
                    Bio = reader["Bio"]?.ToString(),
                    JoinDate = reader["JoinDate"] != DBNull.Value ? Convert.ToDateTime(reader["JoinDate"]) : DateTime.Now
                };
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            reader?.Close();
            con?.Close();
        }

        return user;
    }

public bool CreateUser(User user)
{
    SqlConnection con = null;
    SqlCommand cmd = null;
    try
    {
        con = connect("myProjDB");
        var paramDic = new Dictionary<string, object>
        {
            { "@Username", user.Name },
            { "@Email", user.Email },
            { "@PasswordHash", user.PasswordHash },
            { "@IsAdmin", user.IsAdmin },
            { "@IsLocked", user.IsLocked }
        };

        cmd = CreateCommandWithStoredProcedureGeneral("sp_Users_News_Insert", con, paramDic);
        int affectedRows = cmd.ExecuteNonQuery();
        return affectedRows > 0;
    }
    catch (Exception)
    {
        throw;
    }
    finally
    {
        con?.Close();
    }
}

public bool UpdateUser(User user)
{
    SqlConnection con = null;
    SqlCommand cmd = null;
    try
    {
        con = connect("myProjDB");
        var paramDic = new Dictionary<string, object>
        {
            { "@UserID", user.Id },
            { "@Username", user.Name },
            { "@PasswordHash", user.PasswordHash },
            { "@IsAdmin", user.IsAdmin },
            { "@IsLocked", user.IsLocked }
        };

        cmd = CreateCommandWithStoredProcedureGeneral("sp_Users_News_Update", con, paramDic);
        int affectedRows = cmd.ExecuteNonQuery();
        return affectedRows > 0;
    }
    catch (Exception)
    {
        throw;
    }
    finally
    {
        con?.Close();
    }
}


    //--------------------------------------------------------------------------------------------------
    // News Articles Database Methods
    //--------------------------------------------------------------------------------------------------
    
    public List<NewsArticle> GetAllNewsArticles(int pageNumber = 1, int pageSize = 10, string? category = null, int? currentUserId = null)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;
        List<NewsArticle> articles = new List<NewsArticle>();

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@PageNumber", pageNumber },
                { "@PageSize", pageSize },
                { "@Category", (object?)category ?? DBNull.Value }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("sp_NewsArticles_GetAll", con, paramDic);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var article = new NewsArticle
                {
                    ArticleID = Convert.ToInt32(reader["ArticleID"]),
                    Title = reader["Title"]?.ToString(),
                    Content = reader["Content"]?.ToString(),
                    ImageURL = reader["ImageURL"]?.ToString(),
                    SourceURL = reader["SourceURL"]?.ToString(),
                    SourceName = reader["SourceName"]?.ToString(),
                    Category = reader["Category"]?.ToString(),
                    PublishDate = Convert.ToDateTime(reader["PublishDate"]),
                    UserID = Convert.ToInt32(reader["UserID"]),
                    Username = reader["Username"]?.ToString(),
                    LikesCount = Convert.ToInt32(reader["LikesCount"]),
                    ViewsCount = Convert.ToInt32(reader["ViewsCount"])
                };

                // Check if current user liked/saved this article
                if (currentUserId.HasValue)
                {
                    article.IsLiked = CheckUserLikedArticle(article.ArticleID, currentUserId.Value);
                    article.IsSaved = CheckUserSavedArticle(article.ArticleID, currentUserId.Value);
                }

                articles.Add(article);
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            reader?.Close();
            con?.Close();
        }

        return articles;
    }

    public int CreateNewsArticle(NewsArticle article)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@Title", (object?)article.Title ?? DBNull.Value },
                { "@Content", (object?)article.Content ?? DBNull.Value },
                { "@ImageURL", (object?)article.ImageURL ?? DBNull.Value },
                { "@SourceURL", (object?)article.SourceURL ?? DBNull.Value },
                { "@SourceName", (object?)article.SourceName ?? DBNull.Value },
                { "@Category", (object?)article.Category ?? DBNull.Value },
                { "@UserID", article.UserID }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("sp_NewsArticles_Insert", con, paramDic);
            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            con?.Close();
        }
    }

    public List<NewsArticle> GetArticlesByUser(int userId, int pageNumber = 1, int pageSize = 10)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;
        List<NewsArticle> articles = new List<NewsArticle>();

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@UserID", userId },
                { "@PageNumber", pageNumber },
                { "@PageSize", pageSize }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("sp_NewsArticles_GetByUser", con, paramDic);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                articles.Add(new NewsArticle
                {
                    ArticleID = Convert.ToInt32(reader["ArticleID"]),
                    Title = reader["Title"]?.ToString(),
                    Content = reader["Content"]?.ToString(),
                    ImageURL = reader["ImageURL"]?.ToString(),
                    SourceURL = reader["SourceURL"]?.ToString(),
                    SourceName = reader["SourceName"]?.ToString(),
                    Category = reader["Category"]?.ToString(),
                    PublishDate = Convert.ToDateTime(reader["PublishDate"]),
                    LikesCount = Convert.ToInt32(reader["LikesCount"]),
                    ViewsCount = Convert.ToInt32(reader["ViewsCount"])
                });
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            reader?.Close();
            con?.Close();
        }

        return articles;
    }

    public string ToggleArticleLike(int articleId, int userId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@ArticleID", articleId },
                { "@UserID", userId }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("sp_ArticleLikes_Toggle", con, paramDic);
            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? "error";
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            con?.Close();
        }
    }

    public string ToggleSaveArticle(int articleId, int userId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@ArticleID", articleId },
                { "@UserID", userId }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("sp_SavedArticles_Toggle", con, paramDic);
            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? "error";
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            con?.Close();
        }
    }

    public bool RecordArticleView(int articleId, int? userId = null)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@ArticleID", articleId },
                { "@UserID", (object?)userId ?? DBNull.Value }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("sp_ArticleViews_Insert", con, paramDic);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            con?.Close();
        }
    }

    public bool ReportArticle(int articleId, int userId, string? reason = null)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@ArticleID", articleId },
                { "@UserID", userId },
                { "@Reason", (object?)reason ?? DBNull.Value }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("sp_Reports_Insert", con, paramDic);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            con?.Close();
        }
    }

    public UserActivity GetUserStats(int userId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@UserID", userId }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("sp_UserStats_Get", con, paramDic);
            reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new UserActivity
                {
                    PostsCount = Convert.ToInt32(reader["PostsCount"]),
                    LikesCount = Convert.ToInt32(reader["LikesCount"]),
                    SavedCount = Convert.ToInt32(reader["SavedCount"]),
                    FollowersCount = Convert.ToInt32(reader["FollowersCount"])
                };
            }

            return new UserActivity();
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            reader?.Close();
            con?.Close();
        }
    }

    public bool UpdateUserProfile(int userId, string username, string? bio = null)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@UserID", userId },
                { "@Username", username },
                { "@Bio", (object?)bio ?? DBNull.Value }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("sp_UserProfile_Update", con, paramDic);
            int affectedRows = cmd.ExecuteNonQuery();
            return affectedRows > 0;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            con?.Close();
        }
    }

    private bool CheckUserLikedArticle(int articleId, int userId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;

        try
        {
            con = connect("myProjDB");
            cmd = new SqlCommand("SELECT COUNT(*) FROM ArticleLikes WHERE ArticleID = @ArticleID AND UserID = @UserID", con);
            cmd.Parameters.AddWithValue("@ArticleID", articleId);
            cmd.Parameters.AddWithValue("@UserID", userId);
            
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            con?.Close();
        }
    }

    private bool CheckUserSavedArticle(int articleId, int userId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;

        try
        {
            con = connect("myProjDB");
            cmd = new SqlCommand("SELECT COUNT(*) FROM SavedArticles WHERE ArticleID = @ArticleID AND UserID = @UserID", con);
            cmd.Parameters.AddWithValue("@ArticleID", articleId);
            cmd.Parameters.AddWithValue("@UserID", userId);
            
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            con?.Close();
        }
    }

    // Admin-specific methods
    public async Task<AdminDashboardStats> GetAdminDashboardStats()
    {
        var stats = new AdminDashboardStats();
        
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");
            
            using (var con = new SqlConnection(cStr))
            {
                await con.OpenAsync();
                
                // Get total users
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users_News", con))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalUsers = result != null ? (int)result : 0;
                }
                
                // Get active users (not banned and active)
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users_News WHERE IsActive = 1 AND (IsBanned = 0 OR BannedUntil < GETDATE())", con))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.ActiveUsers = result != null ? (int)result : 0;
                }
                
                // Get banned users
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users_News WHERE IsBanned = 1 AND (BannedUntil IS NULL OR BannedUntil > GETDATE())", con))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.BannedUsers = result != null ? (int)result : 0;
                }
                
                // Get total posts
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM NewsArticles", con))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalPosts = result != null ? (int)result : 0;
                }
                
                // Get total reports
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Reports", con))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalReports = result != null ? (int)result : 0;
                }
                
                // Get pending reports
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Reports WHERE Status = 'Pending'", con))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.PendingReports = result != null ? (int)result : 0;
                }
                
                // Get today's registrations
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users_News WHERE CAST(JoinDate AS DATE) = CAST(GETDATE() AS DATE)", con))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TodayRegistrations = result != null ? (int)result : 0;
                }
                
                // Get today's posts
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM NewsArticles WHERE CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE)", con))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TodayPosts = result != null ? (int)result : 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting admin dashboard stats: " + ex.Message);
        }
        
        return stats;
    }

    public async Task<List<AdminUserView>> GetAllUsersForAdmin(int page, int pageSize)
    {
        var users = new List<AdminUserView>();
        
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");
            
            using (var con = new SqlConnection(cStr))
            {
                await con.OpenAsync();
                
                var offset = (page - 1) * pageSize;
                var query = @"
                    SELECT u.ID, u.Name AS Username, u.Email, u.Bio, u.JoinDate, u.LastActivity, 
                           u.IsAdmin, u.IsActive, u.IsBanned, u.BannedUntil,
                           COALESCE(pc.PostCount, 0) AS PostCount,
                           COALESCE(lc.LikesReceived, 0) AS LikesReceived
                    FROM Users_News u
                    LEFT JOIN (
                        SELECT UserID, COUNT(*) AS PostCount 
                        FROM NewsArticles 
                        GROUP BY UserID
                    ) pc ON u.ID = pc.UserID
                    LEFT JOIN (
                        SELECT na.UserID, COUNT(*) AS LikesReceived
                        FROM NewsArticles na
                        INNER JOIN ArticleLikes al ON na.ID = al.ArticleID
                        GROUP BY na.UserID
                    ) lc ON u.ID = lc.UserID
                    ORDER BY u.JoinDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Offset", offset);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var user = new AdminUserView
                            {
                                Id = reader.GetInt32("ID"),
                                Username = reader.GetString("Username"),
                                Email = reader.GetString("Email"),
                                JoinDate = reader.GetDateTime("JoinDate"),
                                LastActivity = reader.IsDBNull("LastActivity") ? null : reader.GetDateTime("LastActivity"),
                                PostCount = reader.GetInt32("PostCount"),
                                LikesReceived = reader.GetInt32("LikesReceived"),
                                IsAdmin = reader.GetBoolean("IsAdmin")
                            };
                            
                            // Determine status
                            var isActive = reader.GetBoolean("IsActive");
                            var isBanned = reader.GetBoolean("IsBanned");
                            var bannedUntil = reader.IsDBNull("BannedUntil") ? (DateTime?)null : reader.GetDateTime("BannedUntil");
                            
                            if (isBanned && (bannedUntil == null || bannedUntil > DateTime.Now))
                            {
                                user.Status = "Banned";
                            }
                            else if (!isActive)
                            {
                                user.Status = "Inactive";
                            }
                            else
                            {
                                user.Status = "Active";
                            }
                            
                            users.Add(user);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting users for admin: " + ex.Message);
        }
        
        return users;
    }

    public async Task<List<AdminUserView>> GetFilteredUsersForAdmin(int page, int pageSize, string search, string status, string joinDate)
    {
        var users = new List<AdminUserView>();
        
        try
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                
                var offset = (page - 1) * pageSize;
                var whereConditions = new List<string>();
                var parameters = new List<SqlParameter>();
                
                // Search filter
                if (!string.IsNullOrEmpty(search))
                {
                    whereConditions.Add("(u.Name LIKE @Search OR u.Email LIKE @Search)");
                    parameters.Add(new SqlParameter("@Search", $"%{search}%"));
                }
                
                // Status filter
                if (!string.IsNullOrEmpty(status))
                {
                    switch (status.ToLower())
                    {
                        case "active":
                            whereConditions.Add("u.IsActive = 1 AND (u.IsBanned = 0 OR u.BannedUntil < GETDATE())");
                            break;
                        case "banned":
                            whereConditions.Add("u.IsBanned = 1 AND (u.BannedUntil IS NULL OR u.BannedUntil > GETDATE())");
                            break;
                        case "inactive":
                            whereConditions.Add("u.IsActive = 0");
                            break;
                    }
                }
                
                // Join date filter
                if (!string.IsNullOrEmpty(joinDate))
                {
                    switch (joinDate.ToLower())
                    {
                        case "today":
                            whereConditions.Add("CAST(u.JoinDate AS DATE) = CAST(GETDATE() AS DATE)");
                            break;
                        case "week":
                            whereConditions.Add("u.JoinDate >= DATEADD(week, -1, GETDATE())");
                            break;
                        case "month":
                            whereConditions.Add("u.JoinDate >= DATEADD(month, -1, GETDATE())");
                            break;
                        case "year":
                            whereConditions.Add("u.JoinDate >= DATEADD(year, -1, GETDATE())");
                            break;
                    }
                }
                
                var whereClause = whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";
                
                var query = $@"
                    SELECT u.ID, u.Name AS Username, u.Email, u.Bio, u.JoinDate, u.LastActivity, 
                           u.IsAdmin, u.IsActive, u.IsBanned, u.BannedUntil,
                           COALESCE(pc.PostCount, 0) AS PostCount,
                           COALESCE(lc.LikesReceived, 0) AS LikesReceived
                    FROM Users_News u
                    LEFT JOIN (
                        SELECT UserID, COUNT(*) AS PostCount 
                        FROM NewsArticles 
                        GROUP BY UserID
                    ) pc ON u.ID = pc.UserID
                    LEFT JOIN (
                        SELECT na.UserID, COUNT(*) AS LikesReceived
                        FROM NewsArticles na
                        INNER JOIN ArticleLikes al ON na.ID = al.ArticleID
                        GROUP BY na.UserID
                    ) lc ON u.ID = lc.UserID
                    {whereClause}
                    ORDER BY u.JoinDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Offset", offset);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);
                    
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.Add(param);
                    }
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var user = new AdminUserView
                            {
                                Id = reader.GetInt32("ID"),
                                Username = reader.GetString("Username"),
                                Email = reader.GetString("Email"),
                                JoinDate = reader.GetDateTime("JoinDate"),
                                LastActivity = reader.IsDBNull("LastActivity") ? null : reader.GetDateTime("LastActivity"),
                                PostCount = reader.GetInt32("PostCount"),
                                LikesReceived = reader.GetInt32("LikesReceived"),
                                IsAdmin = reader.GetBoolean("IsAdmin")
                            };
                            
                            // Determine status
                            var isActive = reader.GetBoolean("IsActive");
                            var isBanned = reader.GetBoolean("IsBanned");
                            var bannedUntil = reader.IsDBNull("BannedUntil") ? (DateTime?)null : reader.GetDateTime("BannedUntil");
                            
                            if (isBanned && (bannedUntil == null || bannedUntil > DateTime.Now))
                            {
                                user.Status = "Banned";
                            }
                            else if (!isActive)
                            {
                                user.Status = "Inactive";
                            }
                            else
                            {
                                user.Status = "Active";
                            }
                            
                            users.Add(user);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting filtered users for admin: " + ex.Message);
        }
        
        return users;
    }

    public async Task<int> GetFilteredUsersCount(string search, string status, string joinDate)
    {
        try
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                
                var whereConditions = new List<string>();
                var parameters = new List<SqlParameter>();
                
                // Search filter
                if (!string.IsNullOrEmpty(search))
                {
                    whereConditions.Add("(Name LIKE @Search OR Email LIKE @Search)");
                    parameters.Add(new SqlParameter("@Search", $"%{search}%"));
                }
                
                // Status filter
                if (!string.IsNullOrEmpty(status))
                {
                    switch (status.ToLower())
                    {
                        case "active":
                            whereConditions.Add("IsActive = 1 AND (IsBanned = 0 OR BannedUntil < GETDATE())");
                            break;
                        case "banned":
                            whereConditions.Add("IsBanned = 1 AND (BannedUntil IS NULL OR BannedUntil > GETDATE())");
                            break;
                        case "inactive":
                            whereConditions.Add("IsActive = 0");
                            break;
                    }
                }
                
                // Join date filter
                if (!string.IsNullOrEmpty(joinDate))
                {
                    switch (joinDate.ToLower())
                    {
                        case "today":
                            whereConditions.Add("CAST(JoinDate AS DATE) = CAST(GETDATE() AS DATE)");
                            break;
                        case "week":
                            whereConditions.Add("JoinDate >= DATEADD(week, -1, GETDATE())");
                            break;
                        case "month":
                            whereConditions.Add("JoinDate >= DATEADD(month, -1, GETDATE())");
                            break;
                        case "year":
                            whereConditions.Add("JoinDate >= DATEADD(year, -1, GETDATE())");
                            break;
                    }
                }
                
                var whereClause = whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";
                var query = $"SELECT COUNT(*) FROM Users_News {whereClause}";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.Add(param);
                    }
                    
                    return (int)await cmd.ExecuteScalarAsync();
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting filtered users count: " + ex.Message);
        }
    }

    public async Task<AdminUserDetails> GetUserDetailsForAdmin(int userId)
    {
        try
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                
                var query = @"
                    SELECT u.ID, u.Name AS Username, u.Email, u.Bio, u.JoinDate, u.LastActivity, 
                           u.IsAdmin, u.IsActive, u.IsBanned, u.BannedUntil, u.BanReason,
                           COALESCE(pc.PostCount, 0) AS PostCount,
                           COALESCE(lc.LikesReceived, 0) AS LikesReceived
                    FROM Users_News u
                    LEFT JOIN (
                        SELECT UserID, COUNT(*) AS PostCount 
                        FROM NewsArticles 
                        WHERE UserID = @UserId
                        GROUP BY UserID
                    ) pc ON u.ID = pc.UserID
                    LEFT JOIN (
                        SELECT na.UserID, COUNT(*) AS LikesReceived
                        FROM NewsArticles na
                        INNER JOIN ArticleLikes al ON na.ID = al.ArticleID
                        WHERE na.UserID = @UserId
                        GROUP BY na.UserID
                    ) lc ON u.ID = lc.UserID
                    WHERE u.ID = @UserId";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var user = new AdminUserDetails
                            {
                                Id = reader.GetInt32("ID"),
                                Username = reader.GetString("Username"),
                                Email = reader.GetString("Email"),
                                Bio = reader.IsDBNull("Bio") ? null : reader.GetString("Bio"),
                                JoinDate = reader.GetDateTime("JoinDate"),
                                LastActivity = reader.IsDBNull("LastActivity") ? null : reader.GetDateTime("LastActivity"),
                                PostCount = reader.GetInt32("PostCount"),
                                LikesReceived = reader.GetInt32("LikesReceived"),
                                IsAdmin = reader.GetBoolean("IsAdmin"),
                                BannedUntil = reader.IsDBNull("BannedUntil") ? null : reader.GetDateTime("BannedUntil"),
                                BanReason = reader.IsDBNull("BanReason") ? null : reader.GetString("BanReason")
                            };
                            
                            // Determine status
                            var isActive = reader.GetBoolean("IsActive");
                            var isBanned = reader.GetBoolean("IsBanned");
                            var bannedUntil = reader.IsDBNull("BannedUntil") ? (DateTime?)null : reader.GetDateTime("BannedUntil");
                            
                            if (isBanned && (bannedUntil == null || bannedUntil > DateTime.Now))
                            {
                                user.Status = "Banned";
                            }
                            else if (!isActive)
                            {
                                user.Status = "Inactive";
                            }
                            else
                            {
                                user.Status = "Active";
                            }
                            
                            return user;
                        }
                    }
                }
            }
            
            throw new Exception("User not found");
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting user details for admin: " + ex.Message);
        }
    }

    public async Task<bool> BanUser(int userId, string reason, int durationDays, int adminId)
    {
        try
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                
                var bannedUntil = durationDays == -1 ? (DateTime?)null : DateTime.Now.AddDays(durationDays);
                
                var query = @"
                    UPDATE Users_News 
                    SET IsBanned = 1, BannedUntil = @BannedUntil, BanReason = @Reason
                    WHERE ID = @UserId";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@BannedUntil", (object?)bannedUntil ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Reason", reason);
                    
                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error banning user: " + ex.Message);
        }
    }

    public async Task<bool> UnbanUser(int userId)
    {
        try
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                
                var query = @"
                    UPDATE Users_News 
                    SET IsBanned = 0, BannedUntil = NULL, BanReason = NULL
                    WHERE ID = @UserId";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    
                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error unbanning user: " + ex.Message);
        }
    }

    public async Task<bool> DeactivateUser(int userId)
    {
        try
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                
                var query = "UPDATE Users_News SET IsActive = 0 WHERE ID = @UserId";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    
                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error deactivating user: " + ex.Message);
        }
    }

    public async Task<List<ActivityLog>> GetRecentActivityLogs(int count)
    {
        var logs = new List<ActivityLog>();
        
        try
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                
                var query = @"
                    SELECT TOP (@Count) al.ID, al.UserID, u.Name AS Username, al.Action, 
                           al.Details, al.Timestamp, al.IpAddress, al.UserAgent
                    FROM ActivityLogs al
                    INNER JOIN Users_News u ON al.UserID = u.ID
                    ORDER BY al.Timestamp DESC";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Count", count);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            logs.Add(new ActivityLog
                            {
                                Id = reader.GetInt32("ID"),
                                UserId = reader.GetInt32("UserID"),
                                Username = reader.GetString("Username"),
                                Action = reader.GetString("Action"),
                                Details = reader.GetString("Details"),
                                Timestamp = reader.GetDateTime("Timestamp"),
                                IpAddress = reader.GetString("IpAddress"),
                                UserAgent = reader.GetString("UserAgent")
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting recent activity logs: " + ex.Message);
        }
        
        return logs;
    }

    public async Task<List<ActivityLog>> GetActivityLogs(int page, int pageSize)
    {
        var logs = new List<ActivityLog>();
        
        try
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                
                var offset = (page - 1) * pageSize;
                var query = @"
                    SELECT al.ID, al.UserID, u.Name AS Username, al.Action, 
                           al.Details, al.Timestamp, al.IpAddress, al.UserAgent
                    FROM ActivityLogs al
                    INNER JOIN Users_News u ON al.UserID = u.ID
                    ORDER BY al.Timestamp DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Offset", offset);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            logs.Add(new ActivityLog
                            {
                                Id = reader.GetInt32("ID"),
                                UserId = reader.GetInt32("UserID"),
                                Username = reader.GetString("Username"),
                                Action = reader.GetString("Action"),
                                Details = reader.GetString("Details"),
                                Timestamp = reader.GetDateTime("Timestamp"),
                                IpAddress = reader.GetString("IpAddress"),
                                UserAgent = reader.GetString("UserAgent")
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting activity logs: " + ex.Message);
        }
        
        return logs;
    }

    public async Task<List<UserReport>> GetPendingReports()
    {
        var reports = new List<UserReport>();
        
        try
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                
                var query = @"
                    SELECT r.ID, r.ReporterID, ru.Name AS ReporterUsername, 
                           r.ReportedUserID, rpu.Name AS ReportedUsername,
                           r.Reason, r.Description, r.CreatedAt, r.Status
                    FROM Reports r
                    INNER JOIN Users_News ru ON r.ReporterID = ru.ID
                    INNER JOIN Users_News rpu ON r.ReportedUserID = rpu.ID
                    WHERE r.Status = 'Pending'
                    ORDER BY r.CreatedAt DESC";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            reports.Add(new UserReport
                            {
                                Id = reader.GetInt32("ID"),
                                ReporterId = reader.GetInt32("ReporterID"),
                                ReporterUsername = reader.GetString("ReporterUsername"),
                                ReportedUserId = reader.GetInt32("ReportedUserID"),
                                ReportedUsername = reader.GetString("ReportedUsername"),
                                Reason = reader.GetString("Reason"),
                                Description = reader.GetString("Description"),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                Status = reader.GetString("Status")
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting pending reports: " + ex.Message);
        }
        
        return reports;
    }

    public async Task<List<UserReport>> GetAllReports()
    {
        var reports = new List<UserReport>();
        
        try
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                
                var query = @"
                    SELECT r.ID, r.ReporterID, ru.Name AS ReporterUsername, 
                           r.ReportedUserID, rpu.Name AS ReportedUsername,
                           r.Reason, r.Description, r.CreatedAt, r.Status,
                           r.ResolvedBy, r.ResolvedAt, r.ResolutionNotes
                    FROM Reports r
                    INNER JOIN Users_News ru ON r.ReporterID = ru.ID
                    INNER JOIN Users_News rpu ON r.ReportedUserID = rpu.ID
                    ORDER BY r.CreatedAt DESC";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            reports.Add(new UserReport
                            {
                                Id = reader.GetInt32("ID"),
                                ReporterId = reader.GetInt32("ReporterID"),
                                ReporterUsername = reader.GetString("ReporterUsername"),
                                ReportedUserId = reader.GetInt32("ReportedUserID"),
                                ReportedUsername = reader.GetString("ReportedUsername"),
                                Reason = reader.GetString("Reason"),
                                Description = reader.GetString("Description"),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                Status = reader.GetString("Status"),
                                ResolvedBy = reader.IsDBNull("ResolvedBy") ? null : reader.GetInt32("ResolvedBy"),
                                ResolvedAt = reader.IsDBNull("ResolvedAt") ? null : reader.GetDateTime("ResolvedAt"),
                                ResolutionNotes = reader.IsDBNull("ResolutionNotes") ? null : reader.GetString("ResolutionNotes")
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting all reports: " + ex.Message);
        }
        
        return reports;
    }

    public async Task<bool> ResolveReport(int reportId, string action, string notes, int adminId)
    {
        try
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                
                var query = @"
                    UPDATE Reports 
                    SET Status = @Status, ResolvedBy = @AdminId, ResolvedAt = GETDATE(), ResolutionNotes = @Notes
                    WHERE ID = @ReportId";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ReportId", reportId);
                    cmd.Parameters.AddWithValue("@Status", action);
                    cmd.Parameters.AddWithValue("@AdminId", adminId);
                    cmd.Parameters.AddWithValue("@Notes", notes);
                    
                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error resolving report: " + ex.Message);
        }
    }

    public async Task<bool> LogAdminAction(int adminId, string action, string details)
    {
        try
        {
            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                
                var query = @"
                    INSERT INTO ActivityLogs (UserID, Action, Details, Timestamp, IpAddress, UserAgent)
                    VALUES (@AdminId, @Action, @Details, GETDATE(), 'Admin Panel', 'Admin Action')";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@AdminId", adminId);
                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@Details", details);
                    
                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error logging admin action: " + ex.Message);
        }
    }

    // Notification methods
    public async Task<List<Notification>> GetUserNotifications(int userId, int page, int pageSize)
    {
        var notifications = new List<Notification>();
        
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");
            
            using (var con = new SqlConnection(cStr))
            {
                await con.OpenAsync();
                
                var offset = (page - 1) * pageSize;
                var query = @"
                    SELECT n.ID, n.UserID, n.Type, n.Title, n.Message, n.RelatedEntityType, 
                           n.RelatedEntityID, n.IsRead, n.CreatedAt, n.FromUserID, n.ActionUrl,
                           u.Name AS FromUserName
                    FROM Notifications n
                    LEFT JOIN Users_News u ON n.FromUserID = u.ID
                    WHERE n.UserID = @UserID
                    ORDER BY n.CreatedAt DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@Offset", offset);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            notifications.Add(new Notification
                            {
                                ID = reader.GetInt32("ID"),
                                UserID = reader.GetInt32("UserID"),
                                Type = reader.GetString("Type"),
                                Title = reader.GetString("Title"),
                                Message = reader.GetString("Message"),
                                RelatedEntityType = reader.IsDBNull("RelatedEntityType") ? null : reader.GetString("RelatedEntityType"),
                                RelatedEntityID = reader.IsDBNull("RelatedEntityID") ? null : reader.GetInt32("RelatedEntityID"),
                                IsRead = reader.GetBoolean("IsRead"),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                FromUserID = reader.IsDBNull("FromUserID") ? null : reader.GetInt32("FromUserID"),
                                FromUserName = reader.IsDBNull("FromUserName") ? null : reader.GetString("FromUserName"),
                                ActionUrl = reader.IsDBNull("ActionUrl") ? null : reader.GetString("ActionUrl")
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting user notifications: " + ex.Message);
        }
        
        return notifications;
    }

    public async Task<NotificationSummary> GetNotificationSummary(int userId)
    {
        var summary = new NotificationSummary();
        
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");
            
            using (var con = new SqlConnection(cStr))
            {
                await con.OpenAsync();
                
                // Get total unread count
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Notifications WHERE UserID = @UserID AND IsRead = 0", con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    var result = await cmd.ExecuteScalarAsync();
                    summary.TotalUnread = result != null ? (int)result : 0;
                }
                
                // Get unread count by type
                var typeQuery = @"
                    SELECT Type, COUNT(*) AS Count
                    FROM Notifications 
                    WHERE UserID = @UserID AND IsRead = 0
                    GROUP BY Type";
                
                using (var cmd = new SqlCommand(typeQuery, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            summary.UnreadByType[reader.GetString("Type")] = reader.GetInt32("Count");
                        }
                    }
                }
                
                // Get recent notifications
                summary.RecentNotifications = await GetUserNotifications(userId, 1, 5);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting notification summary: " + ex.Message);
        }
        
        return summary;
    }

    public async Task<int> GetUnreadNotificationCount(int userId)
    {
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");
            
            using (var con = new SqlConnection(cStr))
            {
                await con.OpenAsync();
                
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Notifications WHERE UserID = @UserID AND IsRead = 0", con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? (int)result : 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting unread notification count: " + ex.Message);
        }
    }

    public async Task<bool> CreateNotification(CreateNotificationRequest request)
    {
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");
            
            using (var con = new SqlConnection(cStr))
            {
                await con.OpenAsync();
                
                var query = @"
                    INSERT INTO Notifications (UserID, Type, Title, Message, RelatedEntityType, RelatedEntityID, FromUserID, ActionUrl, CreatedAt)
                    VALUES (@UserID, @Type, @Title, @Message, @RelatedEntityType, @RelatedEntityID, @FromUserID, @ActionUrl, GETDATE())";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", request.UserID);
                    cmd.Parameters.AddWithValue("@Type", request.Type);
                    cmd.Parameters.AddWithValue("@Title", request.Title);
                    cmd.Parameters.AddWithValue("@Message", request.Message);
                    cmd.Parameters.AddWithValue("@RelatedEntityType", (object?)request.RelatedEntityType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RelatedEntityID", (object?)request.RelatedEntityID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromUserID", (object?)request.FromUserID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionUrl", (object?)request.ActionUrl ?? DBNull.Value);
                    
                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error creating notification: " + ex.Message);
        }
    }

    public async Task<bool> MarkNotificationAsRead(int notificationId, int userId)
    {
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");
            
            using (var con = new SqlConnection(cStr))
            {
                await con.OpenAsync();
                
                var query = "UPDATE Notifications SET IsRead = 1 WHERE ID = @ID AND UserID = @UserID";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ID", notificationId);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    
                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error marking notification as read: " + ex.Message);
        }
    }

    public async Task<bool> MarkAllNotificationsAsRead(int userId)
    {
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");
            
            using (var con = new SqlConnection(cStr))
            {
                await con.OpenAsync();
                
                var query = "UPDATE Notifications SET IsRead = 1 WHERE UserID = @UserID AND IsRead = 0";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    
                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error marking all notifications as read: " + ex.Message);
        }
    }

    public async Task<bool> MarkNotificationsAsRead(int userId)
    {
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");
            
            using (var con = new SqlConnection(cStr))
            {
                await con.OpenAsync();
                
                var query = "UPDATE Notifications SET IsRead = 1 WHERE UserID = @UserID AND IsRead = 0";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    
                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error marking notifications as read: " + ex.Message);
        }
    }

    public async Task<Dictionary<string, NotificationPreferenceSettings>> GetUserNotificationPreferences(int userId)
    {
        var preferences = new Dictionary<string, NotificationPreferenceSettings>();
        
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");
            
            using (var con = new SqlConnection(cStr))
            {
                await con.OpenAsync();
                
                var query = @"
                    SELECT NotificationType, IsEnabled, EmailNotification, PushNotification
                    FROM NotificationPreferences 
                    WHERE UserID = @UserID";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            preferences[reader.GetString("NotificationType")] = new NotificationPreferenceSettings
                            {
                                IsEnabled = reader.GetBoolean("IsEnabled"),
                                EmailNotification = reader.GetBoolean("EmailNotification"),
                                PushNotification = reader.GetBoolean("PushNotification")
                            };
                        }
                    }
                }
                
                // Add default preferences for types not in database
                var defaultTypes = new[] { "Like", "Comment", "Follow", "NewPost", "PostShare", "AdminMessage" };
                foreach (var type in defaultTypes)
                {
                    if (!preferences.ContainsKey(type))
                    {
                        preferences[type] = new NotificationPreferenceSettings
                        {
                            IsEnabled = true,
                            EmailNotification = false,
                            PushNotification = true
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting notification preferences: " + ex.Message);
        }
        
        return preferences;
    }

    public async Task<bool> UpdateNotificationPreferences(int userId, Dictionary<string, NotificationPreferenceSettings> preferences)
    {
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");
            
            using (var con = new SqlConnection(cStr))
            {
                await con.OpenAsync();
                
                foreach (var pref in preferences)
                {
                    var query = @"
                        MERGE NotificationPreferences AS target
                        USING (VALUES (@UserID, @NotificationType, @IsEnabled, @EmailNotification, @PushNotification)) 
                        AS source (UserID, NotificationType, IsEnabled, EmailNotification, PushNotification)
                        ON target.UserID = source.UserID AND target.NotificationType = source.NotificationType
                        WHEN MATCHED THEN
                            UPDATE SET IsEnabled = source.IsEnabled, EmailNotification = source.EmailNotification, PushNotification = source.PushNotification
                        WHEN NOT MATCHED THEN
                            INSERT (UserID, NotificationType, IsEnabled, EmailNotification, PushNotification)
                            VALUES (source.UserID, source.NotificationType, source.IsEnabled, source.EmailNotification, source.PushNotification);";
                    
                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@NotificationType", pref.Key);
                        cmd.Parameters.AddWithValue("@IsEnabled", pref.Value.IsEnabled);
                        cmd.Parameters.AddWithValue("@EmailNotification", pref.Value.EmailNotification);
                        cmd.Parameters.AddWithValue("@PushNotification", pref.Value.PushNotification);
                        
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                
                return true;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error updating notification preferences: " + ex.Message);
        }
    }

    // Compatibility method for NotificationsModel
    public async Task<Dictionary<string, NotificationPreferenceSettings>> GetUserNotificationPreferencesDictionary(int userId)
    {
        return await GetUserNotificationPreferences(userId);
    }


}