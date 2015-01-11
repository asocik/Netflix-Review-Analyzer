//
// Netflix Database Application using N-Tier Design. 
//
// Adam Socik
//

//
// Data Access Tier:  interface between business tier and data store.
//

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataAccessTier
{
  public class Data
  {
    //
    // Fields:
    //
    private string _DBFile;
    private string _DBConnectionInfo;

    //
    // Constructor:
    //
    public Data(string DatabaseFilename)
    {
      _DBFile = DatabaseFilename;
      _DBConnectionInfo = String.Format(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=|DataDirectory|\{0};Integrated Security=True;", DatabaseFilename);
    }

    //
    // TestConnection:  returns true if the database can be successfully opened and closed,
    // false if not.
    //
    public bool TestConnection()
    {
        SqlConnection db = new SqlConnection(_DBConnectionInfo);
        bool  state = false;

        try
        {
            db.Open();
            state = (db.State == ConnectionState.Open);
        }
        catch
        {
            // nothing, just discard:
        }
        finally
        {
            db.Close();
        }

        return state;
    }

    //
    // ExecuteScalarQuery:  executes a scalar Select query, returning the single result 
    // as an object.  
    //
    public object ExecuteScalarQuery(string sql)
    {
        SqlConnection dbConn = new SqlConnection(_DBConnectionInfo);
        dbConn.Open();

        object result;
        SqlCommand dbCmd = new SqlCommand();
        dbCmd.Connection = dbConn;
        dbCmd.CommandText = sql;
        result = dbCmd.ExecuteScalar();
        dbConn.Close();

        return result != null ? String.Format("{0}", result) : null;
    }

    // 
    // ExecuteNonScalarQuery:  executes a Select query that generates a temporary table,
    // returning this table in the form of a Dataset.
    //
    public DataSet ExecuteNonScalarQuery(string sql)
    {
        SqlConnection dbConn = new SqlConnection(_DBConnectionInfo);
        dbConn.Open();

        SqlCommand dbCmd = new SqlCommand();
        dbCmd.Connection = dbConn;
        SqlDataAdapter adapter = new SqlDataAdapter(dbCmd);
        DataSet ds = new DataSet();
        dbCmd.CommandText = sql;
        adapter.Fill(ds);
        dbConn.Close();

        // Check to see if the dataset is empty
        return (ds.Tables[0].Rows.Count == 0) ? null : ds;
    }

    //
    // ExecutionActionQuery:  executes an Insert, Update or Delete query, and returns
    // the number of records modified.
    //
    public int ExecuteActionQuery(string sql)
    {
        SqlConnection dbConn = new SqlConnection(_DBConnectionInfo);
        dbConn.Open();

        int result;
        SqlCommand dbCmd = new SqlCommand();
        dbCmd.Connection = dbConn;
        dbCmd.CommandText = sql;
        result = dbCmd.ExecuteNonQuery();
        dbConn.Close();

        return result;
    }

  }//class

}//namespace
