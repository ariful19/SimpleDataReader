using SDR.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testConsole
{
    class Program
    {
        static SqlConnection sc = new SqlConnection("Data Source=.\\sqlexpress;Initial Catalog=tempdb;Integrated Security=True");
        static void Main(string[] args)
        {
            using (var dr = new SDataReader(sc, "select * from Company"))
            {
                var oList = dr.AutoMap<Company>().ToList();
                foreach (var item in oList)
                {
                    Console.WriteLine(item.EstDate);
                }
            }
            Console.ReadKey();
        }
    }
    class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public DateTime EstDate { get; set; }
    }
}
