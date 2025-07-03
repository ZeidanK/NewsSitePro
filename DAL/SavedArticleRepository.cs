using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using NewsSite.BL;

namespace NewsSite.DAL
{
    public class SavedArticleRepository
    {
        private readonly DBservices _dbServices;

        public SavedArticleRepository()
        {
            _dbServices = new DBservices();
        }

        public bool SaveArticle(int userId, int articleId)
        {
            SqlConnection con = null;
            SqlCommand cmd = null;

            try
            {
                con = _dbServices.connect("myProjDB");

                var paramDic = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@ArticleId", articleId }
                };

                cmd = CreateCommand("sp_SaveArticle", con, paramDic);
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

        public List<int> GetSavedArticles(int userId)
        {
            SqlConnection con = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            List<int> savedArticleIds = new List<int>();

            try
            {
                con = _dbServices.connect("myProjDB");

                var paramDic = new Dictionary<string, object>
                {
                    { "@UserId", userId }
                };

                cmd = CreateCommand("sp_GetSavedArticles", con, paramDic);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    savedArticleIds.Add(Convert.ToInt32(reader["ArticleId"]));
                }

                return savedArticleIds;
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

        private SqlCommand CreateCommand(string spName, SqlConnection con, Dictionary<string, object> paramDic)
        {
            return typeof(DBservices)
                .GetMethod("CreateCommandWithStoredProcedureGeneral", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_dbServices, new object[] { spName, con, paramDic }) as SqlCommand;
        }
    }
}
