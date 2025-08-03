using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using NewsSite.BL;
using System.ComponentModel.Design;
using Microsoft.Extensions.Configuration;

/// <summary>
/// DBServices is a class created by me to provides some DataBase Services
/// </summary>
public class DBservices
{
    private string connectionString;

    public DBservices()
    {
        // Initialize connection string from configuration
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json").Build();
        connectionString = configuration.GetConnectionString("myProjDB");
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





    // Get user by id, username, or email
    public User GetUser(string? email = null, int? id = null, string? name = null)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;
        User? user = null;

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@UserID", id.HasValue ? id.Value : DBNull.Value },
                { "@Username", string.IsNullOrEmpty(name) ? DBNull.Value : name },
                { "@Email", string.IsNullOrEmpty(email) ? DBNull.Value : email }
            };
            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Users_Get", con, paramDic);
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
                    JoinDate = reader["JoinDate"] != DBNull.Value ? Convert.ToDateTime(reader["JoinDate"]) : DateTime.Now,
                    // Add ProfilePicture if needed
                    ProfilePicture = reader["ProfilePicture"]?.ToString()
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

    // Check if email already exists
    public bool EmailExists(string email)
    {
        var user = GetUser(email: email);
        return user != null;
    }

    // Create user
    public bool CreateUser(User user)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@Username", user.Name },
                { "@Email", user.Email },
                { "@PasswordHash", user.PasswordHash },
                { "@IsAdmin", user.IsAdmin },
                { "@IsLocked", user.IsLocked },
                { "@Bio", user.Bio ?? (object)DBNull.Value }
            };
            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Users_Insert", con, paramDic);
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

    // Update user (basic info)
    public bool UpdateUser(User user)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@UserID", user.Id },
                { "@Username", user.Name },
                { "@PasswordHash", user.PasswordHash },
                { "@IsAdmin", user.IsAdmin },
                { "@IsLocked", user.IsLocked },
                { "@Bio", user.Bio ?? (object)DBNull.Value }
            };
            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Users_Update", con, paramDic);
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

    // Update user profile picture
    public async Task<bool> UpdateUserProfilePic(int userId, string profilePicPath)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@UserID", userId },
                { "@ProfilePicture", profilePicPath }
            };
            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Users_UpdateProfilePic", con, paramDic);
            int affectedRows = await cmd.ExecuteNonQueryAsync();
            return affectedRows > 0;
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

    // Toggle user follow/unfollow
    public async Task<FollowResult> ToggleUserFollow(int followerId, int followedId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;
        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@FollowerUserID", followerId },
                { "@FollowedUserID", followedId }
            };
            
            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_UserFollows_Toggle", con, paramDic);
            reader = await cmd.ExecuteReaderAsync();
            
            if (reader.Read())
            {
                return new FollowResult
                {
                    Action = reader["Action"]?.ToString() ?? "unknown",
                    IsFollowing = reader["Action"]?.ToString() == "followed"
                };
            }
            
            return new FollowResult { Action = "unknown", IsFollowing = false };
        }
        catch (Exception)
        {
            return new FollowResult { Action = "error", IsFollowing = false };
        }
        finally
        {
            reader?.Close();
            con?.Close();
        }
    }

    // Check if user is following another user
    public async Task<bool> IsUserFollowing(int followerId, int followedId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        try
        {
            con = connect("myProjDB");
            string sql = "SELECT COUNT(*) FROM NewsSitePro2025_UserFollows WHERE FollowerUserID = @FollowerUserID AND FollowedUserID = @FollowedUserID";
            cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@FollowerUserID", followerId);
            cmd.Parameters.AddWithValue("@FollowedUserID", followedId);
            
            int count = (int)(await cmd.ExecuteScalarAsync() ?? 0);
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
                { "@Category", (object?)category ?? DBNull.Value },
                {"@CurrentUserID",currentUserId }

            };

            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_NewsArticles_GetAll", con, paramDic);
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
                    ViewsCount = Convert.ToInt32(reader["ViewsCount"]),
                    UserProfilePicture = reader["ProfilePicture"]?.ToString()
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

            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_NewsArticles_Insert", con, paramDic);
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

            try
            {
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_NewsArticles_GetByUser", con, paramDic);
                reader = cmd.ExecuteReader();
            }
            catch
            {
                // If stored procedure doesn't exist, use direct SQL query
                reader?.Close();
                cmd?.Dispose();
                
                int offset = (pageNumber - 1) * pageSize;
                string sql = @"
                    SELECT ArticleID, Title, Content, ImageURL, SourceURL, SourceName, Category, PublishDate,
                           ISNULL(LikesCount, 0) as LikesCount, ISNULL(ViewsCount, 0) as ViewsCount
                    FROM NewsSitePro2025_NewsArticles 
                    WHERE UserID = @UserID 
                    ORDER BY PublishDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                
                cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Offset", offset);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                reader = cmd.ExecuteReader();
            }

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
                    ViewsCount = Convert.ToInt32(reader["ViewsCount"]),
                    UserProfilePicture = reader["ProfilePicture"]?.ToString()
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

            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_ArticleLikes_Toggle", con, paramDic);
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

            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_SavedArticles_Toggle", con, paramDic);
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

            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_ArticleViews_Insert", con, paramDic);
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

            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Reports_Insert", con, paramDic);
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

            try
            {
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_UserStats_Get", con, paramDic);
                reader = cmd.ExecuteReader();
            }
            catch
            {
                // If stored procedure doesn't exist, use direct SQL query
                reader?.Close();
                cmd?.Dispose();
                
                string sql = @"
                    SELECT 
                        (SELECT COUNT(*) FROM NewsArticles WHERE UserID = @UserID) as PostsCount,
                        (SELECT COUNT(*) FROM UserLikes ul INNER JOIN NewsArticles na ON ul.ArticleID = na.ArticleID WHERE na.UserID = @UserID) as LikesCount,
                        (SELECT COUNT(*) FROM SavedArticles WHERE UserID = @UserID) as SavedCount,
                        (SELECT COUNT(*) FROM NewsSitePro2025_UserFollows WHERE FollowedUserID = @UserID) as FollowersCount";
                
                cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                reader = cmd.ExecuteReader();
            }

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

            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_UserProfile_Update", con, paramDic);
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
            cmd = new SqlCommand("SELECT COUNT(*) FROM NewsSitePro2025_ArticleLikes WHERE ArticleID = @ArticleID AND UserID = @UserID", con);
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
            cmd = new SqlCommand("SELECT COUNT(*) FROM NewsSitePro2025_SavedArticles WHERE ArticleID = @ArticleID AND UserID = @UserID", con);
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
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        
        try
        {
            con = connect("myProjDB");
            
            // Try to use stored procedure first
            var paramDic = new Dictionary<string, object>();
            
            try
            {
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Admin_GetDashboardStats", con, paramDic);
                var reader = await cmd.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    stats.TotalUsers = reader.GetInt32("TotalUsers");
                    stats.ActiveUsers = reader.GetInt32("ActiveUsers");
                    stats.BannedUsers = reader.GetInt32("BannedUsers");
                    stats.TotalPosts = reader.GetInt32("TotalPosts");
                    stats.TotalReports = reader.GetInt32("TotalReports");
                    stats.PendingReports = reader.GetInt32("PendingReports");
                    stats.TodayRegistrations = reader.GetInt32("TodayRegistrations");
                    stats.TodayPosts = reader.GetInt32("TodayPosts");
                }
                reader.Close();
            }
            catch
            {
                // Fallback to direct SQL queries
                cmd = new SqlCommand("SELECT COUNT(*) FROM NewsSitePro2025_Users", con);
                var result = await cmd.ExecuteScalarAsync();
                stats.TotalUsers = result != null ? (int)result : 0;
                
                cmd = new SqlCommand("SELECT COUNT(*) FROM NewsSitePro2025_Users WHERE IsActive = 1 AND (IsBanned = 0 OR BannedUntil < GETDATE())", con);
                result = await cmd.ExecuteScalarAsync();
                stats.ActiveUsers = result != null ? (int)result : 0;
                
                cmd = new SqlCommand("SELECT COUNT(*) FROM NewsSitePro2025_Users WHERE IsBanned = 1 AND (BannedUntil IS NULL OR BannedUntil > GETDATE())", con);
                result = await cmd.ExecuteScalarAsync();
                stats.BannedUsers = result != null ? (int)result : 0;
                
                cmd = new SqlCommand("SELECT COUNT(*) FROM NewsSitePro2025_NewsArticles", con);
                result = await cmd.ExecuteScalarAsync();
                stats.TotalPosts = result != null ? (int)result : 0;
                
                cmd = new SqlCommand("SELECT COUNT(*) FROM NewsSitePro2025_Reports", con);
                result = await cmd.ExecuteScalarAsync();
                stats.TotalReports = result != null ? (int)result : 0;
                
                cmd = new SqlCommand("SELECT COUNT(*) FROM NewsSitePro2025_Reports WHERE Status = 'Pending'", con);
                result = cmd.ExecuteScalar();
                stats.PendingReports = result != null ? (int)result : 0;
                
                cmd = new SqlCommand("SELECT COUNT(*) FROM NewsSitePro2025_Users WHERE CAST(JoinDate AS DATE) = CAST(GETDATE() AS DATE)", con);
                result = cmd.ExecuteScalar();
                stats.TodayRegistrations = result != null ? (int)result : 0;
                
                cmd = new SqlCommand("SELECT COUNT(*) FROM NewsSitePro2025_NewsArticles WHERE CAST(PublishDate AS DATE) = CAST(GETDATE() AS DATE)", con);
                result = cmd.ExecuteScalar();
                stats.TodayPosts = result != null ? (int)result : 0;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting admin dashboard stats: " + ex.Message);
        }
        finally
        {
            con?.Close();
        }
        
        return stats;
    }

    public List<AdminUserView> GetAllUsersForAdmin(int page, int pageSize)
    {
        var users = new List<AdminUserView>();
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;
        
        try
        {
            con = connect("myProjDB");
            
            // Try to use stored procedure first
            var paramDic = new Dictionary<string, object>
            {
                { "@PageNumber", page },
                { "@PageSize", pageSize }
            };
            
            try
            {
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Admin_GetAllUsers", con, paramDic);
                reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    var user = new AdminUserView
                    {
                        Id = reader.GetInt32("UserID"),
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
                reader.Close();
            }
            catch
            {
                // Fallback to direct SQL query
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
                
                var offset = (page - 1) * pageSize;
                var query = @"
                    SELECT u.UserID AS ID, u.Username, u.Email, u.Bio, u.JoinDate, u.LastUpdated AS LastActivity, 
                           u.IsAdmin, u.IsActive, u.IsBanned, u.BannedUntil,
                           COALESCE(pc.PostCount, 0) AS PostCount,
                           COALESCE(lc.LikesReceived, 0) AS LikesReceived
                    FROM NewsSitePro2025_Users u
                    LEFT JOIN (
                        SELECT UserID, COUNT(*) AS PostCount 
                        FROM NewsSitePro2025_NewsArticles 
                        GROUP BY UserID
                    ) pc ON u.UserID = pc.UserID
                    LEFT JOIN (
                        SELECT na.UserID, COUNT(*) AS LikesReceived
                        FROM NewsSitePro2025_NewsArticles na
                        INNER JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID
                        GROUP BY na.UserID
                    ) lc ON u.UserID = lc.UserID
                    ORDER BY u.JoinDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Offset", offset);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                
                reader = cmd.ExecuteReader();
                while (reader.Read())
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
        catch (Exception ex)
        {
            throw new Exception("Error getting users for admin: " + ex.Message);
        }
        finally
        {
            if (reader != null && !reader.IsClosed)
            {
                reader.Close();
            }
            con?.Close();
        }
        
        return users;
    }

    public async Task<List<AdminUserView>> GetFilteredUsersForAdmin(int page, int pageSize, string search, string status, string joinDate)
    {
        var users = new List<AdminUserView>();
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;
        
        try
        {
            con = connect("myProjDB");
            
            // Try to use stored procedure first
            var paramDic = new Dictionary<string, object>
            {
                { "@PageNumber", page },
                { "@PageSize", pageSize },
                { "@Search", search ?? "" },
                { "@Status", status ?? "" },
                { "@JoinDate", joinDate ?? "" }
            };
            
            try
            {
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Admin_GetFilteredUsers", con, paramDic);
                reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var user = new AdminUserView
                    {
                        Id = reader.GetInt32("UserID"),
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
                reader.Close();
            }
            catch
            {
                // Fallback to direct SQL query
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
                
                var offset = (page - 1) * pageSize;
                var whereConditions = new List<string>();
                var parameters = new List<SqlParameter>();
                
                // Search filter
                if (!string.IsNullOrEmpty(search))
                {
                    whereConditions.Add("(u.Username LIKE @Search OR u.Email LIKE @Search)");
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
                    SELECT u.UserID, u.Username AS Username, u.Email, u.Bio, u.JoinDate, u.LastActivity, 
                           u.IsAdmin, u.IsActive, u.IsBanned, u.BannedUntil,
                           COALESCE(pc.PostCount, 0) AS PostCount,
                           COALESCE(lc.LikesReceived, 0) AS LikesReceived
                    FROM NewsSitePro2025_Users u
                    LEFT JOIN (
                        SELECT UserID, COUNT(*) AS PostCount 
                        FROM NewsSitePro2025_NewsArticles 
                        GROUP BY UserID
                    ) pc ON u.UserID = pc.UserID
                    LEFT JOIN (
                        SELECT na.UserID, COUNT(*) AS LikesReceived
                        FROM NewsSitePro2025_NewsArticles na
                        INNER JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID
                        GROUP BY na.UserID
                    ) lc ON u.UserID = lc.UserID
                    {whereClause}
                    ORDER BY u.JoinDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Offset", offset);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
                
                reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var user = new AdminUserView
                    {
                        Id = reader.GetInt32("UserID"),
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
        catch (Exception ex)
        {
            throw new Exception("Error getting filtered users for admin: " + ex.Message);
        }
        finally
        {
            if (reader != null && !reader.IsClosed)
            {
                reader.Close();
            }
            con?.Close();
        }
        
        return users;
    }

    public async Task<int> GetFilteredUsersCount(string search, string status, string joinDate)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        
        try
        {
            con = connect("myProjDB");
            
            // Try to use stored procedure first
            var paramDic = new Dictionary<string, object>
            {
                { "@Search", search ?? "" },
                { "@Status", status ?? "" },
                { "@JoinDate", joinDate ?? "" }
            };
            
            try
            {
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Admin_GetFilteredUsersCount", con, paramDic);
                var result = await cmd.ExecuteScalarAsync();
                return result != null ? (int)result : 0;
            }
            catch
            {
                // Fallback to direct SQL query
                var whereConditions = new List<string>();
                var parameters = new List<SqlParameter>();
                
                // Search filter
                if (!string.IsNullOrEmpty(search))
                {
                    whereConditions.Add("(Username LIKE @Search OR Email LIKE @Search)");
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
                var query = $"SELECT COUNT(*) FROM NewsSitePro2025_Users {whereClause}";
                
                cmd = new SqlCommand(query, con);
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
                
                var result = cmd.ExecuteScalar();
                return result != null ? (int)result : 0;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting filtered users count: " + ex.Message);
        }
        finally
        {
            con?.Close();
        }
    }

    public async Task<AdminUserDetails> GetUserDetailsForAdmin(int userId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;
        
        try
        {
            con = connect("myProjDB");
            
            // Try to use stored procedure first
            var paramDic = new Dictionary<string, object>
            {
                { "@UserID", userId }
            };
            
            try
            {
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Admin_GetUserDetails", con, paramDic);
                reader = await cmd.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    var user = new AdminUserDetails
                    {
                        Id = reader.GetInt32("UserID"),
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
                    
                    reader.Close();
                    return user;
                }
                reader.Close();
            }
            catch
            {
                // Fallback to direct SQL query
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
                
                var query = @"
                    SELECT u.UserID, u.Username AS Username, u.Email, u.Bio, u.JoinDate, u.LastActivity, 
                           u.IsAdmin, u.IsActive, u.IsBanned, u.BannedUntil, u.BanReason,
                           COALESCE(pc.PostCount, 0) AS PostCount,
                           COALESCE(lc.LikesReceived, 0) AS LikesReceived
                    FROM NewsSitePro2025_Users u
                    LEFT JOIN (
                        SELECT UserID, COUNT(*) AS PostCount 
                        FROM NewsSitePro2025_NewsArticles 
                        WHERE UserID = @UserId
                        GROUP BY UserID
                    ) pc ON u.UserID = pc.UserID
                    LEFT JOIN (
                        SELECT na.UserID, COUNT(*) AS LikesReceived
                        FROM NewsSitePro2025_NewsArticles na
                        INNER JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID
                        WHERE na.UserID = @UserId
                        GROUP BY na.UserID
                    ) lc ON u.UserID = lc.UserID
                    WHERE u.UserID = @UserId";

                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                
                reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var user = new AdminUserDetails
                    {
                        Id = reader.GetInt32("UserID"),
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
            
            throw new Exception("User not found");
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting user details for admin: " + ex.Message);
        }
        finally
        {
            if (reader != null && !reader.IsClosed)
            {
                reader.Close();
            }
            con?.Close();
        }
    }

    public async Task<bool> BanUser(int userId, string reason, int durationDays, int adminId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        
        try
        {
            con = connect("myProjDB");
            
            // Try to use stored procedure first
            var paramDic = new Dictionary<string, object>
            {
                { "@UserID", userId },
                { "@Reason", reason },
                { "@DurationDays", durationDays },
                { "@AdminID", adminId }
            };
            
            try
            {
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Admin_BanUser", con, paramDic);
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch
            {
                // Fallback to direct SQL query
                var bannedUntil = durationDays == -1 ? (DateTime?)null : DateTime.Now.AddDays(durationDays);
                
                var query = @"
                    UPDATE NewsSitePro2025_Users 
                    SET IsBanned = 1, BannedUntil = @BannedUntil, BanReason = @Reason
                    WHERE UserID = @UserId";
                
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@BannedUntil", (object?)bannedUntil ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Reason", reason);
                
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error banning user: " + ex.Message);
        }
        finally
        {
            con?.Close();
        }
    }

    public async Task<bool> UnbanUser(int userId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        
        try
        {
            con = connect("myProjDB");
            
            // Try to use stored procedure first
            var paramDic = new Dictionary<string, object>
            {
                { "@UserID", userId }
            };
            
            try
            {
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Admin_UnbanUser", con, paramDic);
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch
            {
                // Fallback to direct SQL query
                var query = @"
                    UPDATE NewsSitePro2025_Users 
                    SET IsBanned = 0, BannedUntil = NULL, BanReason = NULL
                    WHERE UserID = @UserId";
                
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error unbanning user: " + ex.Message);
        }
        finally
        {
            con?.Close();
        }
    }

    public async Task<bool> DeactivateUser(int userId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        
        try
        {
            con = connect("myProjDB");
            
            // Try to use stored procedure first
            var paramDic = new Dictionary<string, object>
            {
                { "@UserID", userId }
            };
            
            try
            {
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Admin_DeactivateUser", con, paramDic);
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch
            {
                // Fallback to direct SQL query
                var query = "UPDATE NewsSitePro2025_Users SET IsActive = 0 WHERE UserID = @UserId";
                
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error deactivating user: " + ex.Message);
        }
        finally
        {
            con?.Close();
        }
    }

    public List<ActivityLog> GetRecentActivityLogs(int count)
    {
        var logs = new List<ActivityLog>();
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;
        
        try
        {
            con = connect("myProjDB");
            
            // Try to use stored procedure first
            var paramDic = new Dictionary<string, object>
            {
                { "@Count", count }
            };
            
            try
            {
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Admin_GetRecentActivityLogs", con, paramDic);
                reader = cmd.ExecuteReader();
                
                while (reader.Read())
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
                reader.Close();
            }
            catch
            {
                // Fallback to direct SQL query
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
                
                var query = @"
                    SELECT TOP (@Count) al.ID, al.UserID, u.Username AS Username, al.Action, 
                           al.Details, al.Timestamp, al.IpAddress, al.UserAgent
                    FROM NewsSitePro2025_ActivityLogs al
                    INNER JOIN NewsSitePro2025_Users u ON al.UserID = u.UserID
                    ORDER BY al.Timestamp DESC";
                
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Count", count);
                
                reader = cmd.ExecuteReader();
                while (reader.Read())
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
        catch (Exception ex)
        {
            throw new Exception("Error getting recent activity logs: " + ex.Message);
        }
        finally
        {
            if (reader != null && !reader.IsClosed)
            {
                reader.Close();
            }
            con?.Close();
        }
        
        return logs;
    }

    public async Task<List<ActivityLog>> GetActivityLogs(int page, int pageSize)
    {
        var logs = new List<ActivityLog>();
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;
        
        try
        {
            con = connect("myProjDB");
            
            var offset = (page - 1) * pageSize;
            var query = @"
                SELECT al.ID, al.UserID, u.Username AS Username, al.Action, 
                       al.Details, al.Timestamp, al.IpAddress, al.UserAgent
                FROM NewsSitePro2025_ActivityLogs al
                INNER JOIN NewsSitePro2025_Users u ON al.UserID = u.UserID
                ORDER BY al.Timestamp DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            
            cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            
            reader = await cmd.ExecuteReaderAsync();
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
        catch (Exception ex)
        {
            throw new Exception("Error getting activity logs: " + ex.Message);
        }
        finally
        {
            if (reader != null && !reader.IsClosed)
            {
                reader.Close();
            }
            con?.Close();
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
                    FROM NewsSitePro2025_Reports r
                    INNER JOIN NewsSitePro2025_Users ru ON r.ReporterID = ru.UserID
                    INNER JOIN NewsSitePro2025_Users rpu ON r.ReportedUserID = rpu.UserID
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
                    FROM NewsSitePro2025_Reports r
                    INNER JOIN NewsSitePro2025_Users ru ON r.ReporterID = ru.UserID
                    INNER JOIN NewsSitePro2025_Users rpu ON r.ReportedUserID = rpu.UserID
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
                    UPDATE NewsSitePro2025_Reports 
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
                    INSERT INTO NewsSitePro2025_ActivityLogs (UserID, Action, Details, Timestamp, IpAddress, UserAgent)
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
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;
        
        try
        {
            con = connect("myProjDB");
            // Connection is already opened by connect method, no need to call OpenAsync
            
            var paramDic = new Dictionary<string, object>
            {
                { "@UserID", userId },
                { "@PageNumber", page },
                { "@PageSize", pageSize }
            };
            
            try
            {
                // Try to use stored procedure first
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Notifications_GetUserNotifications", con, paramDic);
                
                reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    notifications.Add(new Notification
                    {
                        ID = reader.GetInt32("NotificationID"),
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
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
            }
            catch (Exception spEx)
            {
                Console.WriteLine($"[DBservices] Stored procedure error: {spEx.Message}");
                // Close reader if it was opened
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
                
                // Fallback to direct SQL query
                notifications.Clear();
                
                var offset = (page - 1) * pageSize;
                var query = @"
                    SELECT n.NotificationID as NotificationID, n.UserID, n.Type, n.Title, n.Message, n.RelatedEntityType, 
                           n.RelatedEntityID, n.IsRead, n.CreatedAt, n.FromUserID, n.ActionUrl,
                           u.Username AS FromUserName
                    FROM NewsSitePro2025_Notifications n
                    LEFT JOIN NewsSitePro2025_Users u ON n.FromUserID = u.UserID
                    WHERE n.UserID = @UserID
                    ORDER BY n.CreatedAt DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Offset", offset);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                
                reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    notifications.Add(new Notification
                    {
                        ID = reader.GetInt32("NotificationID"),
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
        catch (Exception ex)
        {
            Console.WriteLine($"[DBservices] Error in GetUserNotifications: {ex.Message}");
            throw new Exception("Error getting user notifications: " + ex.Message);
        }
        finally
        {
            if (reader != null && !reader.IsClosed)
            {
                reader.Close();
            }
            if (con != null && con.State == System.Data.ConnectionState.Open)
            {
                con.Close();
            }
        }
        
        return notifications;
    }

    public async Task<NotificationSummary> GetNotificationSummary(int userId)
    {
        var summary = new NotificationSummary();
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;
        
        try
        {
            con = connect("myProjDB");
            
            // Try to use stored procedure first
            var paramDic = new Dictionary<string, object>
            {
                { "@UserID", userId }
            };
            
            try
            {
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Notifications_GetSummary", con, paramDic);
                reader = await cmd.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    summary.TotalUnread = reader.GetInt32("TotalUnread");
                    
                    // Read type counts if available
                    while (await reader.NextResultAsync() && await reader.ReadAsync())
                    {
                        summary.UnreadByType[reader.GetString("Type")] = reader.GetInt32("Count");
                    }
                }
                reader.Close();
                
                // Get recent notifications
                summary.RecentNotifications = new List<Notification>(); // Placeholder until GetUserNotifications is updated
            }
            catch
            {
                // Fallback to direct SQL queries
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
                
                // Get total unread count
                cmd = new SqlCommand("SELECT COUNT(*) FROM NewsSitePro2025_Notifications WHERE UserID = @UserID AND IsRead = 0", con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                var result = await cmd.ExecuteScalarAsync();
                summary.TotalUnread = result != null ? (int)result : 0;
                
                // Get unread count by type
                var typeQuery = @"
                    SELECT Type, COUNT(*) AS Count
                    FROM NewsSitePro2025_Notifications
                    WHERE UserID = @UserID AND IsRead = 0
                    GROUP BY Type";
                
                cmd = new SqlCommand(typeQuery, con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                
                reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    summary.UnreadByType[reader.GetString("Type")] = reader.GetInt32("Count");
                }
                reader.Close();
                
                // Get recent notifications
                summary.RecentNotifications = new List<Notification>(); // Placeholder until GetUserNotifications is updated
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting notification summary: " + ex.Message);
        }
        finally
        {
            if (reader != null && !reader.IsClosed)
            {
                reader.Close();
            }
            con?.Close();
        }
        
        return summary;
    }

    public int GetUnreadNotificationCount(int userId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        
        try
        {
            con = connect("myProjDB");
            
            var paramDic = new Dictionary<string, object>
            {
                { "@UserID", userId }
            };
            
            try
            {
                // Try to use stored procedure first
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Notifications_GetUnreadCount", con, paramDic);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch
            {
                // Fallback to direct SQL query
                cmd = new SqlCommand("SELECT COUNT(*) FROM NewsSitePro2025_Notifications WHERE UserID = @UserID AND IsRead = 0", con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                var result = cmd.ExecuteScalar();
                return result != null ? (int)result : 0;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting unread notification count: " + ex.Message);
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Creates a new notification using the stored procedure
    /// </summary>
    /// <param name="request">Notification creation request</param>
    /// <returns>The ID of the created notification, or -1 if failed</returns>
    public async Task<int> CreateNotification(CreateNotificationRequest request)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;
        
        try
        {
            try
            {
                
                con = connect("myProjDB");
            }catch (Exception ex)
            {
                throw new Exception("Error connecting to database: " + ex.Message);
            }

            try
            {


                var paramDic = new Dictionary<string, object>
                {
                    { "@UserID", request.UserID },
                    { "@Type", request.Type },
                    { "@Title", request.Title },
                    { "@Message", request.Message },
                    { "@RelatedEntityType", (object?)request.RelatedEntityType ?? DBNull.Value },
                    { "@RelatedEntityID", (object?)request.RelatedEntityID ?? DBNull.Value },
                    { "@FromUserID", (object?)request.FromUserID ?? DBNull.Value },
                    { "@ActionUrl", (object?)request.ActionUrl ?? DBNull.Value }
                };

                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Notifications_Insert", con, paramDic);

                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : -1;
            }
            catch { throw; }
        }
        catch (Exception ex)
        {
            throw new Exception("Error creating notification: " + ex.Message);
        }
    }

    /// <summary>
    /// Creates a notification when someone comments on a post
    /// </summary>
    /// <param name="postId">ID of the post that was commented on</param>
    /// <param name="commenterUserId">ID of the user who made the comment</param>
    /// <param name="commentId">ID of the created comment</param>
    /// <returns>True if notification was created successfully</returns>
    private async Task<bool> CreateCommentNotification(int postId, int commenterUserId, int commentId)
    {
        try
        {
            // Get the post author's ID to send them the notification
            using (var con = connect("myProjDB"))
            {
                var getPostAuthorQuery = "SELECT UserID FROM NewsSitePro2025_NewsArticles WHERE ArticleID = @PostID";
                using (var cmd = new SqlCommand(getPostAuthorQuery, con))
                {
                    cmd.Parameters.AddWithValue("@PostID", postId);
                    var postAuthorId = await cmd.ExecuteScalarAsync();
                    
                    if (postAuthorId != null)
                    {
                        var authorId = Convert.ToInt32(postAuthorId);
                        
                        // Don't create notification if user commented on their own post
                        if (authorId != commenterUserId)
                        {
                            var notificationRequest = new CreateNotificationRequest
                            {
                                UserID = authorId,
                                Type = "Comment",
                                Title = "New Comment",
                                Message = "Someone commented on your post",
                                RelatedEntityType = "Post",
                                RelatedEntityID = postId,
                                FromUserID = commenterUserId,
                                ActionUrl = $"/Posts/Details/{postId}#comment-{commentId}"
                            };
                            
                            var notificationId = await CreateNotification(notificationRequest);
                            return notificationId > 0;
                        }
                    }
                }
            }
            return true; // Return true even if no notification was needed
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DBservices] Error creating comment notification: {ex.Message}");
            return false; // Don't throw, just return false
        }
    }

    /// <summary>
    /// Marks a specific notification as read using stored procedure
    /// </summary>
    /// <param name="notificationId">ID of the notification to mark as read</param>
    /// <param name="userId">ID of the user (for security)</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> MarkNotificationAsRead(int notificationId, int userId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;
        
        try
        {
            using (con = connect("myProjDB"))
            {
                await con.OpenAsync();
                
                var paramDic = new Dictionary<string, object>
                {
                    { "@NotificationID", notificationId }
                };
                
                // First verify the notification belongs to the user for security
                var verifyQuery = "SELECT COUNT(*) FROM NewsSitePro2025_Notifications WHERE NotificationID = @NotificationID AND UserID = @UserID";
                using (var verifyCmd = new SqlCommand(verifyQuery, con))
                {
                    verifyCmd.Parameters.AddWithValue("@NotificationID", notificationId);
                    verifyCmd.Parameters.AddWithValue("@UserID", userId);
                    
                    var count = (int?)await verifyCmd.ExecuteScalarAsync();
                    if (count == null || count == 0)
                    {
                        return false; // Notification doesn't belong to user
                    }
                }
                
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Notifications_MarkAsRead", con, paramDic);
                
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error marking notification as read: " + ex.Message);
        }
    }

    public async Task<bool> MarkAllNotificationsAsRead(int userId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;
        
        try
        {
            using (con = connect("myProjDB"))
            {
                await con.OpenAsync();
                
                var paramDic = new Dictionary<string, object>
                {
                    { "@UserID", userId }
                };
                
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Notifications_MarkAllAsRead", con, paramDic);
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
        catch (Exception)
        {
            // Fallback to direct SQL if stored procedure doesn't exist
            try
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json").Build();
                string cStr = configuration.GetConnectionString("myProjDB");
                
                using (var fallbackCon = new SqlConnection(cStr))
                {
                    await fallbackCon.OpenAsync();

                    var query = "UPDATE NewsSitePro2025_Notifications SET IsRead = 1 WHERE UserID = @UserID AND IsRead = 0";

                    using (var fallbackCmd = new SqlCommand(query, fallbackCon))
                    {
                        fallbackCmd.Parameters.AddWithValue("@UserID", userId);
                        
                        var rowsAffected = await fallbackCmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error marking all notifications as read: " + ex.Message);
            }
        }
    }

    public async Task<bool> MarkNotificationsAsRead(int userId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;

        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");

              con = new SqlConnection(cStr);
              await con.OpenAsync();

                var query = "UPDATE NewsSitePro2025_Notifications SET IsRead = 1 WHERE UserID = @UserID AND IsRead = 0";

                 cmd = new SqlCommand(query, con);
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            
        }
        catch (Exception ex)
        {
            throw new Exception("Error marking notifications as read: " + ex.Message);
        }
    }

    public async Task<Dictionary<string, NotificationPreferenceSettings>> GetUserNotificationPreferences(int userId)
    {

        SqlConnection? con = null;
SqlCommand? cmd = null;
SqlDataReader? reader = null;
        var preferences = new Dictionary<string, NotificationPreferenceSettings>();
        
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");
            
             con = new SqlConnection(cStr);
              await con.OpenAsync();

                var query = @"
                    SELECT NotificationType, IsEnabled, EmailNotification, PushNotification
                    FROM NewsSitePro2025_NotificationPreferences
                    WHERE UserID = @UserID";

                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserID", userId);

                reader = await cmd.ExecuteReaderAsync();
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
        catch (Exception ex)
        {
            throw new Exception("Error getting notification preferences: " + ex.Message);
        }
        
        return preferences;
    }

    public async Task<bool> UpdateNotificationPreferences(int userId, Dictionary<string, NotificationPreferenceSettings> preferences)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        try
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("myProjDB");

            con = new SqlConnection(cStr);
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

                    cmd = new SqlCommand(query, con);
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

    //--------------------------------------------------------------------------------------------------
    // Comment Management Methods
    //--------------------------------------------------------------------------------------------------
    
    public async Task<List<NewsSite.BL.Comment>> GetCommentsByPostId(int postId)
    {
        var comments = new List<NewsSite.BL.Comment>();
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        SqlDataReader? reader = null;

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@PostID", postId }
            };
            
            try
            {
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Comments_GetByPostID", con, paramDic);
                reader = await cmd.ExecuteReaderAsync();
            }
            catch
            {
                // If stored procedure doesn't exist, use direct SQL query
                reader?.Close();
                cmd?.Dispose();
                
                string sql = @"
                    SELECT c.CommentID, c.PostID, c.UserID, c.Content, c.CreatedAt, c.UpdatedAt, 
                           c.IsDeleted, c.ParentCommentID, u.Name as UserName,
                           (SELECT COUNT(*) FROM CommentLikes cl WHERE cl.CommentID = c.CommentID) as LikesCount
                    FROM NewsSitePro2025_Comments c
                    INNER JOIN NewsSitePro2025_Users u ON c.UserID = u.UserID
                    WHERE c.PostID = @PostID AND c.IsDeleted = 0
                    ORDER BY c.CreatedAt ASC";
                
                cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@PostID", postId);
                reader = await cmd.ExecuteReaderAsync();
            }

            while (await reader.ReadAsync())
            {
                var comment = new NewsSite.BL.Comment
                {
                    ID = Convert.ToInt32(reader["CommentID"]),
                    PostID = Convert.ToInt32(reader["PostID"]),
                    UserID = Convert.ToInt32(reader["UserID"]),
                    Content = reader["Content"]?.ToString() ?? "",
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                    UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["UpdatedAt"]) : null,
                    IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                    ParentCommentID = reader["ParentCommentID"] != DBNull.Value ? Convert.ToInt32(reader["ParentCommentID"]) : null,
                    UserName = reader["UserName"]?.ToString(),
                    LikesCount = reader["LikesCount"] != DBNull.Value ? Convert.ToInt32(reader["LikesCount"]) : 0
                };
                comments.Add(comment);
            }
            
            // Organize comments into parent-child structure
            var parentComments = comments.Where(c => c.ParentCommentID == null).ToList();
            foreach (var parent in parentComments)
            {
                parent.Replies = comments.Where(c => c.ParentCommentID == parent.ID).ToList();
            }
            
            return parentComments;
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

    /// <summary>
    /// Creates a new comment and returns the comment ID
    /// </summary>
    /// <param name="comment">Comment to create</param>
    /// <returns>The ID of the created comment, or -1 if failed</returns>
    public async Task<int> CreateComment(NewsSite.BL.Comment comment)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        
        try
        {
            con = connect("myProjDB");
            // Connection is already opened by connect method, no need to call OpenAsync
            
            // Map PostID to ArticleID for the stored procedure
            var paramDic = new Dictionary<string, object>
            {
                { "@ArticleID", comment.PostID },
                { "@UserID", comment.UserID },
                { "@Content", comment.Content }
            };
            
            int commentId = -1;
            
            try
            {
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Comments_Insert", con, paramDic);
                var result = await cmd.ExecuteScalarAsync();
                commentId = result != null ? Convert.ToInt32(result) : -1;
            }
            catch
            {
                // If stored procedure doesn't exist, use direct SQL query with SCOPE_IDENTITY()
                cmd?.Dispose();
                
                string sql = @"
                    INSERT INTO NewsSitePro2025_Comments (PostID, UserID, Content, ParentCommentID, CreatedAt)
                    VALUES (@PostID, @UserID, @Content, @ParentCommentID, GETDATE());
                    SELECT SCOPE_IDENTITY();";
                
                cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@PostID", comment.PostID);
                cmd.Parameters.AddWithValue("@UserID", comment.UserID);
                cmd.Parameters.AddWithValue("@Content", comment.Content);
                cmd.Parameters.AddWithValue("@ParentCommentID", comment.ParentCommentID ?? (object)DBNull.Value);
                
                var result = await cmd.ExecuteScalarAsync();
                commentId = result != null ? Convert.ToInt32(result) : -1;
            }
            
            // Create notification for the post author if comment was created successfully
            if (commentId > 0)
            {
                try
                {
                    await CreateCommentNotification(comment.PostID, comment.UserID, commentId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DBservices] Failed to create comment notification: {ex.Message}");
                    // Don't fail the comment creation if notification fails
                }
            }
            
            return commentId;
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

    public async Task<bool> UpdateComment(int commentId, int userId, string content)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        
        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@CommentID", commentId },
                { "@UserID", userId },
                { "@Content", content }
            };
            
            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Comments_Update", con, paramDic);
            int result = await cmd.ExecuteNonQueryAsync();
            return result > 0;
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

    public async Task<bool> DeleteComment(int commentId, int userId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        
        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@CommentID", commentId },
                { "@UserID", userId }
            };
            
            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Comments_Delete", con, paramDic);
            int result = await cmd.ExecuteNonQueryAsync();
            return result > 0;
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

    public async Task<int> GetCommentsCount(int postId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;
        
        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@PostID", postId }
            };
            
            cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Comments_GetCount", con, paramDic);
            object result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
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

    /// <summary>
    /// Toggles like on a comment
    /// </summary>
    /// <param name="commentId">ID of the comment to like/unlike</param>
    /// <param name="userId">ID of the user performing the action</param>
    /// <returns>String indicating the action performed ("liked" or "unliked")</returns>
    public async Task<string> ToggleCommentLike(int commentId, int userId)
    {
        SqlConnection? con = null;
        SqlCommand? cmd = null;

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@CommentID", commentId },
                { "@UserID", userId }
            };

            try
            {
                // Try to use stored procedure first
                cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_Comments_ToggleLike", con, paramDic);
                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "error";
            }
            catch
            {
                // Fallback to direct SQL query
                cmd?.Dispose();
                
                // Check if like exists
                var checkQuery = "SELECT COUNT(*) FROM NewsSitePro2025_CommentLikes WHERE CommentID = @CommentID AND UserID = @UserID";
                cmd = new SqlCommand(checkQuery, con);
                cmd.Parameters.AddWithValue("@CommentID", commentId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                
                var exists = Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
                
                if (exists)
                {
                    // Remove like
                    cmd?.Dispose();
                    cmd = new SqlCommand("DELETE FROM NewsSitePro2025_CommentLikes WHERE CommentID = @CommentID AND UserID = @UserID", con);
                    cmd.Parameters.AddWithValue("@CommentID", commentId);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    await cmd.ExecuteNonQueryAsync();
                    return "unliked";
                }
                else
                {
                    // Add like
                    cmd?.Dispose();
                    cmd = new SqlCommand("INSERT INTO NewsSitePro2025_CommentLikes (CommentID, UserID, LikeDate) VALUES (@CommentID, @UserID, GETDATE())", con);
                    cmd.Parameters.AddWithValue("@CommentID", commentId);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    await cmd.ExecuteNonQueryAsync();
                    return "liked";
                }
            }
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
    // Search Articles
    //--------------------------------------------------------------------------------------------------
    public async Task<List<NewsArticle>> SearchArticlesAsync(string searchTerm, string category = "", int pageNumber = 1, int pageSize = 10, int? currentUserId = null)
    {
        SqlConnection? con = null;
        List<NewsArticle> articles = new List<NewsArticle>();

        try
        {
            con = connect("myProjDB");
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                ["@SearchTerm"] = searchTerm,
                ["@Category"] = string.IsNullOrEmpty(category) ? (object)DBNull.Value : category,
                ["@PageNumber"] = pageNumber,
                ["@PageSize"] = pageSize,
                ["@CurrentUserID"] = currentUserId.HasValue ? (object)currentUserId.Value : DBNull.Value
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_NewsArticles_Search", con, paramDic);
            SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                articles.Add(new NewsArticle
                {
                    ArticleID = reader.GetInt32("ArticleID"),
                    Title = reader["Title"]?.ToString(),
                    Content = reader["Content"]?.ToString(),
                    ImageURL = reader["ImageURL"]?.ToString(),
                    SourceURL = reader["SourceURL"]?.ToString(),
                    SourceName = reader["SourceName"]?.ToString(),
                    Category = reader["Category"]?.ToString(),
                    PublishDate = reader.GetDateTime("PublishDate"),
                    UserID = reader.GetInt32("UserID"),
                    Username = reader["Username"]?.ToString(),
                    LikesCount = reader.GetInt32("LikesCount"),
                    ViewsCount = reader.GetInt32("ViewsCount"),
                    IsLiked = reader.GetInt32("IsLiked") == 1,
                    IsSaved = reader.GetInt32("IsSaved") == 1
                });
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            con?.Close();
        }

        return articles;
    }

    //--------------------------------------------------------------------------------------------------
    // Search Users
    //--------------------------------------------------------------------------------------------------
    public async Task<List<User>> SearchUsersAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        SqlConnection? con = null;
        List<User> users = new List<User>();

        try
        {
            con = connect("myProjDB");
            
            // Use a custom search query since we don't have a stored procedure for user search yet
            string sql = @"
                SELECT TOP (@PageSize) u.UserID, u.Username, u.Email, u.Bio, u.JoinDate, u.IsAdmin, u.IsLocked,
                       (SELECT COUNT(*) FROM NewsSitePro2025_NewsArticles WHERE UserID = u.UserID) as PostsCount
                FROM NewsSitePro2025_Users u
                WHERE (u.Username LIKE @SearchTerm OR u.Email LIKE @SearchTerm OR u.Bio LIKE @SearchTerm)
                ORDER BY u.Username
                OFFSET (@PageNumber - 1) * @PageSize ROWS";

            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                var user = new User
                {
                    Id = reader.GetInt32("UserID"),
                    Name = reader["Username"]?.ToString(),
                    Email = reader["Email"]?.ToString(),
                    Bio = reader["Bio"]?.ToString(),
                    JoinDate = reader.GetDateTime("JoinDate"),
                    IsAdmin = reader.GetBoolean("IsAdmin"),
                    IsLocked = reader.GetBoolean("IsLocked")
                };
                
                // Add computed properties for display
                user.GetType().GetProperties()
                    .Where(p => p.Name == "PostsCount")
                    .FirstOrDefault()?.SetValue(user, reader.GetInt32("PostsCount"));

                users.Add(user);
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            con?.Close();
        }

        return users;
    }

    //--------------------------------------------------------------------------------------------------
    // User Liked and Saved Articles Methods
    //--------------------------------------------------------------------------------------------------
    
    public async Task<List<NewsArticle>> GetLikedArticlesByUser(int userId, int pageNumber = 1, int pageSize = 10)
    {
        SqlConnection? con = null;
        List<NewsArticle> articles = new List<NewsArticle>();

        try
        {
            con = connect("myProjDB");
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                ["@UserID"] = userId,
                ["@PageNumber"] = pageNumber,
                ["@PageSize"] = pageSize
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_UserLikedArticles_Get", con, paramDic);
            SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                articles.Add(new NewsArticle
                {
                    ArticleID = reader.GetInt32("ArticleID"),
                    Title = reader["Title"]?.ToString(),
                    Content = reader["Content"]?.ToString(),
                    ImageURL = reader["ImageURL"]?.ToString(),
                    SourceURL = reader["SourceURL"]?.ToString(),
                    SourceName = reader["SourceName"]?.ToString(),
                    Category = reader["Category"]?.ToString(),
                    PublishDate = reader.GetDateTime("PublishDate"),
                    UserID = reader.GetInt32("UserID"),
                    Username = reader["Username"]?.ToString(),
                    LikesCount = reader.GetInt32("LikesCount"),
                    ViewsCount = reader.GetInt32("ViewsCount"),
                    IsLiked = true, // Always true for liked articles
                    IsSaved = reader.GetInt32("IsSaved") == 1
                });
            }
        }
        catch (Exception)
        {
            // If stored procedure doesn't exist, use direct SQL query
            try
            {
                if (con != null)
                {
                    con.Close();
                    con = connect("myProjDB");
                }
                
                string sql = @"
                    SELECT na.ArticleID, na.Title, na.Content, na.ImageURL, na.SourceURL, na.SourceName, 
                           na.Category, na.PublishDate, na.UserID, u.Name as Username,
                           COALESCE(lc.LikesCount, 0) as LikesCount,
                           COALESCE(vc.ViewsCount, 0) as ViewsCount,
                           CASE WHEN sa.UserID IS NOT NULL THEN 1 ELSE 0 END as IsSaved
                    FROM NewsArticles na
                    INNER JOIN ArticleLikes al ON na.ArticleID = al.ArticleID
                    INNER JOIN Users_News u ON na.UserID = u.ID
                    LEFT JOIN (
                        SELECT ArticleID, COUNT(*) as LikesCount
                        FROM ArticleLikes
                        GROUP BY ArticleID
                    ) lc ON na.ArticleID = lc.ArticleID
                    LEFT JOIN (
                        SELECT ArticleID, COUNT(*) as ViewsCount
                        FROM ArticleViews
                        GROUP BY ArticleID
                    ) vc ON na.ArticleID = vc.ArticleID
                    LEFT JOIN SavedArticles sa ON na.ArticleID = sa.ArticleID AND sa.UserID = @UserID
                    WHERE al.UserID = @UserID
                    ORDER BY al.CreatedAt DESC
                    OFFSET (@PageNumber - 1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    articles.Add(new NewsArticle
                    {
                        ArticleID = reader.GetInt32("ArticleID"),
                        Title = reader["Title"]?.ToString(),
                        Content = reader["Content"]?.ToString(),
                        ImageURL = reader["ImageURL"]?.ToString(),
                        SourceURL = reader["SourceURL"]?.ToString(),
                        SourceName = reader["SourceName"]?.ToString(),
                        Category = reader["Category"]?.ToString(),
                        PublishDate = reader.GetDateTime("PublishDate"),
                        UserID = reader.GetInt32("UserID"),
                        Username = reader["Username"]?.ToString(),
                        LikesCount = reader.GetInt32("LikesCount"),
                        ViewsCount = reader.GetInt32("ViewsCount"),
                        IsLiked = true,
                        IsSaved = reader.GetInt32("IsSaved") == 1
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        finally
        {
            con?.Close();
        }

        return articles;
    }

    public async Task<List<NewsArticle>> GetSavedArticlesByUser(int userId, int pageNumber = 1, int pageSize = 10)
    {
        SqlConnection? con = null;
        List<NewsArticle> articles = new List<NewsArticle>();

        try
        {
            con = connect("myProjDB");
            
            // Use direct SQL query instead of stored procedure
            int offset = (pageNumber - 1) * pageSize;
            string sql = @"
                SELECT 
                    na.ArticleID,
                    na.Title,
                    na.Content,
                    na.ImageURL,
                    na.SourceURL,
                    na.SourceName,
                    na.Category,
                    na.PublishDate,
                    na.UserID,
                    u.Username,
                    sa.SaveDate
                FROM NewsSitePro2025_SavedArticles sa
                INNER JOIN NewsSitePro2025_NewsArticles na ON sa.ArticleID = na.ArticleID
                INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
                WHERE sa.UserID = @UserID
                ORDER BY sa.SaveDate DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Offset", offset);
            
            SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                articles.Add(new NewsArticle
                {
                    ArticleID = reader.GetInt32("ArticleID"),
                    Title = reader["Title"]?.ToString(),
                    Content = reader["Content"]?.ToString(),
                    ImageURL = reader["ImageURL"]?.ToString(),
                    SourceURL = reader["SourceURL"]?.ToString(),
                    SourceName = reader["SourceName"]?.ToString(),
                    Category = reader["Category"]?.ToString(),
                    PublishDate = reader.GetDateTime("PublishDate"),
                    UserID = reader.GetInt32("UserID"),
                    Username = reader["Username"]?.ToString(),
                    LikesCount = 0, // Set default values
                    ViewsCount = 0,
                    IsLiked = false,
                    IsSaved = true // Always true for saved articles
                });
            }
        }
        catch (Exception)
        {
            // If stored procedure doesn't exist, use direct SQL query
            try
            {
                if (con != null)
                {
                    con.Close();
                    con = connect("myProjDB");
                }
                
                string sql = @"
                    SELECT na.ArticleID, na.Title, na.Content, na.ImageURL, na.SourceURL, na.SourceName, 
                           na.Category, na.PublishDate, na.UserID, u.Username,
                           COALESCE(lc.LikesCount, 0) as LikesCount,
                           COALESCE(vc.ViewsCount, 0) as ViewsCount,
                           CASE WHEN al.UserID IS NOT NULL THEN 1 ELSE 0 END as IsLiked
                    FROM NewsSitePro2025_NewsArticles na
                    INNER JOIN NewsSitePro2025_SavedArticles sa ON na.ArticleID = sa.ArticleID
                    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
                    LEFT JOIN (
                        SELECT ArticleID, COUNT(*) as LikesCount
                        FROM NewsSitePro2025_ArticleLikes
                        GROUP BY ArticleID
                    ) lc ON na.ArticleID = lc.ArticleID
                    LEFT JOIN (
                        SELECT ArticleID, COUNT(*) as ViewsCount
                        FROM NewsSitePro2025_ArticleViews
                        GROUP BY ArticleID
                    ) vc ON na.ArticleID = vc.ArticleID
                    LEFT JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID AND al.UserID = @UserID
                    WHERE sa.UserID = @UserID
                    ORDER BY sa.SaveDate DESC
                    OFFSET (@PageNumber - 1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    articles.Add(new NewsArticle
                    {
                        ArticleID = reader.GetInt32("ArticleID"),
                        Title = reader["Title"]?.ToString(),
                        Content = reader["Content"]?.ToString(),
                        ImageURL = reader["ImageURL"]?.ToString(),
                        SourceURL = reader["SourceURL"]?.ToString(),
                        SourceName = reader["SourceName"]?.ToString(),
                        Category = reader["Category"]?.ToString(),
                        PublishDate = reader.GetDateTime("PublishDate"),
                        UserID = reader.GetInt32("UserID"),
                        Username = reader["Username"]?.ToString(),
                        LikesCount = reader.GetInt32("LikesCount"),
                        ViewsCount = reader.GetInt32("ViewsCount"),
                        IsLiked = reader.GetInt32("IsLiked") == 1,
                        IsSaved = true
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        finally
        {
            con?.Close();
        }

        return articles;
    }

    public async Task<NewsArticle?> GetNewsArticleById(int articleId)
    {
        SqlConnection? con = null;
        NewsArticle? article = null;

        try
        {
            con = connect("myProjDB");
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                ["@ArticleID"] = articleId
            };

            SqlCommand cmd1 = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_NewsArticles_Get", con, paramDic);
            SqlDataReader reader1 = await cmd1.ExecuteReaderAsync();

            if (false)//reader.Read())
            {
                article = new NewsArticle
                {
                    //ArticleID = reader.GetInt32("ArticleID"),
                    //Title = reader["Title"]?.ToString(),
                    //Content = reader["Content"]?.ToString(),
                    //ImageURL = reader["ImageURL"]?.ToString(),
                    //SourceURL = reader["SourceURL"]?.ToString(),
                    //SourceName = reader["SourceName"]?.ToString(),
                    //Category = reader["Category"]?.ToString(),
                    //PublishDate = reader.GetDateTime("PublishDate"),
                    //UserID = reader.GetInt32("UserID"),
                    //Username = reader["Username"]?.ToString(),
                    //LikesCount = reader.GetInt32("LikesCount"),
                    //ViewsCount = reader.GetInt32("ViewsCount")
                };
            }

            // If stored procedure doesn't exist, use direct SQL query
            try
            {
                if (con != null)
                {
                    con.Close();
                    con = connect("myProjDB");
                }

                string sql = @"
                    SELECT na.ArticleID, na.Title, na.Content, na.ImageURL, na.SourceURL, na.SourceName, 
                           na.Category, na.PublishDate, na.UserID, u.Username as Username,
                           COALESCE(lc.LikesCount, 0) as LikesCount,
                           COALESCE(vc.ViewsCount, 0) as ViewsCount
                    FROM NewsSitePro2025_NewsArticles na
                    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
                    LEFT JOIN (
                        SELECT ArticleID, COUNT(*) as LikesCount
                        FROM NewsSitePro2025_ArticleLikes
                        GROUP BY ArticleID
                    ) lc ON na.ArticleID = lc.ArticleID
                    LEFT JOIN (
                        SELECT ArticleID, COUNT(*) as ViewsCount
                        FROM NewsSitePro2025_ArticleViews
                        GROUP BY ArticleID
                    ) vc ON na.ArticleID = vc.ArticleID
                    WHERE na.ArticleID = @ArticleID";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@ArticleID", articleId);

                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (reader.Read())
                {
                    article = new NewsArticle
                    {
                        ArticleID = reader.GetInt32("ArticleID"),
                        Title = reader["Title"]?.ToString(),
                        Content = reader["Content"]?.ToString(),
                        ImageURL = reader["ImageURL"]?.ToString(),
                        SourceURL = reader["SourceURL"]?.ToString(),
                        SourceName = reader["SourceName"]?.ToString(),
                        Category = reader["Category"]?.ToString(),
                        PublishDate = reader.GetDateTime("PublishDate"),
                        UserID = reader.GetInt32("UserID"),
                        Username = reader["Username"]?.ToString(),
                        LikesCount = reader.GetInt32("LikesCount"),
                        ViewsCount = reader.GetInt32("ViewsCount")
                    };
                }
            
                
            }
            catch (Exception)
            {
                throw;
            }
        }
        finally
        {
            con?.Close();
        }

        return article;
    }

    public async Task<bool> DeleteNewsArticle(int articleId)
    {
        SqlConnection? con = null;
        bool success = false;

        try
        {
            con = connect("myProjDB");
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                ["@ArticleID"] = articleId
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_NewsArticle_Delete", con, paramDic);
            int result = await cmd.ExecuteNonQueryAsync();
            success = result > 0;
        }
        catch (Exception)
        {
            // If stored procedure doesn't exist, use direct SQL query
            try
            {
                if (con != null)
                {
                    con.Close();
                    con = connect("myProjDB");
                }
                
                string sql = "DELETE FROM NewsArticles WHERE ArticleID = @ArticleID";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@ArticleID", articleId);

                int result = await cmd.ExecuteNonQueryAsync();
                success = result > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }
        finally
        {
            con?.Close();
        }

        return success;
    }

    //--------------------------------------------------------------------------------------------------
    // Recommendation System Methods
    //--------------------------------------------------------------------------------------------------
    
    public async Task<List<NewsArticle>> GetRecommendedArticlesAsync(int userId, int count = 20)
    {
        var articles = new List<NewsArticle>();
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@UserID", userId },
                { "@Count", count }
            };

            try
            {
                var cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_GetRecommendedArticles", con, paramDic);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    articles.Add(new NewsArticle
                    {
                        ArticleID = reader.GetInt32("ArticleID"),
                        Title = reader["Title"]?.ToString(),
                        Content = reader["Content"]?.ToString(),
                        ImageURL = reader["ImageURL"]?.ToString(),
                        SourceURL = reader["SourceURL"]?.ToString(),
                        SourceName = reader["SourceName"]?.ToString(),
                        Category = reader["Category"]?.ToString(),
                        PublishDate = reader.GetDateTime("PublishDate"),
                        UserID = reader.GetInt32("UserID"),
                        Username = reader["Username"]?.ToString(),
                        LikesCount = reader.GetInt32("LikesCount"),
                        ViewsCount = reader.GetInt32("ViewsCount"),
                        IsLiked = await CheckUserLikedArticleAsync(reader.GetInt32("ArticleID"), userId),
                        IsSaved = await CheckUserSavedArticleAsync(reader.GetInt32("ArticleID"), userId)
                    });
                }
            }
            catch
            {
                // Fallback to basic query if stored procedure doesn't exist
                var sql = @"
                    SELECT TOP (@Count) na.ArticleID, na.Title, na.Content, na.ImageURL, na.SourceURL, 
                           na.SourceName, na.Category, na.PublishDate, na.UserID, u.Name as Username,
                           COALESCE(lc.LikesCount, 0) as LikesCount,
                           COALESCE(vc.ViewsCount, 0) as ViewsCount
                    FROM NewsArticles na
                    INNER JOIN Users_News u ON na.UserID = u.ID
                    LEFT JOIN (
                        SELECT ArticleID, COUNT(*) as LikesCount
                        FROM ArticleLikes
                        GROUP BY ArticleID
                    ) lc ON na.ArticleID = lc.ArticleID
                    LEFT JOIN (
                        SELECT ArticleID, COUNT(*) as ViewsCount
                        FROM ArticleViews
                        GROUP BY ArticleID
                    ) vc ON na.ArticleID = vc.ArticleID
                    WHERE na.PublishDate >= DATEADD(day, -7, GETDATE())
                    ORDER BY 
                        (COALESCE(lc.LikesCount, 0) * 3 + COALESCE(vc.ViewsCount, 0)) DESC,
                        na.PublishDate DESC";

                var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Count", count);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    articles.Add(new NewsArticle
                    {
                        ArticleID = reader.GetInt32("ArticleID"),
                        Title = reader["Title"]?.ToString(),
                        Content = reader["Content"]?.ToString(),
                        ImageURL = reader["ImageURL"]?.ToString(),
                        SourceURL = reader["SourceURL"]?.ToString(),
                        SourceName = reader["SourceName"]?.ToString(),
                        Category = reader["Category"]?.ToString(),
                        PublishDate = reader.GetDateTime("PublishDate"),
                        UserID = reader.GetInt32("UserID"),
                        Username = reader["Username"]?.ToString(),
                        LikesCount = reader.GetInt32("LikesCount"),
                        ViewsCount = reader.GetInt32("ViewsCount"),
                        IsLiked = await CheckUserLikedArticleAsync(reader.GetInt32("ArticleID"), userId),
                        IsSaved = await CheckUserSavedArticleAsync(reader.GetInt32("ArticleID"), userId)
                    });
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            con?.Close();
        }

        return articles;
    }

    public async Task<List<UserInterest>> GetUserInterestsAsync(int userId)
    {
        var interests = new List<UserInterest>();
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            var sql = @"
                SELECT InterestID, UserID, Category, InterestScore, LastUpdated
                FROM NewsSitePro2025_UserInterests
                WHERE UserID = @UserID
                ORDER BY InterestScore DESC";

            var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@UserID", userId);
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                interests.Add(new UserInterest
                {
                    InterestID = reader.GetInt32("InterestID"),
                    UserID = reader.GetInt32("UserID"),
                    Category = reader.GetString("Category"),
                    InterestScore = reader.GetDouble("InterestScore"),
                    LastUpdated = reader.GetDateTime("LastUpdated")
                });
            }
        }
        catch (Exception ex)
        {
            // Table might not exist yet, return empty list
            Console.WriteLine($"UserInterests table error: {ex.Message}");
        }
        finally
        {
            con?.Close();
        }

        return interests;
    }

    public async Task<bool> UpdateUserInterestAsync(int userId, string category, double interestScore)
    {
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            var sql = @"
                MERGE NewsSitePro2025_UserInterests AS target
                USING (VALUES (@UserID, @Category, @InterestScore)) AS source (UserID, Category, InterestScore)
                ON target.UserID = source.UserID AND target.Category = source.Category
                WHEN MATCHED THEN
                    UPDATE SET InterestScore = source.InterestScore, LastUpdated = GETDATE()
                WHEN NOT MATCHED THEN
                    INSERT (UserID, Category, InterestScore, LastUpdated)
                    VALUES (source.UserID, source.Category, source.InterestScore, GETDATE());";

            var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@Category", category);
            cmd.Parameters.AddWithValue("@InterestScore", interestScore);

            await cmd.ExecuteNonQueryAsync();
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

    public async Task<UserBehavior?> GetUserBehaviorAsync(int userId)
    {
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            var sql = @"
                SELECT UserID, TotalViews, TotalLikes, TotalShares, TotalComments, 
                       AvgSessionDuration, LastActivity, PreferredReadingTime, 
                       MostActiveHour, FavoriteCategories
                FROM NewsSitePro2025_UserBehavior
                WHERE UserID = @UserID";

            var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@UserID", userId);
            var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new UserBehavior
                {
                    UserID = reader.GetInt32("UserID"),
                    TotalViews = reader.GetInt32("TotalViews"),
                    TotalLikes = reader.GetInt32("TotalLikes"),
                    TotalShares = reader.GetInt32("TotalShares"),
                    TotalComments = reader.GetInt32("TotalComments"),
                    AvgSessionDuration = reader.GetDouble("AvgSessionDuration"),
                    LastActivity = reader.GetDateTime("LastActivity"),
                    PreferredReadingTime = TimeSpan.Parse(reader.GetString("PreferredReadingTime") ?? "08:00:00"),
                    MostActiveHour = reader.GetInt32("MostActiveHour"),
                    FavoriteCategories = (reader.GetString("FavoriteCategories") ?? "").Split(',').Where(s => !string.IsNullOrEmpty(s)).ToList()
                };
            }
        }
        catch (Exception)
        {
            // Table might not exist yet, return null
        }
        finally
        {
            con?.Close();
        }

        return null;
    }

    public async Task<bool> UpdateUserBehaviorAsync(UserBehavior behavior)
    {
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            var favoriteCategories = string.Join(",", behavior.FavoriteCategories ?? new List<string>());
            var sql = @"
                MERGE NewsSitePro2025_UserBehavior AS target
                USING (VALUES (@UserID, @TotalViews, @TotalLikes, @TotalShares, @TotalComments, 
                              @AvgSessionDuration, @PreferredReadingTime, @MostActiveHour, @FavoriteCategories)) 
                AS source (UserID, TotalViews, TotalLikes, TotalShares, TotalComments, 
                          AvgSessionDuration, PreferredReadingTime, MostActiveHour, FavoriteCategories)
                ON target.UserID = source.UserID
                WHEN MATCHED THEN
                    UPDATE SET TotalViews = source.TotalViews, TotalLikes = source.TotalLikes,
                              TotalShares = source.TotalShares, TotalComments = source.TotalComments,
                              AvgSessionDuration = source.AvgSessionDuration, LastActivity = GETDATE(),
                              PreferredReadingTime = source.PreferredReadingTime,
                              MostActiveHour = source.MostActiveHour,
                              FavoriteCategories = source.FavoriteCategories
                WHEN NOT MATCHED THEN
                    INSERT (UserID, TotalViews, TotalLikes, TotalShares, TotalComments, 
                           AvgSessionDuration, LastActivity, PreferredReadingTime, 
                           MostActiveHour, FavoriteCategories)
                    VALUES (source.UserID, source.TotalViews, source.TotalLikes, source.TotalShares, 
                           source.TotalComments, source.AvgSessionDuration, GETDATE(),
                           source.PreferredReadingTime, source.MostActiveHour, source.FavoriteCategories);";

            var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@UserID", behavior.UserID);
            cmd.Parameters.AddWithValue("@TotalViews", behavior.TotalViews);
            cmd.Parameters.AddWithValue("@TotalLikes", behavior.TotalLikes);
            cmd.Parameters.AddWithValue("@TotalShares", behavior.TotalShares);
            cmd.Parameters.AddWithValue("@TotalComments", behavior.TotalComments);
            cmd.Parameters.AddWithValue("@AvgSessionDuration", behavior.AvgSessionDuration);
            cmd.Parameters.AddWithValue("@PreferredReadingTime", behavior.PreferredReadingTime);
            cmd.Parameters.AddWithValue("@MostActiveHour", behavior.MostActiveHour);
            cmd.Parameters.AddWithValue("@FavoriteCategories", favoriteCategories);

            await cmd.ExecuteNonQueryAsync();
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

    public async Task<bool> RecordUserInteractionAsync(int userId, int articleId, string interactionType)
    {
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            var sql = @"
                INSERT INTO NewsSitePro2025_ArticleInteractions (UserID, ArticleID, InteractionType, Timestamp)
                VALUES (@UserID, @ArticleID, @InteractionType, GETDATE())";

            var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@ArticleID", articleId);
            cmd.Parameters.AddWithValue("@InteractionType", interactionType);

            await cmd.ExecuteNonQueryAsync();
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

    /// <summary>
    /// Gets trending topics using the enhanced calculation stored procedure
    /// </summary>
    /// <param name="count">Number of trending topics to return</param>
    /// <param name="category">Optional category filter</param>
    /// <param name="minScore">Minimum trend score threshold</param>
    /// <returns>List of trending topics</returns>
    public async Task<List<TrendingTopic>> GetTrendingTopicsAsync(int count = 10, string? category = null, double minScore = 0.0)
    {
        var topics = new List<TrendingTopic>();
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            
            // Use the new stored procedure for better trending calculation
            var paramDic = new Dictionary<string, object>
            {
                { "@Count", count },
                { "@MinScore", minScore }
            };
            
            if (!string.IsNullOrEmpty(category))
            {
                paramDic.Add("@Category", category);
            }

            var cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_TrendingTopics_Get", con, paramDic);
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                topics.Add(new TrendingTopic
                {
                    TrendID = reader.GetInt32("TrendID"),
                    Topic = reader.GetString("Topic"),
                    Category = reader.GetString("Category"),
                    TrendScore = reader.GetDouble("TrendScore"),
                    TotalInteractions = reader.GetInt32("TotalInteractions"),
                    LastUpdated = reader.GetDateTime("LastUpdated"),
                    RelatedKeywords = reader.IsDBNull("RelatedKeywords") ? null : reader.GetString("RelatedKeywords"),
                    GeographicRegions = reader.IsDBNull("GeographicRegions") ? null : reader.GetString("GeographicRegions")
                });
            }
        }
        catch (Exception)
        {
            // If stored procedure doesn't exist, return sample trending topics for development
            topics.AddRange(new[]
            {
                new TrendingTopic { TrendID = 1, Topic = "AI Technology", Category = "Technology", TrendScore = 85.5, TotalInteractions = 150, LastUpdated = DateTime.Now },
                new TrendingTopic { TrendID = 2, Topic = "World Cup 2025", Category = "Sports", TrendScore = 78.2, TotalInteractions = 120, LastUpdated = DateTime.Now },
                new TrendingTopic { TrendID = 3, Topic = "Election Updates", Category = "Politics", TrendScore = 72.8, TotalInteractions = 98, LastUpdated = DateTime.Now }
            });
        }
        finally
        {
            con?.Close();
        }

        return topics;
    }

    /// <summary>
    /// Calculates and updates trending topics based on current engagement metrics
    /// </summary>
    /// <param name="timeWindowHours">Time window in hours for trending calculation</param>
    /// <param name="maxTopics">Maximum number of trending topics to maintain</param>
    /// <returns>Success status and message</returns>
    public async Task<(bool Success, string Message)> CalculateTrendingTopicsAsync(int timeWindowHours = 24, int maxTopics = 20)
    {
        SqlConnection? con = null;
        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@TimeWindowHours", timeWindowHours },
                { "@MaxTopics", maxTopics }
            };

            var cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_TrendingTopics_Calculate", con, paramDic);
            var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var status = reader.GetString("Status");
                var message = reader.GetString("Message");
                return (status == "SUCCESS", message);
            }

            return (false, "No response from calculation procedure");
        }
        catch (Exception ex)
        {
            return (false, $"Error calculating trending topics: {ex.Message}");
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Gets trending topics grouped by categories
    /// </summary>
    /// <param name="topicsPerCategory">Number of topics per category</param>
    /// <returns>List of trending topics grouped by category</returns>
    public async Task<List<TrendingTopic>> GetTrendingTopicsByCategoryAsync(int topicsPerCategory = 3)
    {
        var topics = new List<TrendingTopic>();
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@TopicsPerCategory", topicsPerCategory }
            };

            var cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_TrendingTopics_GetByCategory", con, paramDic);
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                topics.Add(new TrendingTopic
                {
                    TrendID = reader.GetInt32("TrendID"),
                    Topic = reader.GetString("Topic"),
                    Category = reader.GetString("Category"),
                    TrendScore = reader.GetDouble("TrendScore"),
                    TotalInteractions = reader.GetInt32("TotalInteractions"),
                    LastUpdated = reader.GetDateTime("LastUpdated")
                });
            }
        }
        catch (Exception)
        {
            // Return fallback data if procedure doesn't exist
            topics.AddRange(new[]
            {
                new TrendingTopic { TrendID = 1, Topic = "AI Innovation", Category = "Technology", TrendScore = 85.5, TotalInteractions = 150, LastUpdated = DateTime.Now },
                new TrendingTopic { TrendID = 2, Topic = "Climate Change", Category = "Environment", TrendScore = 78.2, TotalInteractions = 120, LastUpdated = DateTime.Now },
                new TrendingTopic { TrendID = 3, Topic = "Election 2024", Category = "Politics", TrendScore = 72.8, TotalInteractions = 98, LastUpdated = DateTime.Now }
            });
        }
        finally
        {
            con?.Close();
        }

        return topics;
    }

    /// <summary>
    /// Gets articles related to a specific trending topic
    /// </summary>
    /// <param name="topic">The trending topic</param>
    /// <param name="category">Category of the topic</param>
    /// <param name="count">Number of articles to return</param>
    /// <param name="currentUserId">Current user ID for context</param>
    /// <returns>List of related articles</returns>
    public async Task<List<NewsArticle>> GetTrendingTopicRelatedArticlesAsync(string topic, string category, int count = 10, int? currentUserId = null)
    {
        var articles = new List<NewsArticle>();
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@Topic", topic },
                { "@Category", category },
                { "@Count", count }
            };

            if (currentUserId.HasValue)
            {
                paramDic.Add("@CurrentUserID", currentUserId.Value);
            }

            var cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_TrendingTopics_GetRelatedArticles", con, paramDic);
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                articles.Add(new NewsArticle
                {
                    ArticleID = reader.GetInt32("ArticleID"),
                    Title = reader.GetString("Title"),
                    Content = reader.GetString("Content"),
                    ImageURL = reader.IsDBNull("ImageURL") ? null : reader.GetString("ImageURL"),
                    SourceURL = reader.IsDBNull("SourceURL") ? null : reader.GetString("SourceURL"),
                    SourceName = reader.IsDBNull("SourceName") ? null : reader.GetString("SourceName"),
                    PublishDate = reader.GetDateTime("PublishDate"),
                    Category = reader.GetString("Category"),
                    UserID = reader.GetInt32("UserID"),
                    Username = reader.GetString("Username"),
                    UserProfilePicture = reader.IsDBNull("UserProfilePicture") ? null : reader.GetString("UserProfilePicture"),
                    LikesCount = reader.GetInt32("LikesCount"),
                    ViewsCount = reader.GetInt32("ViewsCount"),
                    IsLiked = reader.GetInt32("IsLiked") == 1,
                    IsSaved = reader.GetInt32("IsSaved") == 1
                });
            }
        }
        catch (Exception)
        {
            // Return empty list if procedure doesn't exist
        }
        finally
        {
            con?.Close();
        }

        return articles;
    }

    /// <summary>
    /// Cleans up old trending topics
    /// </summary>
    /// <param name="maxAgeHours">Maximum age in hours for trending topics</param>
    /// <returns>Number of deleted topics and message</returns>
    public async Task<(int DeletedCount, string Message)> CleanupOldTrendingTopicsAsync(int maxAgeHours = 24)
    {
        SqlConnection? con = null;
        try
        {
            con = connect("myProjDB");
            var paramDic = new Dictionary<string, object>
            {
                { "@MaxAgeHours", maxAgeHours }
            };

            var cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_TrendingTopics_Cleanup", con, paramDic);
            var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var deletedCount = reader.GetInt32("DeletedCount");
                var message = reader.GetString("Message");
                return (deletedCount, message);
            }

            return (0, "No response from cleanup procedure");
        }
        catch (Exception ex)
        {
            return (0, $"Error during cleanup: {ex.Message}");
        }
        finally
        {
            con?.Close();
        }
    }

    public async Task<List<NewsArticle>> GetArticlesByInterestAsync(int userId, string category, int count = 10)
    {
        var articles = new List<NewsArticle>();
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            var sql = @"
                SELECT TOP (@Count) na.ArticleID, na.Title, na.Content, na.ImageURL, na.SourceURL, 
                       na.SourceName, na.Category, na.PublishDate, na.UserID, u.Name as Username,
                       COALESCE(lc.LikesCount, 0) as LikesCount,
                       COALESCE(vc.ViewsCount, 0) as ViewsCount
                FROM NewsArticles na
                INNER JOIN Users_News u ON na.UserID = u.ID
                LEFT JOIN (
                    SELECT ArticleID, COUNT(*) as LikesCount
                    FROM ArticleLikes
                    GROUP BY ArticleID
                ) lc ON na.ArticleID = lc.ArticleID
                LEFT JOIN (
                    SELECT ArticleID, COUNT(*) as ViewsCount
                    FROM ArticleViews
                    GROUP BY ArticleID
                ) vc ON na.ArticleID = vc.ArticleID
                WHERE na.Category = @Category
                  AND na.PublishDate >= DATEADD(day, -30, GETDATE())
                ORDER BY 
                    (COALESCE(lc.LikesCount, 0) * 2 + COALESCE(vc.ViewsCount, 0)) DESC,
                    na.PublishDate DESC";

            var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Count", count);
            cmd.Parameters.AddWithValue("@Category", category);
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                articles.Add(new NewsArticle
                {
                    ArticleID = reader.GetInt32("ArticleID"),
                    Title = reader["Title"]?.ToString(),
                    Content = reader["Content"]?.ToString(),
                    ImageURL = reader["ImageURL"]?.ToString(),
                    SourceURL = reader["SourceURL"]?.ToString(),
                    SourceName = reader["SourceName"]?.ToString(),
                    Category = reader["Category"]?.ToString(),
                    PublishDate = reader.GetDateTime("PublishDate"),
                    UserID = reader.GetInt32("UserID"),
                    Username = reader["Username"]?.ToString(),
                    LikesCount = reader.GetInt32("LikesCount"),
                    ViewsCount = reader.GetInt32("ViewsCount"),
                    IsLiked = await CheckUserLikedArticleAsync(reader.GetInt32("ArticleID"), userId),
                    IsSaved = await CheckUserSavedArticleAsync(reader.GetInt32("ArticleID"), userId)
                });
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            con?.Close();
        }

        return articles;
    }

    public async Task<FeedConfiguration?> GetUserFeedConfigurationAsync(int userId)
    {
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            var sql = @"
                SELECT UserID, PersonalizationWeight, FreshnessWeight, PopularityWeight, 
                       SerendipityWeight, MaxArticlesPerFeed, PreferredCategories, 
                       ExcludedCategories, LastUpdated
                FROM NewsSitePro2025_FeedConfigurations
                WHERE UserID = @UserID";

            var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@UserID", userId);
            var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new FeedConfiguration
                {
                    UserID = reader.GetInt32("UserID"),
                    PersonalizationWeight = reader.GetDouble("PersonalizationWeight"),
                    FreshnessWeight = reader.GetDouble("FreshnessWeight"),
                    PopularityWeight = reader.GetDouble("PopularityWeight"),
                    SerendipityWeight = reader.GetDouble("SerendipityWeight"),
                    MaxArticlesPerFeed = reader.GetInt32("MaxArticlesPerFeed"),
                    PreferredCategories = reader.GetString("PreferredCategories")?.Split(',').ToList() ?? new List<string>(),
                    ExcludedCategories = reader.GetString("ExcludedCategories")?.Split(',').ToList() ?? new List<string>(),
                    LastUpdated = reader.GetDateTime("LastUpdated")
                };
            }
        }
        catch (Exception)
        {
            // Return default configuration if table doesn't exist
        }
        finally
        {
            con?.Close();
        }

        // Return default configuration
        return new FeedConfiguration
        {
            UserID = userId,
            PersonalizationWeight = 0.4,
            FreshnessWeight = 0.3,
            PopularityWeight = 0.2,
            SerendipityWeight = 0.1,
            MaxArticlesPerFeed = 20,
            PreferredCategories = new List<string>(),
            ExcludedCategories = new List<string>(),
            LastUpdated = DateTime.Now
        };
    }

    public async Task<bool> UpdateUserFeedConfigurationAsync(FeedConfiguration config)
    {
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            var preferredCategories = string.Join(",", config.PreferredCategories ?? new List<string>());
            var excludedCategories = string.Join(",", config.ExcludedCategories ?? new List<string>());

            var sql = @"
                MERGE NewsSitePro2025_FeedConfigurations AS target
                USING (VALUES (@UserID, @PersonalizationWeight, @FreshnessWeight, @PopularityWeight, 
                              @SerendipityWeight, @MaxArticlesPerFeed, @PreferredCategories, @ExcludedCategories)) 
                AS source (UserID, PersonalizationWeight, FreshnessWeight, PopularityWeight, 
                          SerendipityWeight, MaxArticlesPerFeed, PreferredCategories, ExcludedCategories)
                ON target.UserID = source.UserID
                WHEN MATCHED THEN
                    UPDATE SET PersonalizationWeight = source.PersonalizationWeight,
                              FreshnessWeight = source.FreshnessWeight,
                              PopularityWeight = source.PopularityWeight,
                              SerendipityWeight = source.SerendipityWeight,
                              MaxArticlesPerFeed = source.MaxArticlesPerFeed,
                              PreferredCategories = source.PreferredCategories,
                              ExcludedCategories = source.ExcludedCategories,
                              LastUpdated = GETDATE()
                WHEN NOT MATCHED THEN
                    INSERT (UserID, PersonalizationWeight, FreshnessWeight, PopularityWeight, 
                           SerendipityWeight, MaxArticlesPerFeed, PreferredCategories, 
                           ExcludedCategories, LastUpdated)
                    VALUES (source.UserID, source.PersonalizationWeight, source.FreshnessWeight, 
                           source.PopularityWeight, source.SerendipityWeight, source.MaxArticlesPerFeed,
                           source.PreferredCategories, source.ExcludedCategories, GETDATE());";

            var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@UserID", config.UserID);
            cmd.Parameters.AddWithValue("@PersonalizationWeight", config.PersonalizationWeight);
            cmd.Parameters.AddWithValue("@FreshnessWeight", config.FreshnessWeight);
            cmd.Parameters.AddWithValue("@PopularityWeight", config.PopularityWeight);
            cmd.Parameters.AddWithValue("@SerendipityWeight", config.SerendipityWeight);
            cmd.Parameters.AddWithValue("@MaxArticlesPerFeed", config.MaxArticlesPerFeed);
            cmd.Parameters.AddWithValue("@PreferredCategories", preferredCategories);
            cmd.Parameters.AddWithValue("@ExcludedCategories", excludedCategories);

            await cmd.ExecuteNonQueryAsync();
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

    private async Task<bool> CheckUserLikedArticleAsync(int articleId, int userId)
    {
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            var cmd = new SqlCommand("SELECT COUNT(*) FROM NewsSitePro2025_ArticleLikes WHERE ArticleID = @ArticleID AND UserID = @UserID", con);
            cmd.Parameters.AddWithValue("@ArticleID", articleId);
            cmd.Parameters.AddWithValue("@UserID", userId);
            
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
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

    private async Task<bool> CheckUserSavedArticleAsync(int articleId, int userId)
    {
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            var cmd = new SqlCommand("SELECT COUNT(*) FROM NewsSitePro2025_SavedArticles WHERE ArticleID = @ArticleID AND UserID = @UserID", con);
            cmd.Parameters.AddWithValue("@ArticleID", articleId);
            cmd.Parameters.AddWithValue("@UserID", userId);
            
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
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

    // Additional methods needed by RecommendationService
    public async Task<NewsArticle?> GetNewsArticleByIdAsync(int articleId)
    {
        return await GetNewsArticleById(articleId);
    }

    //--------------------------------------------------------------------------------------------------
    // User Activity Methods
    //--------------------------------------------------------------------------------------------------
    
    public async Task<List<UserActivityItem>> GetUserRecentActivityAsync(int userId, int pageNumber = 1, int pageSize = 20)
    {
        var activities = new List<UserActivityItem>();
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                ["@UserID"] = userId,
                ["@PageNumber"] = pageNumber,
                ["@PageSize"] = pageSize
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_UserActivity_GetRecent", con, paramDic);
            SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                activities.Add(new UserActivityItem
                {
                    ActivityType = reader["ActivityType"]?.ToString() ?? "",
                    ArticleID = reader.GetInt32("ArticleID"),
                    ActivityDate = reader.GetDateTime("ActivityDate"),
                    Title = reader["Title"]?.ToString() ?? "",
                    Category = reader["Category"]?.ToString() ?? "",
                    ImageURL = reader["ImageURL"]?.ToString(),
                    SourceName = reader["SourceName"]?.ToString() ?? "",
                    Username = reader["Username"]?.ToString() ?? ""
                });
            }
        }
        catch (Exception ex)
        {
            // If stored procedure doesn't exist, use fallback query
            try
            {
                if (con != null)
                {
                    con.Close();
                    con = connect("myProjDB");
                }

                string sql = @"
                SELECT ActivityType, ArticleID, ActivityDate, Title, Category, ImageURL, SourceName, Username
                FROM (
                    -- Liked Articles
                    SELECT 
                        'liked' as ActivityType,
                        na.ArticleID,
                        al.CreatedAt as ActivityDate,
                        na.Title,
                        na.Category,
                        na.ImageURL,
                        na.SourceName,
                        u.Username as Username
                    FROM NewsSitePro2025_ArticleLikes al
                    INNER JOIN NewsSitePro2025_NewsArticles na ON al.ArticleID = na.ArticleID
                    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
                    WHERE al.UserID = @UserID
                    
                    UNION ALL
                    
                    -- Commented Articles
                    SELECT DISTINCT
                        'commented' as ActivityType,
                        na.ArticleID,
                        MAX(c.CreatedAt) as ActivityDate,
                        na.Title,
                        na.Category,
                        na.ImageURL,
                        na.SourceName,
                        u.Username as Username
                    FROM NewsSitePro2025_Comments c
                    INNER JOIN NewsSitePro2025_NewsArticles na ON c.PostID = na.ArticleID
                    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
                    WHERE c.UserID = @UserID AND c.IsDeleted = 0
                    GROUP BY na.ArticleID, na.Title, na.Category, na.ImageURL, na.SourceName, u.Username
                ) as Activities
                ORDER BY ActivityDate DESC
                OFFSET (@PageNumber - 1) * @PageSize ROWS
                FETCH NEXT @PageSize ROWS ONLY";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    activities.Add(new UserActivityItem
                    {
                        ActivityType = reader["ActivityType"]?.ToString() ?? "",
                        ArticleID = reader.GetInt32("ArticleID"),
                        ActivityDate = reader.GetDateTime("ActivityDate"),
                        Title = reader["Title"]?.ToString() ?? "",
                        Category = reader["Category"]?.ToString() ?? "",
                        ImageURL = reader["ImageURL"]?.ToString(),
                        SourceName = reader["SourceName"]?.ToString() ?? "",
                        Username = reader["Username"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception)
            {
                // If there's an error with fallback, return empty list
                activities = new List<UserActivityItem>();
            }
        }
        finally
        {
            con?.Close();
        }

        return activities;
    }

    public async Task<List<NewsArticle>> SearchSavedArticlesAsync(int userId, string? searchTerm = null, string? category = null, int pageNumber = 1, int pageSize = 10)
    {
        var articles = new List<NewsArticle>();
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                ["@UserID"] = userId,
                ["@SearchTerm"] = string.IsNullOrEmpty(searchTerm) ? (object)DBNull.Value : searchTerm,
                ["@Category"] = string.IsNullOrEmpty(category) ? (object)DBNull.Value : category,
                ["@PageNumber"] = pageNumber,
                ["@PageSize"] = pageSize
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_SavedArticles_Search", con, paramDic);
            SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                articles.Add(new NewsArticle
                {
                    ArticleID = reader.GetInt32("ArticleID"),
                    Title = reader["Title"]?.ToString(),
                    Content = reader["Content"]?.ToString(),
                    ImageURL = reader["ImageURL"]?.ToString(),
                    SourceURL = reader["SourceURL"]?.ToString(),
                    SourceName = reader["SourceName"]?.ToString(),
                    Category = reader["Category"]?.ToString(),
                    PublishDate = reader.GetDateTime("PublishDate"),
                    UserID = reader.GetInt32("UserID"),
                    Username = reader["Username"]?.ToString(),
                    LikesCount = reader.GetInt32("LikesCount"),
                    ViewsCount = reader.GetInt32("ViewsCount"),
                    IsLiked = reader.GetInt32("IsLiked") == 1,
                    IsSaved = reader.GetInt32("IsSaved") == 1
                });
            }
        }
        catch (Exception)
        {
            // If stored procedure doesn't exist, use fallback query
            try
            {
                if (con != null)
                {
                    con.Close();
                    con = connect("myProjDB");
                }

                string sql = @"
                    SELECT na.ArticleID, na.Title, na.Content, na.ImageURL, na.SourceURL, 
                           na.SourceName, na.Category, na.PublishDate, na.UserID, u.Username,
                           COALESCE(lc.LikesCount, 0) as LikesCount,
                           COALESCE(vc.ViewsCount, 0) as ViewsCount,
                           CASE WHEN al.UserID IS NOT NULL THEN 1 ELSE 0 END as IsLiked,
                           1 as IsSaved
                    FROM NewsSitePro2025_SavedArticles sa
                    INNER JOIN NewsSitePro2025_NewsArticles na ON sa.ArticleID = na.ArticleID
                    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
                    LEFT JOIN (
                        SELECT ArticleID, COUNT(*) as LikesCount
                        FROM NewsSitePro2025_ArticleLikes
                        GROUP BY ArticleID
                    ) lc ON na.ArticleID = lc.ArticleID
                    LEFT JOIN (
                        SELECT ArticleID, COUNT(*) as ViewsCount
                        FROM NewsSitePro2025_ArticleViews
                        GROUP BY ArticleID
                    ) vc ON na.ArticleID = vc.ArticleID
                    LEFT JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID AND al.UserID = @UserID
                    WHERE sa.UserID = @UserID
                      AND (@SearchTerm IS NULL OR na.Title LIKE '%' + @SearchTerm + '%' OR na.Content LIKE '%' + @SearchTerm + '%')
                      AND (@Category IS NULL OR na.Category = @Category)
                    ORDER BY sa.SavedAt DESC
                    OFFSET (@PageNumber - 1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@SearchTerm", string.IsNullOrEmpty(searchTerm) ? (object)DBNull.Value : $"%{searchTerm}%");
                cmd.Parameters.AddWithValue("@Category", string.IsNullOrEmpty(category) ? (object)DBNull.Value : category);
                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    articles.Add(new NewsArticle
                    {
                        ArticleID = reader.GetInt32("ArticleID"),
                        Title = reader["Title"]?.ToString(),
                        Content = reader["Content"]?.ToString(),
                        ImageURL = reader["ImageURL"]?.ToString(),
                        SourceURL = reader["SourceURL"]?.ToString(),
                        SourceName = reader["SourceName"]?.ToString(),
                        Category = reader["Category"]?.ToString(),
                        PublishDate = reader.GetDateTime("PublishDate"),
                        UserID = reader.GetInt32("UserID"),
                        Username = reader["Username"]?.ToString(),
                        LikesCount = reader.GetInt32("LikesCount"),
                        ViewsCount = reader.GetInt32("ViewsCount"),
                        IsLiked = reader.GetInt32("IsLiked") == 1,
                        IsSaved = true
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        finally
        {
            con?.Close();
        }

        return articles;
    }

    public async Task<List<NewsArticle>> GetSavedArticlesWithOptionsAsync(int userId, string displayType = "grid", string sortBy = "SavedAt", string sortOrder = "DESC", int pageNumber = 1, int pageSize = 12)
    {
        var articles = new List<NewsArticle>();
        SqlConnection? con = null;

        try
        {
            con = connect("myProjDB");
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                ["@UserID"] = userId,
                ["@DisplayType"] = displayType,
                ["@SortBy"] = sortBy,
                ["@SortOrder"] = sortOrder,
                ["@PageNumber"] = pageNumber,
                ["@PageSize"] = pageSize
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_SavedArticles_GetWithOptions", con, paramDic);
            SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                articles.Add(new NewsArticle
                {
                    ArticleID = reader.GetInt32("ArticleID"),
                    Title = reader["Title"]?.ToString(),
                    Content = reader["Content"]?.ToString(),
                    ImageURL = reader["ImageURL"]?.ToString(),
                    SourceURL = reader["SourceURL"]?.ToString(),
                    SourceName = reader["SourceName"]?.ToString(),
                    Category = reader["Category"]?.ToString(),
                    PublishDate = reader.GetDateTime("PublishDate"),
                    UserID = reader.GetInt32("UserID"),
                    Username = reader["Username"]?.ToString(),
                    LikesCount = reader.GetInt32("LikesCount"),
                    ViewsCount = reader.GetInt32("ViewsCount"),
                    IsLiked = reader.GetInt32("IsLiked") == 1,
                    IsSaved = reader.GetInt32("IsSaved") == 1
                });
            }
        }
        catch (Exception)
        {
            // Fallback to regular saved articles method
            articles = await GetSavedArticlesByUser(userId, pageNumber, pageSize);
        }
        finally
        {
            con?.Close();
        }

        return articles;
    }

    public async Task<int> GetSavedArticlesCountAsync(int userId, string? searchTerm = null, string? category = null)
    {
        SqlConnection? con = null;
        int count = 0;

        try
        {
            con = connect("myProjDB");
            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                ["@UserID"] = userId,
                ["@SearchTerm"] = string.IsNullOrEmpty(searchTerm) ? (object)DBNull.Value : searchTerm,
                ["@Category"] = string.IsNullOrEmpty(category) ? (object)DBNull.Value : category
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("NewsSitePro2025_sp_SavedArticles_GetCount", con, paramDic);
            object result = await cmd.ExecuteScalarAsync();
            count = result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception)
        {
            // Fallback query
            try
            {
                if (con != null)
                {
                    con.Close();
                    con = connect("myProjDB");
                }

                string sql = @"
                    SELECT COUNT(*) 
                    FROM NewsSitePro2025_SavedArticles sa
                    INNER JOIN NewsSitePro2025_NewsArticles na ON sa.ArticleID = na.ArticleID
                    WHERE sa.UserID = @UserID
                      AND (@SearchTerm IS NULL OR na.Title LIKE '%' + @SearchTerm + '%' OR na.Content LIKE '%' + @SearchTerm + '%')
                      AND (@Category IS NULL OR na.Category = @Category)";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@SearchTerm", string.IsNullOrEmpty(searchTerm) ? (object)DBNull.Value : $"%{searchTerm}%");
                cmd.Parameters.AddWithValue("@Category", string.IsNullOrEmpty(category) ? (object)DBNull.Value : category);

                object result = await cmd.ExecuteScalarAsync();
                count = result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception)
            {
                count = 0;
            }
        }
        finally
        {
            con?.Close();
        }

        return count;
    }

    public async Task<bool> RecordArticleInteractionAsync(int userId, int articleId, string interactionType)
    {
        return await RecordUserInteractionAsync(userId, articleId, interactionType);
    }

    public async Task<List<NewsArticle>> GetTrendingArticlesAsync(int count = 20)
    {
        return await GetRecommendedArticlesAsync(1, count); // Use recommended as fallback
    }

    public async Task<bool> CreateUserFeedConfigurationAsync(FeedConfiguration config)
    {
        return await UpdateUserFeedConfigurationAsync(config);
    }

    public async Task<List<NewsArticle>> GetSimilarArticlesAsync(int articleId, int count = 10)
    {
        // Simplified - get articles from same category
        var article = await GetNewsArticleById(articleId);
        if (article == null) return new List<NewsArticle>();
        
        return await GetArticlesByInterestAsync(1, article.Category ?? "", count);
    }

  

    public async Task<List<ArticleInteraction>> GetUserInteractionHistoryAsync(int userId, int days = 30)
    {
        // Return empty list for now - can be implemented later
        return new List<ArticleInteraction>();
    }

    public async Task<bool> ResetUserInterestsAsync(int userId)
    {
        SqlConnection? con = null;
        try
        {
            con = connect("myProjDB");
            var cmd = new SqlCommand("DELETE FROM NewsSitePro2025_UserInterests WHERE UserID = @UserID", con);
            cmd.Parameters.AddWithValue("@UserID", userId);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            con?.Close();
        }
    }

    public async Task<List<NewsArticle>> GetRecentHighEngagementArticlesAsync(int count = 10)
    {
        return await GetRecommendedArticlesAsync(1, count);
    }

    public async Task<bool> SaveTrendingTopicsAsync(List<TrendingTopic> topics)
    {
        // For now, just return true - can implement actual saving later
        return true;
    }

    /// <summary>
    /// Gets the list of user IDs that are followed by the specified user
    /// </summary>
    /// <param name="userId">The user ID to get followers for</param>
    /// <returns>List of user IDs that follow the specified user</returns>
    public async Task<List<int>> GetFollowedUserIdsAsync(int userId)
    {
        var followerIds = new List<int>();
        
        try
        {
            using (var con = connect("myProjDB"))
            {
                await con.OpenAsync();
                
                var query = @"
                    SELECT FollowerUserID 
                    FROM NewsSitePro2025_UserFollows 
                    WHERE FollowedUserID = @UserId";
                
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            followerIds.Add(reader.GetInt32("FollowerUserID"));
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // Return empty list if there's an error (table might not exist yet)
        }
        
        return followerIds;
    }

    public async Task<List<NewsArticle>> GetFollowingFeedAsync(int userId, int count = 20)
    {
        // Return regular feed for now
        return await GetRecommendedArticlesAsync(userId, count);
    }

    public async Task<List<NewsArticle>> GetPopularArticlesAsync(int count = 20)
    {
        return await GetRecommendedArticlesAsync(1, count);
    }

    public async Task<List<NewsArticle>> GetMostLikedArticlesAsync(int count = 20)
    {
        return await GetRecommendedArticlesAsync(1, count);
    }

    public async Task<List<NewsArticle>> GetMostViewedArticlesAsync(int count = 20)
    {
        return await GetRecommendedArticlesAsync(1, count);
    }

    public async Task<List<NewsArticle>> GetRecentArticlesAsync(int count = 20)
    {
        return GetAllNewsArticles(1, count);
    }

    public async Task<List<NewsArticle>> GetRandomQualityArticlesAsync(int count = 10)
    {
        return await GetRecommendedArticlesAsync(1, count);
    }

    #region Repost Methods

    /// <summary>
    /// Creates a new repost for an article
    /// </summary>
    /// <param name="request">Repost creation request</param>
    /// <returns>ID of created repost or 0 if failed</returns>
    public async Task<int> CreateRepostAsync(CreateRepostRequest request)
    {
        SqlConnection? con = null;
        
        try
        {
            con = new SqlConnection(connectionString);
            await con.OpenAsync();

            // Try stored procedure first
            try
            {
                var paramDic = new Dictionary<string, object>
                {
                    {"@OriginalArticleID", request.OriginalArticleID},
                    {"@UserID", request.UserID},
                    {"@AdditionalContent", request.RepostText ?? ""}
                };

                using var cmd = CreateCommandWithStoredProcedureGeneral("sp_Reposts_Create", con, paramDic);
                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (SqlException ex) when (ex.Number == 2812) // Stored procedure not found
            {
                // Fallback to direct SQL
                const string sql = @"
                    INSERT INTO NewsSitePro2025_Reposts (OriginalArticleID, UserID, AdditionalContent, CreatedDate, IsDeleted)
                    VALUES (@OriginalArticleID, @UserID, @AdditionalContent, GETDATE(), 0);
                    SELECT SCOPE_IDENTITY();";

                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@OriginalArticleID", request.OriginalArticleID);
                cmd.Parameters.AddWithValue("@UserID", request.UserID);
                cmd.Parameters.AddWithValue("@AdditionalContent", request.RepostText ?? "");

                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
        catch (Exception)
        {
            return 0;
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Gets reposts for a user's feed
    /// </summary>
    /// <param name="userId">ID of the user requesting the feed</param>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Number of reposts per page</param>
    /// <returns>List of reposts for the user's feed</returns>
    public async Task<List<Repost>> GetUserFeedRepostsAsync(int userId, int pageNumber = 1, int pageSize = 10)
    {
        SqlConnection? con = null;
        var reposts = new List<Repost>();
        
        try
        {
            con = new SqlConnection(connectionString);
            await con.OpenAsync();

            // Try stored procedure first
            try
            {
                var paramDic = new Dictionary<string, object>
                {
                    {"@UserID", userId},
                    {"@PageNumber", pageNumber},
                    {"@PageSize", pageSize}
                };

                using var cmd = CreateCommandWithStoredProcedureGeneral("sp_Reposts_GetUserFeed", con, paramDic);
                using var reader = await cmd.ExecuteReaderAsync();
                
                return await ReadRepostsFromReaderAsync(reader);
            }
            catch (SqlException ex) when (ex.Number == 2812) // Stored procedure not found
            {
                // Fallback to direct SQL
                var offset = (pageNumber - 1) * pageSize;
                const string sql = @"
                    SELECT r.RepostID, r.OriginalArticleID, r.UserID, r.AdditionalContent, r.CreatedDate,
                           u.Username, u.FirstName, u.LastName,
                           a.Title, a.Content, a.ImageUrl, a.CreatedDate as ArticleCreatedDate
                    FROM NewsSitePro2025_Reposts r
                    INNER JOIN NewsSitePro2025_Users u ON r.UserID = u.UserID
                    INNER JOIN NewsSitePro2025_NewsArticles a ON r.OriginalArticleID = a.NewsId
                    WHERE r.IsDeleted = 0 AND a.IsDeleted = 0
                    ORDER BY r.CreatedDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Offset", offset);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using var reader = await cmd.ExecuteReaderAsync();
                return await ReadRepostsFromReaderAsync(reader);
            }
        }
        catch (Exception)
        {
            return reposts;
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Gets reposts created by a specific user
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Number of reposts per page</param>
    /// <returns>List of reposts by the user</returns>
    public async Task<List<Repost>> GetUserRepostsAsync(int userId, int pageNumber = 1, int pageSize = 10)
    {
        SqlConnection? con = null;
        var reposts = new List<Repost>();
        
        try
        {
            con = new SqlConnection(connectionString);
            await con.OpenAsync();

            var offset = (pageNumber - 1) * pageSize;
            const string sql = @"
                SELECT r.RepostID, r.OriginalArticleID, r.UserID, r.AdditionalContent, r.CreatedDate,
                       u.Username, u.FirstName, u.LastName,
                       a.Title, a.Content, a.ImageUrl, a.CreatedDate as ArticleCreatedDate
                FROM NewsSitePro2025_Reposts r
                INNER JOIN NewsSitePro2025_Users u ON r.UserID = u.UserID
                INNER JOIN NewsSitePro2025_NewsArticles a ON r.OriginalArticleID = a.NewsId
                WHERE r.UserID = @UserID AND r.IsDeleted = 0 AND a.IsDeleted = 0
                ORDER BY r.CreatedDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync();
            return await ReadRepostsFromReaderAsync(reader);
        }
        catch (Exception)
        {
            return reposts;
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Deletes a repost (soft delete)
    /// </summary>
    /// <param name="repostId">ID of the repost to delete</param>
    /// <param name="userId">ID of the user requesting deletion</param>
    /// <returns>True if successful</returns>
    public async Task<bool> DeleteRepostAsync(int repostId, int userId)
    {
        SqlConnection? con = null;
        
        try
        {
            con = new SqlConnection(connectionString);
            await con.OpenAsync();

            // Try stored procedure first
            try
            {
                var paramDic = new Dictionary<string, object>
                {
                    {"@RepostID", repostId},
                    {"@UserID", userId}
                };

                using var cmd = CreateCommandWithStoredProcedureGeneral("sp_Reposts_Delete", con, paramDic);
                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (SqlException ex) when (ex.Number == 2812) // Stored procedure not found
            {
                // Fallback to direct SQL
                const string sql = @"
                    UPDATE NewsSitePro2025_Reposts 
                    SET IsDeleted = 1, DeletedDate = GETDATE()
                    WHERE RepostID = @RepostID AND UserID = @UserID AND IsDeleted = 0";

                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@RepostID", repostId);
                cmd.Parameters.AddWithValue("@UserID", userId);

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
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

    /// <summary>
    /// Toggles like on a repost
    /// </summary>
    /// <param name="repostId">ID of the repost</param>
    /// <param name="userId">ID of the user</param>
    /// <returns>Result of the like toggle operation</returns>
    public async Task<RepostResult> ToggleRepostLikeAsync(int repostId, int userId)
    {
        SqlConnection? con = null;
        
        try
        {
            con = new SqlConnection(connectionString);
            await con.OpenAsync();

            // Try stored procedure first
            try
            {
                var paramDic = new Dictionary<string, object>
                {
                    {"@RepostID", repostId},
                    {"@UserID", userId}
                };

                using var cmd = CreateCommandWithStoredProcedureGeneral("sp_RepostLikes_Toggle", con, paramDic);
                using var reader = await cmd.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return new RepostResult
                    {
                        Success = true,
                        Message = "Like toggled successfully",
                        IsLiked = reader["IsLiked"] != DBNull.Value ? (bool)reader["IsLiked"] : false
                    };
                }
                
                return new RepostResult { Success = false, Message = "Failed to toggle like" };
            }
            catch (SqlException ex) when (ex.Number == 2812) // Stored procedure not found
            {
                // Fallback to direct SQL - check if like exists
                const string checkSql = "SELECT COUNT(*) FROM NewsSitePro2025_RepostLikes WHERE RepostID = @RepostID AND UserID = @UserID";
                using var checkCmd = new SqlCommand(checkSql, con);
                checkCmd.Parameters.AddWithValue("@RepostID", repostId);
                checkCmd.Parameters.AddWithValue("@UserID", userId);

                var likeExists = (int)await checkCmd.ExecuteScalarAsync() > 0;

                if (likeExists)
                {
                    // Remove like
                    const string deleteSql = "DELETE FROM NewsSitePro2025_RepostLikes WHERE RepostID = @RepostID AND UserID = @UserID";
                    using var deleteCmd = new SqlCommand(deleteSql, con);
                    deleteCmd.Parameters.AddWithValue("@RepostID", repostId);
                    deleteCmd.Parameters.AddWithValue("@UserID", userId);
                    await deleteCmd.ExecuteNonQueryAsync();

                    return new RepostResult { Success = true, Message = "Like removed", IsLiked = false };
                }
                else
                {
                    // Add like
                    const string insertSql = "INSERT INTO NewsSitePro2025_RepostLikes (RepostID, UserID, CreatedDate) VALUES (@RepostID, @UserID, GETDATE())";
                    using var insertCmd = new SqlCommand(insertSql, con);
                    insertCmd.Parameters.AddWithValue("@RepostID", repostId);
                    insertCmd.Parameters.AddWithValue("@UserID", userId);
                    await insertCmd.ExecuteNonQueryAsync();

                    return new RepostResult { Success = true, Message = "Like added", IsLiked = true };
                }
            }
        }
        catch (Exception)
        {
            return new RepostResult { Success = false, Message = "An error occurred" };
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Gets like count for a repost
    /// </summary>
    /// <param name="repostId">ID of the repost</param>
    /// <returns>Number of likes on the repost</returns>
    public async Task<int> GetRepostLikeCountAsync(int repostId)
    {
        SqlConnection? con = null;
        
        try
        {
            con = new SqlConnection(connectionString);
            await con.OpenAsync();

            const string sql = "SELECT COUNT(*) FROM NewsSitePro2025_RepostLikes WHERE RepostID = @RepostID";
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@RepostID", repostId);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception)
        {
            return 0;
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Checks if a user has liked a specific repost
    /// </summary>
    /// <param name="repostId">ID of the repost</param>
    /// <param name="userId">ID of the user</param>
    /// <returns>True if user has liked the repost</returns>
    public async Task<bool> HasUserLikedRepostAsync(int repostId, int userId)
    {
        SqlConnection? con = null;
        
        try
        {
            con = new SqlConnection(connectionString);
            await con.OpenAsync();

            const string sql = "SELECT COUNT(*) FROM NewsSitePro2025_RepostLikes WHERE RepostID = @RepostID AND UserID = @UserID";
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@RepostID", repostId);
            cmd.Parameters.AddWithValue("@UserID", userId);

            var result = await cmd.ExecuteScalarAsync();
            return result != null && Convert.ToInt32(result) > 0;
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

    /// <summary>
    /// Creates a new comment on a repost
    /// </summary>
    /// <param name="request">Comment creation request</param>
    /// <returns>ID of created comment or 0 if failed</returns>
    public async Task<int> CreateRepostCommentAsync(CreateRepostCommentRequest request)
    {
        SqlConnection? con = null;
        
        try
        {
            con = new SqlConnection(connectionString);
            await con.OpenAsync();

            // Try stored procedure first
            try
            {
                var paramDic = new Dictionary<string, object>
                {
                    {"@RepostID", request.RepostID},
                    {"@UserID", request.UserID},
                    {"@Content", request.Content},
                    {"@ParentCommentID", request.ParentCommentID ?? (object)DBNull.Value}
                };

                using var cmd = CreateCommandWithStoredProcedureGeneral("sp_RepostComments_Create", con, paramDic);
                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (SqlException ex) when (ex.Number == 2812) // Stored procedure not found
            {
                // Fallback to direct SQL
                const string sql = @"
                    INSERT INTO NewsSitePro2025_RepostComments (RepostID, UserID, Content, ParentCommentID, CreatedDate, IsDeleted)
                    VALUES (@RepostID, @UserID, @Content, @ParentCommentID, GETDATE(), 0);
                    SELECT SCOPE_IDENTITY();";

                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@RepostID", request.RepostID);
                cmd.Parameters.AddWithValue("@UserID", request.UserID);
                cmd.Parameters.AddWithValue("@Content", request.Content);
                cmd.Parameters.AddWithValue("@ParentCommentID", request.ParentCommentID ?? (object)DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
        catch (Exception)
        {
            return 0;
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Gets comments for a specific repost
    /// </summary>
    /// <param name="repostId">ID of the repost</param>
    /// <returns>List of comments for the repost</returns>
    public async Task<List<RepostComment>> GetRepostCommentsAsync(int repostId)
    {
        SqlConnection? con = null;
        var comments = new List<RepostComment>();
        
        try
        {
            con = new SqlConnection(connectionString);
            await con.OpenAsync();

            const string sql = @"
                SELECT c.CommentID, c.RepostID, c.UserID, c.Content, c.ParentCommentID, c.CreatedDate,
                       u.Username, u.FirstName, u.LastName
                FROM NewsSitePro2025_RepostComments c
                INNER JOIN NewsSitePro2025_Users u ON c.UserID = u.UserID
                WHERE c.RepostID = @RepostID AND c.IsDeleted = 0
                ORDER BY c.CreatedDate ASC";

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@RepostID", repostId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                comments.Add(new RepostComment
                {
                    RepostCommentID = (int)reader["CommentID"],
                    RepostID = (int)reader["RepostID"],
                    UserID = (int)reader["UserID"],
                    Content = reader["Content"].ToString(),
                    ParentCommentID = reader["ParentCommentID"] != DBNull.Value ? (int?)reader["ParentCommentID"] : null,
                    CreatedAt = (DateTime)reader["CreatedDate"],
                    Username = reader["Username"].ToString(),
                    UserName = $"{reader["FirstName"]} {reader["LastName"]}"
                });
            }

            return comments;
        }
        catch (Exception)
        {
            return comments;
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Gets comment count for a repost
    /// </summary>
    /// <param name="repostId">ID of the repost</param>
    /// <returns>Number of comments on the repost</returns>
    public async Task<int> GetRepostCommentCountAsync(int repostId)
    {
        SqlConnection? con = null;
        
        try
        {
            con = new SqlConnection(connectionString);
            await con.OpenAsync();

            const string sql = "SELECT COUNT(*) FROM NewsSitePro2025_RepostComments WHERE RepostID = @RepostID AND IsDeleted = 0";
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@RepostID", repostId);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception)
        {
            return 0;
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Helper method to read reposts from SqlDataReader
    /// </summary>
    /// <param name="reader">SqlDataReader containing repost data</param>
    /// <returns>List of reposts</returns>
    private async Task<List<Repost>> ReadRepostsFromReaderAsync(SqlDataReader reader)
    {
        var reposts = new List<Repost>();
        
        while (await reader.ReadAsync())
        {
            reposts.Add(new Repost
            {
                RepostID = (int)reader["RepostID"],
                OriginalArticleID = (int)reader["OriginalArticleID"],
                UserID = (int)reader["UserID"],
                RepostText = reader["AdditionalContent"].ToString(),
                CreatedAt = (DateTime)reader["CreatedDate"],
                RepostAuthor = reader["Username"].ToString(),
                RepostAuthorName = $"{reader["FirstName"]} {reader["LastName"]}",
                OriginalArticle = new NewsArticle
                {
                    ArticleID = (int)reader["OriginalArticleID"],
                    Title = reader["Title"].ToString(),
                    Content = reader["Content"].ToString(),
                    ImageURL = reader["ImageUrl"].ToString(),
                    PublishDate = (DateTime)reader["ArticleCreatedDate"]
                }
            });
        }

        return reposts;
    }

    /// <summary>
    /// Gets the author ID of a repost
    /// </summary>
    /// <param name="repostId">ID of the repost</param>
    /// <returns>User ID of the repost author</returns>
    public async Task<int> GetRepostAuthorIdAsync(int repostId)
    {
        SqlConnection? con = null;
        
        try
        {
            con = new SqlConnection(connectionString);
            await con.OpenAsync();

            const string sql = "SELECT UserID FROM NewsSitePro2025_Reposts WHERE RepostID = @RepostID AND IsDeleted = 0";
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@RepostID", repostId);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception)
        {
            return 0;
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Gets the author ID of an article
    /// </summary>
    /// <param name="articleId">ID of the article</param>
    /// <returns>User ID of the article author</returns>
    public async Task<int> GetArticleAuthorIdAsync(int articleId)
    {
        SqlConnection? con = null;
        
        try
        {
            con = new SqlConnection(connectionString);
            await con.OpenAsync();

            const string sql = "SELECT AuthorId FROM NewsSitePro2025_NewsArticles WHERE NewsId = @ArticleId AND IsDeleted = 0";
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@ArticleId", articleId);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch (Exception)
        {
            return 0;
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Gets the title of an article
    /// </summary>
    /// <param name="articleId">ID of the article</param>
    /// <returns>Title of the article</returns>
    public async Task<string> GetArticleTitleAsync(int articleId)
    {
        SqlConnection? con = null;
        
        try
        {
            con = new SqlConnection(connectionString);
            await con.OpenAsync();

            const string sql = "SELECT Title FROM NewsSitePro2025_NewsArticles WHERE NewsId = @ArticleId AND IsDeleted = 0";
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@ArticleId", articleId);

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? "";
        }
        catch (Exception)
        {
            return "";
        }
        finally
        {
            con?.Close();
        }
    }

    #region Repost Additional Methods

    /// <summary>
    /// Gets a repost by its ID
    /// </summary>
    /// <param name="repostId">ID of the repost</param>
    /// <returns>Repost object or null if not found</returns>
    public async Task<Repost?> GetRepostByIdAsync(int repostId)
    {
        SqlConnection? con = null;
        try
        {
            con = connect("NewsSiteDB");
            const string sql = @"
                SELECT r.RepostID, r.UserID, r.ArticleID as OriginalArticleID, r.AdditionalContent as RepostText, r.CreatedDate as CreatedAt, r.IsDeleted,
                       u.Name as UserName, u.Username,
                       a.Title as OriginalTitle, a.Content as OriginalContent, a.ImageURL as OriginalImageURL
                FROM NewsSitePro2025_Reposts r
                INNER JOIN NewsSitePro2025_Users u ON r.UserID = u.UserID
                INNER JOIN NewsSitePro2025_NewsArticles a ON r.ArticleID = a.ArticleID
                WHERE r.RepostID = @RepostID AND r.IsDeleted = 0";

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@RepostID", repostId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Repost
                {
                    RepostID = reader.GetInt32("RepostID"),
                    UserID = reader.GetInt32("UserID"),
                    OriginalArticleID = reader.GetInt32("OriginalArticleID"),
                    RepostText = reader.IsDBNull("RepostText") ? null : reader.GetString("RepostText"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    IsDeleted = reader.GetBoolean("IsDeleted"),
                    RepostAuthor = reader.IsDBNull("Username") ? null : reader.GetString("Username"),
                    RepostAuthorName = reader.IsDBNull("UserName") ? null : reader.GetString("UserName")
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting repost by ID: {ex.Message}");
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Gets comments for a specific repost (alternative method name)
    /// </summary>
    /// <param name="repostId">ID of the repost</param>
    /// <returns>List of repost comments</returns>
    public async Task<List<RepostComment>> GetCommentsByRepostId(int repostId)
    {
        return await GetRepostCommentsAsync(repostId);
    }

    /// <summary>
    /// Gets a specific repost comment by ID
    /// </summary>
    /// <param name="commentId">ID of the comment</param>
    /// <returns>RepostComment or null if not found</returns>
    public async Task<RepostComment?> GetRepostCommentById(int commentId)
    {
        SqlConnection? con = null;
        try
        {
            con = connect("NewsSiteDB");
            const string sql = @"
                SELECT c.CommentID as RepostCommentID, c.RepostID, c.UserID, c.Content, c.ParentCommentID, c.CreatedDate as CreatedAt,
                       u.Name as CommentAuthorName, u.Username as CommentAuthor
                FROM NewsSitePro2025_RepostComments c
                INNER JOIN NewsSitePro2025_Users u ON c.UserID = u.UserID
                WHERE c.CommentID = @CommentID AND c.IsDeleted = 0";

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@CommentID", commentId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RepostComment
                {
                    RepostCommentID = reader.GetInt32("RepostCommentID"),
                    RepostID = reader.GetInt32("RepostID"),
                    UserID = reader.GetInt32("UserID"),
                    Content = reader.GetString("Content"),
                    ParentCommentID = reader.IsDBNull("ParentCommentID") ? null : reader.GetInt32("ParentCommentID"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    UserName = reader.IsDBNull("CommentAuthorName") ? null : reader.GetString("CommentAuthorName"),
                    Username = reader.IsDBNull("CommentAuthor") ? null : reader.GetString("CommentAuthor")
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting repost comment by ID: {ex.Message}");
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Updates a repost comment content
    /// </summary>
    /// <param name="commentId">ID of the comment to update</param>
    /// <param name="content">New content</param>
    /// <returns>True if successful</returns>
    public async Task<bool> UpdateRepostComment(int commentId, string content)
    {
        SqlConnection? con = null;
        try
        {
            con = connect("NewsSiteDB");
            const string sql = @"
                UPDATE NewsSitePro2025_RepostComments 
                SET Content = @Content, UpdatedDate = GETDATE()
                WHERE CommentID = @CommentID AND IsDeleted = 0";

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@CommentID", commentId);
            cmd.Parameters.AddWithValue("@Content", content);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating repost comment: {ex.Message}");
        }
        finally
        {
            con?.Close();
        }
    }

    /// <summary>
    /// Soft deletes a repost comment
    /// </summary>
    /// <param name="commentId">ID of the comment to delete</param>
    /// <returns>True if successful</returns>
    public async Task<bool> DeleteRepostComment(int commentId)
    {
        SqlConnection? con = null;
        try
        {
            con = connect("NewsSiteDB");
            const string sql = @"
                UPDATE NewsSitePro2025_RepostComments 
                SET IsDeleted = 1, UpdatedDate = GETDATE()
                WHERE CommentID = @CommentID";

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@CommentID", commentId);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting repost comment: {ex.Message}");
        }
        finally
        {
            con?.Close();
        }
    }

    #endregion

    #endregion

}