using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using NewsSite.BL;

namespace NewsSite.DAL
{
    public class SharedArticleRepository
    {
        private readonly DBservices db;

        public SharedArticleRepository()
        {
            db = new DBservices();
        }

        public bool ShareArticle(Article article, int userId)
        {
            SqlConnection con = null;
            SqlCommand cmd = null;

            try
            {
                con = db.connect("myProjDB");

                var paramDic = new Dictionary<string, object>
                {
                    { "@UserID", userId },
                    { "@Title", article.Title },
                    { "@Content", article.Content },
                    { "@Tag", article.Tag },
                    { "@SharedAt", DateTime.Now }
                };

                cmd = db.CreateCommandWithStoredProcedureGeneral("sp_SharedArticles_Insert", con, paramDic);
                int affectedRows = cmd.ExecuteNonQuery();
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while sharing article: " + ex.Message);
                return false;
            }
            finally
            {
                con?.Close();
            }
        }

        public List<Article> GetAllShared()
        {
            List<Article> sharedArticles = new List<Article>();
            SqlConnection con = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;

            try
            {
                con = db.connect("myProjDB");
                cmd = db.CreateCommandWithStoredProcedureGeneral("sp_SharedArticles_GetAll", con, null);

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sharedArticles.Add(new Article
                    {
                        Title = reader["Title"]?.ToString(),
                        Content = reader["Content"]?.ToString(),
                        Tag = reader["Tag"]?.ToString(),
                        SharedAt = reader["SharedAt"] != DBNull.Value ? Convert.ToDateTime(reader["SharedAt"]) : DateTime.MinValue
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching shared articles: " + ex.Message);
                throw;
            }
            finally
            {
                reader?.Close();
                con?.Close();
            }

            return sharedArticles;
        }
    }
}
