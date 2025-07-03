using NewsSite.BL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace NewsSite.DAL
{
    public class SharedArticleRepository
    {
        private readonly DBservices db;

        public SharedArticleRepository()
        {
            db = new DBservices();
        }

        public List<News> GetSharedArticles()
        {
            List<News> sharedArticles = new List<News>();
            SqlConnection con = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;

            try
            {
                con = db.connect("myProjDB");

                cmd = db.CreateCommandWithStoredProcedureGeneral("sp_GetSharedArticles", con, null);

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    News article = new News
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Title = reader["Title"]?.ToString(),
                        Content = reader["Content"]?.ToString(),
                        Tag = reader["Tag"]?.ToString(),
                        AuthorId = Convert.ToInt32(reader["AuthorId"])
                    };

                    sharedArticles.Add(article);
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

            return sharedArticles;
        }
    }
}
