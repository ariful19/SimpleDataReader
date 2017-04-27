using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDR.SqlServer;
using System.Data.SqlClient;

namespace SDRTest
{
    [TestClass]
    public class UnitTest1
    {
        SqlConnection sc = new SqlConnection("Data Source=.\\sqlexpress;Initial Catalog=tempdb;Integrated Security=True");
        [TestMethod]
        public void TestObjectRetrival()
        {
            using (var dr = new SDataReader(sc, "select * from Company"))
            {
                var oList = dr.AutoMap<Company>().ToList();
                var dicList = dr.ToList();
                Assert.AreEqual(oList.First().Name, dicList.First()["Name"].ToString());
            }
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
