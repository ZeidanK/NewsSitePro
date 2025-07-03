using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using NewsSite.BL;

namespace NewsSite.DAL
{
    public class SavedArticleRepository
    {
        private readonly string connectionString = "Your_Connection_String_Here";

        public bool SaveArticle(int userId, int articleId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO SavedArticle (UserID, ArticleID, SavedDate) VALUES (@UserID, @ArticleID, @SavedDate)",
                    conn
                );
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@ArticleID", articleId);
                cmd.Parameters.AddWithValue("@SavedDate", DateTime.Now);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }

        public List<NewsArticle> GetSavedArticles(int userId)
        {
            List<NewsArticle> savedArticles = new List<NewsArticle>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT n.* 
                      FROM NewsArticle n 
                      INNER JOIN SavedArticle s ON s.ArticleID = n.ArticleID 
                      WHERE s.UserID = @UserID", conn);
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    savedArticles.Add(new NewsArticle
                    {
                        ArticleID = Convert.ToInt32(reader["ArticleID"]),
                        Title = reader["Title"].ToString(),
                        Content = reader["Content"].ToString(),
                        SourceURL = reader["SourceURL"].ToString(),
                        ImageURL = reader["ImageURL"].ToString(),
                        PublishDate = Convert.ToDateTime(reader["PublishDate"]),
                        Category = reader["Category"].ToString()
                    });
                }
            }

            return savedArticles;
        }

        public List<NewsArticle> SearchSavedArticles(int userId, string keyword)
        {
            List<NewsArticle> results = new List<NewsArticle>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT n.* 
                      FROM NewsArticle n 
                      INNER JOIN SavedArticle s ON s.ArticleID = n.ArticleID 
                      WHERE s.UserID = @UserID AND (n.Title LIKE @Keyword OR n.Content LIKE @Keyword)", conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new NewsArticle
                    {
                        ArticleID = Convert.ToInt32(reader["ArticleID"]),
                        Title = reader["Title"].ToString(),
                        Content = reader["Content"].ToString(),
                        SourceURL = reader["SourceURL"].ToString(),
                        ImageURL = reader["ImageURL"].ToString(),
                        PublishDate = Convert.ToDateTime(reader["PublishDate"]),
                        Category = reader["Category"].ToString()
                    });
                }
            }

            return results;
        }
    }
}
