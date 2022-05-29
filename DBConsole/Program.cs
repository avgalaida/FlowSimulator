using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Xml;
using System.Xml.Linq;

namespace DBConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = String.Format("Server={0};Port={1};" +
                                                    "User Id={2}; Password={3};Database={4};",
                                                    "localhost", 5432, "postgres",
                                                    "sqwes", "testDB");

            NpgsqlConnection conn = null;

            conn = new NpgsqlConnection(connectionString);

            conn.Open();

            //string sql = @"select * from table1";
            //NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            //DataTable dt = new DataTable();
            //dt.Load(cmd.ExecuteReader());
            //Console.WriteLine(dt.Rows[2][0]);

            string xmlFilePath = XDocument.Load("docs/AbstractProject.xml").ToString();
            string fileName = "AbstractProject";
            string sqlQuery = string.Format(@"insert into table1 (name,file) values ('{0}', '{1}')", fileName, xmlFilePath);
            //string sqlQuery = string.Format(@"insert into table1 (name,val) values ('t2', 'value2')");
            // string sqlQuery = string.Format(@"select val from table1 where name = 't2'");
            NpgsqlCommand cmd = new NpgsqlCommand(sqlQuery, conn);
            var res = cmd.ExecuteScalar();
            conn.Close();
            Console.WriteLine(res);
            //XDocument xdoc = new XDocument();
            //// создаем первый элемент person
            //XElement tom = new XElement("person");
            //// создаем атрибут name
            //XAttribute tomNameAttr = new XAttribute("name", "Tom");
            //// создаем два элемента company и age 
            //XElement tomCompanyElem = new XElement("company", "Microsoft");
            //XElement tomAgeElem = new XElement("age", 37);
            //// добавляем атрибут и элементы в первый элемент person
            //tom.Add(tomNameAttr);
            //tom.Add(tomCompanyElem);
            //tom.Add(tomAgeElem);

            //// создаем корневой элемент
            //XElement people = new XElement("people");

            //people.Add(tom);
            //// добавляем корневой элемент в документ
            //xdoc.Add(people);
            ////сохраняем документ
            //xdoc.Save("docs/people.xml");
        }
    }
}
