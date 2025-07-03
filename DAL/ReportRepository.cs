using NewsSite.BL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace NewsSite.DAL
{
    public class ReportRepository
    {
        private readonly DBservices db;

        public ReportRepository()
        {
            db = new DBservices();
        }

        public bool SubmitReport(int userId, int articleId, string reason)
        {
            SqlConnection con = null;
            SqlCommand cmd = null;

            try
            {
                con = db.connect("myProjDB");

                var paramDic = new Dictionary<string, object>
                {
                    { "@UserID", userId },
                    { "@ArticleID", articleId },
                    { "@Reason", reason },
                    { "@ReportedAt", DateTime.Now }
                };

                cmd = db.CreateCommandWithStoredProcedureGeneral("sp_Reports_Insert", con, paramDic);
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
      
        public List<Report> GetAllReports()
        {
            List<Report> reports = new List<Report>();
            SqlConnection con = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;

            try
            {
                con = db.connect("myProjDB");
                cmd = db.CreateCommandWithStoredProcedureGeneral("sp_Reports_GetAll", con, null);

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    reports.Add(new Report
                    {
                        ReportID = Convert.ToInt32(reader["ReportID"]),
                        UserID = Convert.ToInt32(reader["UserID"]),
                        ArticleID = Convert.ToInt32(reader["ArticleID"]),
                        Reason = reader["Reason"]?.ToString(),
                        ReportedAt = Convert.ToDateTime(reader["ReportedAt"])
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

            return reports;
        }
    }
}
