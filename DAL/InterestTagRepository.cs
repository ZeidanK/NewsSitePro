using NewsSite.BL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace NewsSite.DAL
{
    public class InterestTagRepository
    {
        private readonly DBservices db;

        public InterestTagRepository()
        {
            db = new DBservices();
        }

        public bool AddInterestTag(int userId, string tag)
        {
            SqlConnection con = null;
            SqlCommand cmd = null;

            try
            {
                con = db.connect("myProjDB");

                var paramDic = new Dictionary<string, object>
                {
                    { "@UserID", userId },
                    { "@Tag", tag }
                };

                cmd = db.CreateCommandWithStoredProcedureGeneral("sp_UserTags_Insert", con, paramDic);
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding interest tag: " + ex.Message);
                return false;
            }
            finally
            {
                con?.Close();
            }
        }

        public List<string> GetInterestTags(int userId)
        {
            List<string> tags = new List<string>();
            SqlConnection con = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;

            try
            {
                con = db.connect("myProjDB");

                var paramDic = new Dictionary<string, object>
                {
                    { "@UserID", userId }
                };

                cmd = db.CreateCommandWithStoredProcedureGeneral("sp_UserTags_GetByUser", con, paramDic);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tags.Add(reader["Tag"]?.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving interest tags: " + ex.Message);
            }
            finally
            {
                reader?.Close();
                con?.Close();
            }

            return tags;
        }
    }
}
