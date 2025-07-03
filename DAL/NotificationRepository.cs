using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using NewsSite.BL;

namespace NewsSite.DAL
{
    public class NotificationRepository
    {
        private readonly DBservices db;

        public NotificationRepository()
        {
            db = new DBservices();
        }

        public bool AddNotification(int userId, string message)
        {
            SqlConnection con = null;
            SqlCommand cmd = null;

            try
            {
                con = db.connect("myProjDB");

                var paramDic = new Dictionary<string, object>
                {
                    { "@UserID", userId },
                    { "@Message", message },
                    { "@CreatedAt", DateTime.Now },
                    { "@IsRead", false }
                };

                cmd = db.CreateCommandWithStoredProcedureGeneral("sp_Notifications_Insert", con, paramDic);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding notification: " + ex.Message);
                return false;
            }
            finally
            {
                con?.Close();
            }
        }

        public List<Notification> GetNotificationsForUser(int userId)
        {
            List<Notification> notifications = new List<Notification>();
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

                cmd = db.CreateCommandWithStoredProcedureGeneral("sp_Notifications_GetByUser", con, paramDic);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    notifications.Add(new Notification
                    {
                        Id = Convert.ToInt32(reader["ID"]),
                        UserId = Convert.ToInt32(reader["UserID"]),
                        Message = reader["Message"]?.ToString(),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                        IsRead = Convert.ToBoolean(reader["IsRead"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching notifications: " + ex.Message);
            }
            finally
            {
                reader?.Close();
                con?.Close();
            }

            return notifications;
        }

        public bool MarkAsRead(int notificationId)
        {
            SqlConnection con = null;
            SqlCommand cmd = null;

            try
            {
                con = db.connect("myProjDB");

                var paramDic = new Dictionary<string, object>
                {
                    { "@ID", notificationId }
                };

                cmd = db.CreateCommandWithStoredProcedureGeneral("sp_Notifications_MarkAsRead", con, paramDic);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error marking notification as read: " + ex.Message);
                return false;
            }
            finally
            {
                con?.Close();
            }
        }
    }
}
