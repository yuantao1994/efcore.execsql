using EFGetStarted;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Newtonsoft.Json;
namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new BloggingContext())
            {
                // 事务
                Tran(db);
                // 普通插入
                insert(db);
                // sql 查询
                query(db);
            }
            Console.ReadKey();
        }

        private static void Tran(BloggingContext db)
        {
            using (var tran = db.Database.BeginTransaction())
            {
                using (var conn2 = db.Database.GetDbConnection())
                {
                    conn2.Open();
                    var command2 = conn2.CreateCommand();
                    command2.CommandText = @"INSERT INTO Test(name) values (@name);";
                    var par = command2.CreateParameter();
                    par.ParameterName = "@name";
                    par.Value = "事务插入";
                    command2.Parameters.Add(par);
                    try
                    {
                        command2.ExecuteNonQuery();
                        Console.WriteLine("输入n触发回滚");
                        var str = Console.ReadKey();
                        if (str.KeyChar.Equals('n'))
                        {
                            throw new Exception("手动触发异常");
                        }
                        db.Add(new Blog { Url = "http://blogs.msdn.com/adonettest" }); // add blog
                        db.SaveChanges();
                        tran.Commit();
                        Console.WriteLine("事务插入成功");
                        Console.WriteLine("blog count="+db.Blogs.Count());
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        Console.WriteLine("回滚成功");
                    }
                }
            }
        }

        private static void insert(BloggingContext db)
        {
            //插入
            var conn2 = db.Database.GetDbConnection();
            conn2.Open();
            var command2 = conn2.CreateCommand();
            command2.CommandText = @"INSERT INTO Test(name) values (@name);";
            var par = command2.CreateParameter();
            par.ParameterName = "@name";
            par.Value = "普通插入";
            command2.Parameters.Add(par);
            command2.ExecuteNonQuery();
            conn2.Close();
            Console.WriteLine("普通插入成功");
        }

        private static void query(BloggingContext db)
        {
            // query datatable
            var conn = db.Database.GetDbConnection();
            conn.Open();
            var command = conn.CreateCommand();
            command.CommandText = "select * from Test;";
            var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            var dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            conn.Close();
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(dt));
        }
    }
}
