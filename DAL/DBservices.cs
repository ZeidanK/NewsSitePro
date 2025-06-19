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
                { "@id", id.HasValue ? id.Value : DBNull.Value },
                { "@name", string.IsNullOrEmpty(name) ? DBNull.Value : name },
                { "@Email", string.IsNullOrEmpty(email) ? DBNull.Value : email }
            };

            cmd = CreateCommandWithStoredProcedureGeneral("sp_Users2025Pro_Get", con, paramDic); // create the command  

            reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                user = new User
                {
                    // Map to your User class properties
                    Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                    Name = reader["name"]?.ToString(),
                    Email = reader["email"]?.ToString(),
                    PasswordHash = reader["passwordHash"]?.ToString(),
                    IsAdmin = reader["isAdmin"] != DBNull.Value && Convert.ToBoolean(reader["isAdmin"]),
                    IsLocked = reader["isLocked"] != DBNull.Value && Convert.ToBoolean(reader["isLocked"])
                };
            }
        }
        catch (Exception ex)
        {
            // Optionally log the exception here
            throw;
        }
        finally
        {
            reader?.Close();
            con?.Close();
        }

        return user;
    }


    //--------------------------------------------------------------------------------------------------
// This method creates a new user in the database using a stored procedure
//--------------------------------------------------------------------------------------------------
public bool CreateUser(User user)
{
    SqlConnection con = null;
    SqlCommand cmd = null;
    try
    {
        con = connect("myProjDB");
        var paramDic = new Dictionary<string, object>
        {
            { "@name", user.Name },
            { "@Email", user.Email },
            { "@passwordHash", user.PasswordHash },
            { "@isAdmin", user.IsAdmin },
            { "@isLocked", user.IsLocked }
        };

        cmd = CreateCommandWithStoredProcedureGeneral("sp_Users2025Pro_Insert", con, paramDic);
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
// This method retrieves a user by their ID using a stored procedure
//--------------------------------------------------------------------------------------------------
public User GetUserById(int id)
{
    return GetUser(null, id, null);
}

//--------------------------------------------------------------------------------------------------
// This method updates user details in the database using a stored procedure
//--------------------------------------------------------------------------------------------------
public bool UpdateUser(User user)
{
    SqlConnection con = null;
    SqlCommand cmd = null;
    try
    {
        con = connect("myProjDB");
        var paramDic = new Dictionary<string, object>
        {
            { "@id", user.Id },
            { "@name", user.Name },
            { "@passwordHash", user.PasswordHash },
            { "@isAdmin", user.IsAdmin },
            { "@isLocked", user.IsLocked }
        };

        cmd = CreateCommandWithStoredProcedureGeneral("sp_Users2025Pro_Update", con, paramDic);
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
    //public User GetUser(string username=null,string email=null)

}