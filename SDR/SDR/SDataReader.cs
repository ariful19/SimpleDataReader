using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SDR
{
    public class SDataReader : IDisposable, IEnumerable<Dictionary<string, object>>
    {
        SqlConnection conn;
        SqlCommand cmd;
        public SDataReader(string sql, string connectionString)
        {
            conn = new SqlConnection(connectionString);
            cmd = new SqlCommand(sql, conn);
            conn.Open();
        }
        public SDataReader(string sql, SqlConnection conn)
        {
            this.conn = conn;
            cmd = new SqlCommand(sql, this.conn);
            this.conn.Open();
        }

        public SDataReader(SqlConnection conn)
        {
            this.conn = conn;
            this.conn.Open();
        }
        public SDataReader(string connectionString)
        {
            this.conn = new SqlConnection(connectionString);
            conn.Open();
        }

        public SDataReader(SqlConnection conn, string sql, params object[] parameters)
        {
            this.conn = conn;
            cmd = new SqlCommand(sql, this.conn);
            var matches = Regex.Matches(sql, "@[\\w]+");
            for (int i = 0; i < matches.Count; i++)
            {
                cmd.Parameters.AddWithValue(matches[i].Value, parameters[i]);
            }
            this.conn.Open();
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            cmd = new SqlCommand(sql, this.conn);
            var matches = Regex.Matches(sql, "@[\\w]+");
            for (int i = 0; i < matches.Count; i++)
            {
                cmd.Parameters.AddWithValue(matches[i].Value, parameters[i]);
            }
            return cmd.ExecuteNonQuery();
        }

        public IEnumerable<Dictionary<string, object>> ExecuteQuery(string sql, params object[] parameters)
        {
            cmd = new SqlCommand(sql, this.conn);
            var matches = Regex.Matches(sql, "@[\\w]+");
            for (int i = 0; i < matches.Count; i++)
            {
                cmd.Parameters.AddWithValue(matches[i].Value, parameters[i]);
            }
            SqlDataReader rdr = cmd.ExecuteReader();
            var cnt = rdr.FieldCount;
            while (rdr.Read())
            {
                var dic = new Dictionary<string, object>();
                for (int i = 0; i < cnt; i++)
                {
                    dic[rdr.GetName(i)] = rdr.GetValue(i);
                }
                yield return dic;
            }
            rdr.Close();
        }

        public IEnumerable<dynamic> GetAsDynamicObjects()
        {
            SqlDataReader rdr = cmd.ExecuteReader();
            var cnt = rdr.FieldCount;
            while (rdr.Read())
            {
                var o = new ExpandoObject() as IDictionary<string, object>;
                for (int i = 0; i < cnt; i++)
                {
                    o.Add(rdr.GetName(i), rdr.GetValue(i));
                }
                yield return o as ExpandoObject;
            }
            rdr.Close();
        }

        public void Dispose()
        {
            conn.Close();
        }

        public IEnumerable<Dictionary<string, object>> Result
        {
            get
            {
                SqlDataReader rdr = cmd.ExecuteReader();
                var cnt = rdr.FieldCount;
                while (rdr.Read())
                {
                    var dic = new Dictionary<string, object>();
                    for (int i = 0; i < cnt; i++)
                    {
                        dic[rdr.GetName(i)] = rdr.GetValue(i);
                    }
                    yield return dic;
                }
                rdr.Close();
            }
        }

        public IEnumerator<Dictionary<string, object>> GetEnumerator()
        {
            return Result.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Result.GetEnumerator();
        }

        public DataTable ToDataTable()
        {
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public bool HasData
        {
            get
            {
                var dr = cmd.ExecuteReader();
                var hasrow = dr.HasRows;
                dr.Close();
                return hasrow;
            }
        }
    }
}
