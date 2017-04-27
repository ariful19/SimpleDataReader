using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SDR.Oracle
{
    public class SDataReader : IDisposable, IEnumerable<Dictionary<string, object>>
    {
        OracleConnection conn;
        OracleCommand cmd;
        public SDataReader(string sql, string connectionString)
        {
            conn = new OracleConnection(connectionString);
            cmd = new OracleCommand(sql, conn);
            conn.Open();
        }
        public SDataReader(string sql, OracleConnection conn)
        {
            this.conn = conn;
            cmd = new OracleCommand(sql, this.conn);
            this.conn.Open();
        }

        public SDataReader(OracleConnection conn)
        {
            this.conn = conn;
            this.conn.Open();
        }
        public SDataReader(string connectionString)
        {
            this.conn = new OracleConnection(connectionString);
            conn.Open();
        }

        public SDataReader(OracleConnection conn, string sql, params object[] parameters)
        {
            this.conn = conn;
            cmd = new OracleCommand(sql, this.conn);
            var matches = Regex.Matches(sql, "(?<=:)[\\w]+");
            for (int i = 0; i < matches.Count; i++)
            {
                cmd.Parameters.Add(matches[i].Value, parameters[i]);
            }
            this.conn.Open();
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            cmd = new OracleCommand(sql, this.conn);
            var matches = Regex.Matches(sql, "(?<=:)[\\w]+");
            for (int i = 0; i < matches.Count; i++)
            {
                cmd.Parameters.Add(matches[i].Value, parameters[i]);
            }
            return cmd.ExecuteNonQuery();
        }
        // :userName
        public IEnumerable<Dictionary<string, object>> ExecuteQuery(string sql, params object[] parameters)
        {
            cmd = new OracleCommand(sql, this.conn);
            var matches = Regex.Matches(sql, "(?<=:)[\\w]+");
            for (int i = 0; i < matches.Count; i++)
            {
                cmd.Parameters.Add(matches[i].Value, parameters[i]);
            }
            OracleDataReader rdr = cmd.ExecuteReader();
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
            OracleDataReader rdr = cmd.ExecuteReader();
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
                OracleDataReader rdr = cmd.ExecuteReader();
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
            OracleDataAdapter da = new OracleDataAdapter(cmd);
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

        public IEnumerable<T> AutoMap<T>()
        {
            using (OracleDataReader rdr = cmd.ExecuteReader())
            {
                var cnt = rdr.FieldCount;
                var t = typeof(T);
                var fieldliest = Enumerable.Range(0, rdr.FieldCount)
                    .Select(o => rdr.GetName(o)).ToList();
                var proplist = t.GetProperties()
                    .Where(o => fieldliest.Any(f => f.ToLower() == o.Name.ToLower()))
                    .Select(o => new { Property = o, fieldIndex = fieldliest.IndexOf(o.Name) });
                while (rdr.Read())
                {
                    var x = Activator.CreateInstance(t);
                    foreach (var item in proplist)
                    {
                        item.Property.SetValue(x, rdr.GetValue(item.fieldIndex));
                    }
                    yield return (T)x;
                }

            }
        }
    }
}

