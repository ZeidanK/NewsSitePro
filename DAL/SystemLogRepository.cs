using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using NewsSite.BL;

namespace NewsSite.DAL
{
    public class SystemLogRepository
    {
        private readonly DBservices db;

        public SystemLogRepository()
        {
            db = new DBservices();
        }

        public bool LogEvent(string message, string level, int? userId = null)
        {
            SqlConnection con = null;
            SqlCommand cmd = null;

            try
            {
                con = db.connect("myProjDB");

                var paramDic = new Dictionary<string, object>
                {
                    { "@Message", message },
                    { "@Level", level },
                    { "@UserID", userId.HasValue ? userId.Value : DBNull.Value }
                };

                cmd = db.CreateCommandWithStoredProcedureGeneral("sp_LogEvent", con, paramDic);

                int affectedRows = cmd.ExecuteNonQuery();
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
    }
}
