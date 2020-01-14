/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CRL;

namespace CRLTest.Code
{
    public class TestAll
    {
        public static void TestSelect()
        {
            var time = DateTime.Now;
            //select 测试
            var query = ProductDataManage.Instance.GetLambdaQuery();

            query.Take(3);
            //query.Where(b => DateTime.Parse("2016-02-11 12:56") == b.AddTime);
            //query.Where(b => b.CategoryName != null);
            //query.Where(b => b.AddTime == time);
            var times = "2016-02-11 12:56";
            //query.Where(b => int.Parse(b.InterFaceUser) == 123);//支持Cast转换
            var result = query.Select(b => new
            {
                b.InterFaceUser,
                bb = b.Id * b.Number,
                b.ProductName,
                id1 = Convert.ToString(b.Id),
                id2 = b.Id.ToString(),
                name2 = b.ProductName.Substring(0, 2),
                time = time,
                aa = DateTime.Parse("2016-02-11 12:56"),
                aa2 = Convert.ToDateTime(times),
                times = times,
                field = "field"
            }).ToList();
            var sql = query.ToString();
            Console.WriteLine(sql);
        }
        public static void TestMethod()
        {
            //扩展方法测试
            //var instance = Code.ProductDataManage.Instance;
            //instance.QueryItem(1);
            var ids = new List<int>() { 1,2,3};
            var names = new List<string>() { "111","222"};
            var query = ProductDataManage.Instance.GetLambdaQuery();
            //query.Where(b => names.Contains(b.CategoryName));
            //query.Where(b => b.CategoryName.Contains(b.BarCode));
            //query.Where(b => b.ProductName.Contains("122"));
            //Console.WriteLine(query.ToString());
            //return;
     
            query.Where(b => b.TransType == TransType.In);
            var item = new ProductData();
            var year = DateTime.Now.Year;
            query.Where(b => b.Id == item.Id);
            query.Where(b => b.InterFaceUser == "222" && b.Id < 20);
            #region 扩展方法
            query.Where(b => ids.Contains(b.Id));
            query.Where(b => names.Contains(b.CategoryName));
            query.Where(b => b.IsTop);//没有运算符的bool一元运算
            query.Where(b => 0 < b.Id && b.IsTop);//不再区分左边右边了
            query.Where(b => b.Id < b.Number);//直接比较可以解析通过
            query.Where(b => b.ProductName.Contains("123"));//包含字符串
            query.Where(b => !b.ProductName.Contains("122"));//不包含字符串
            query.Where(b => b.CategoryName.Contains(b.BarCode));//支持属性调用了
            query.Where(b => b.ProductName.In("111", "222"));//string in
            query.Where(b => b.AddTime.Between(DateTime.Now, DateTime.Now));//在时间段内
            query.Where(b => b.AddTime.DateDiff(DatePart.dd, DateTime.Now) > 1);//时间比较
            query.Where(b => b.ProductName.Substring(0, 3) == "222");//截取字符串
            query.Where(b => b.Id.In(1, 2, 3));//in
            query.Where(b => !b.Id.In(1, 2, 3));//not in
            query.Where(b => b.UserId.Equals(Code.ProductChannel.其它));//按值等于,enum等于int
            query.Where(b => b.ProductName.StartsWith("abc"));//开头值判断
            query.Where(b => b.Id.Between(1, 10));//数字区间
            query.Where(b => b.ProductName.Like("123"));// %like%
            query.Where(b => b.ProductName.LikeLeft("123"));// %like
            query.Where(b => b.ProductName.LikeRight("123"));// like%
            query.Where(b => b.Id.ToString() == "123");//支持Cast转换
            query.Where(b => Convert.ToString(b.Id) == "123");//支持Cast转换
            query.Where(b => int.Parse(b.InterFaceUser) == 123);//支持Cast转换
            query.Where(b => b.ProductName.LessThan("sss"));
            query.Where(b => b.ProductName.GreaterThan("sss"));
            //query.Where(b => b.CategoryName != null);
            query.Page(2, 1);
            query.OrderBy(b => b.Id * 1);
            var result = query.ToList();
            var sql = query.PrintQuery();
            Console.WriteLine(sql);
            #endregion
        }
        public static void TestView()
        {
            //关联再关联子查询
            var q1 = Code.OrderManage.Instance.GetLambdaQuery();
            var q2 = q1.CreateQuery<Code.ProductData>();
            q2.Where(b => b.Id > 0);
            var view = q2.CreateQuery<Code.Member>().Where(b => b.Id > 0).GroupBy(b => b.Name).Select(b => new { b.Name, aa = b.Id.COUNT() });//GROUP查询
            var view2 = q2.Join(view, (a, b) => a.CategoryName == b.Name).Select((a, b) => new { ss1 = a.UserId, ss2 = b.aa });//关联GROUP
            q1.Join(view2, (a, b) => a.Id == b.ss1).Select((a, b) => new { a.Id, b.ss1 }).ToList();//再关联
            //q1.Join(view2, (a, b) => a.Id == b.ss1).SelectAppendValue(b => b.ss1);//存入索引
            //var result = view2.ToList();
            var sql = q1.ToString();
            Console.WriteLine(sql);
        }
        public static void TestJoin()
        {
            var query = ProductDataManage.Instance.GetLambdaQuery();
            query.Join<Member>((a, b) => a.Id == b.Id)
                .Join<Order>((a, b, c) => a.UserId == c.UserId).JoinAfter(b=>b.Status==1).Select((a, b, c) => new { b.Id });
            Console.WriteLine(query.ToString());
        }
        public static void TestJoin3()
        {
            //关联再GROUP
            var query = ProductDataManage.Instance.GetLambdaQuery();
            var join = query.Join<Code.Member>((a, b) => a.UserId == b.Id);
            join.GroupBy((a, b) => new { a.ProductName, b.Name });
            var result = join.Select((a, b) => new { a.ProductName, b.Name }).ToList();
            var sql = query.PrintQuery();
            Console.WriteLine(sql);
        }

        public static void TestIn()
        { 
            //按IN查询
            var query = ProductDataManage.Instance.GetLambdaQuery();
            var query2 = query.CreateQuery<Code.Member>();
            var view = query2.Where(b => b.Name == "123").Select(b => b.Id);
            var result = query.In(view, b => b.UserId).ToList();
            var sql = query.PrintQuery();
            Console.WriteLine(sql);
        }
        public static void TestIn2()
        {
            //按IN查询
            var query = ProductDataManage.Instance.GetLambdaQuery();
            query.In<Code.Member>(b => b.UserId, b => b.Id, (a, b) => a.BarCode == b.AccountNo && b.Name == "123");
            var result = query.ToList();
            var sql = query.PrintQuery();
            Console.WriteLine(sql);
        }

        public static void TestExists()
        {
            //按exists
            var query = ProductDataManage.Instance.GetLambdaQuery();
            var query2 = query.CreateQuery<Code.Member>();
            var view2 = query2.Where(b => b.Name == "123").Select(b => b.Id);
            var result = query.Exists(view2).ToList();
            var sql = query.PrintQuery();
            Console.WriteLine(sql);
        }
        public static void TestGroup()
        {
            #region GROUP
            var query = Code.ProductDataManage.Instance.GetLambdaQuery();
            //GROUP条件
            var group =query.Where(b => b.Id > 0).GroupBy(b => new { b.ProductName });

            //having
            group.GroupHaving(b => b.Number.SUM() >= 0);
            //设置排序
            group.OrderBy(b => b.BarCode.Count(), true);//等效为 order by count(BarCode) desc

            //选择GROUP字段
            var list4 = group.Select(b => new
            {
                sum2 = b.SUM(x => x.Number * x.Id),//等效为 sum(Number*Id) as sum2
                total = b.BarCode.COUNT(),//等效为count(BarCode) as total
                sum11 = b.Number.SUM(),//等效为sum(Number) as sum1
                b.ProductName,
                num1 = b.SUM(x => x.Number * x.Id),
                num2 = b.MAX(x => x.Number * x.Id),
                num3 = b.MIN(x => x.Number * x.Id),
                num4 = b.AVG(x => x.Number * x.Id)
            }).ToList();

            foreach (var item in list4)
            {
                var total = item.total;
            }
            var sql = query.PrintQuery();
            Console.WriteLine(sql);
            #endregion
        }
        public static void TestDistinct()
        {
            #region DISTINCT
            var query = Code.ProductDataManage.Instance.GetLambdaQuery();
            query.Where(b => b.Id > 0);
            var list5 = query.Select(b => new { dictinct = b.ProductName.DistinctField() }).ToList();
            Console.WriteLine(query.ToString());
            var distinctCount = query.Select(b => new { count = b.ProductName.DistinctCount() }).ToList();
            foreach (var item in list5)
            {
                //var total = item.Total;
                var name = item.dictinct;
            }
            Console.WriteLine(query.ToString());
            #endregion
        }
        public static void TestUnion()
        {
            //联合查询
            var query = Code.ProductDataManage.Instance.GetLambdaQuery().Where(b => b.Id < 200);
            var query2 = query.CreateQuery<Code.Order>().Where(b => b.Id < 200);
            var view1 = query.Select(b => new { a1 = b.Id, a2 = b.ProductName });
            var view2 = query2.Select(b => new { a1 = b.Id, a2 = b.Remark });
            var result = view1.Union(view2).OrderBy(b => b.a1).OrderBy(b => b.a2, false).ToList();
            var sql = query.PrintQuery();
            Console.WriteLine(sql);
        }
        public static void TestFunc()
        {
            #region 函数
            var instance = Code.ProductDataManage.Instance;
            //按条件id>0,合计Number列
            var sum = instance.Sum(b => b.Id > 0, b => b.Number * b.UserId);
            //按条件id>0,进行总计
            var count = instance.Count(b => b.Id > 0);
            var max = instance.Max(b => b.Id > 0, b => b.Id);
            var min = instance.Min(b => b.Id > 0, b => b.Id);
            //使用语句进行函数查询
            var query = ProductDataManage.Instance.GetLambdaQuery();
            query.Select(b => b.Number.SUM());
            decimal sum2 = query.ToScalar();
            #endregion
        }

        public static void TestFileMapping()
        {
            var query = Code.ProductDataManage.Instance.GetLambdaQuery();
            query.Select(b => new { name2 = b.ProductName, ss2 = b.PurchasePrice * b.Id });
            query.Where(b => b.Id > 0);
            var result = query.ToDynamic();
            Console.WriteLine(query.ToString());
            foreach (var d in result)
            {
                var a = d.name2;
                var c = d.ss2;
            }
            var query3 = Code.ProductDataManage.Instance.GetLambdaQuery();
            query3.Join<Code.Member>((a, b) => a.UserId == b.Id).SelectAppendValue((a, b) => b.Name);
            var resutl3 = query3.ToList();
            Console.WriteLine(query3.ToString());
            foreach (var d in resutl3)
            {
                var a = d.GetBag().Name;
                var a2 = d.GetIndexData("name");
            }
        }
        public static void TestAnonymousResult()
        {
            var instance = MemberManage.Instance;
            var result1 = instance.GetLambdaQuery().Select(b => new { b.Name }).ToList();
            var result2 = instance.GetLambdaQuery().Select(b => new Code.Member { Name = b.Name }).ToList();
            var join = instance.GetLambdaQuery().Join<Order>((a, b) => a.Id == b.UserId);
            var result3 = join.Select((a, b) => new { a.Name, b.UserId }).ToList();
            var result4 = join.Select((a, b) => new anony1 { Name = a.Name, UserId = b.UserId }).ToList();
        }
        public class anony1
        {
            public string Name
            {
                get;set;
            }
            public int UserId
            {
                get; set;
            }
        }
        public static void WhereNotNull()
        {
            var instance = MemberManage.Instance;
            var query = instance.GetLambdaQuery();
            string name = null;
            name = "111";
            query.WhereNotNull(b => b.Name == name);
            query.PrintQuery();
        }

        public static void TestUpdate()
        {
            var instance = Code.ProductDataManage.Instance;
            #region 更新
            //按匿名表达式
            instance.Update(b => b.Id == 1, b => new ProductData { Number = 2 });
            //按匿名对象
            instance.Update(b => b.Id == 1, new { Number = 2 });

            //要更新属性集合
            CRL.ParameCollection c = new CRL.ParameCollection();
            c["ProductName"] = "product1";
            Code.ProductDataManage.Instance.Update(b => b.Id == 4, c);
            //按对象差异更新
            var p = new Code.ProductData() { Id = 4 };
            //手动修改值时,指定修改属性以在Update时识别,分以下几种形式
            p.Change(b => b.BarCode);//表示值被更改了
            p.Change(b => b.BarCode, "123");//通过参数赋值
            p.Change(b => b.BarCode == "123");//通过表达式赋值
            Code.ProductDataManage.Instance.Update(b => b.Id == 4, p);//指定查询更新

            p = Code.ProductDataManage.Instance.QueryItem(b => b.Id > 0);
            p.UserId += 1;
            Code.ProductDataManage.Instance.Update(p);//按主键更新,主键值是必须的

            //关联更新
            var query = Code.OrderManage.Instance.GetLambdaQuery();
            query.Join<ProductData>((a, b) => a.Id == b.Id && b.Number > 10);
            c = new CRL.ParameCollection();
            c["UserId"] = "$UserId";//order.userid=product.userid
            c["Remark"] = "2222";//order.remark=2222
            Code.OrderManage.Instance.Update(query, c);

            //按拼接SQL更新多条
            var list = MemberManage.Instance.GetLambdaQuery().Take(3).ToList();
            list.ForEach(b =>
            {
                b.Name = DateTime.Now.ToString();
            });
            MemberManage.Instance.Update(list);

            #endregion

            #region 删除
            //关联删除
            var query2 = Code.ProductDataManage.Instance.GetLambdaQuery();
            query2.Where(b => b.Id == 10);
            query2.Join<Code.Member>((a, b) => a.SupplierId == "10" && b.Name == "123");
            Code.ProductDataManage.Instance.Delete(query2);
            Code.ProductDataManage.Instance.Delete(999);
            #endregion

            #region 缓存更新
            //按编号为1的数据
            var item = Code.ProductDataManage.Instance.QueryItemFromCache(b => b.Id > 0);
            item.CheckNull("item");
            var guid = Guid.NewGuid().ToString().Substring(0, 8);
            item.Change(b => b.SupplierName, guid);
            Code.ProductDataManage.Instance.Update(item);
            item = Code.ProductDataManage.Instance.QueryItemFromCache(item.Id);
            var item2 = Code.ProductDataManage.Instance.QueryItem(item.Id);
            var a2 = item.SupplierName == item2.SupplierName && item.SupplierName == guid;
            if (!a2)
            {
                throw new Exception("更新缓存失败");
            }
            #endregion

            #region 事务
            string error;
            item = Code.ProductDataManage.Instance.QueryItem(1);

            var result = Code.ProductDataManage.Instance.PackageTrans((out string ex) =>
            {
                ex = "";
                var product = new ProductData();
                product.BarCode = "sdfsdf";
                product.Number = 10;
                ProductDataManage.Instance.Add(product);
                return false;
            }, out error);
            if (result)
            {
                throw new Exception("事务未回滚");
            }
            #endregion
            Console.WriteLine("TestUpdate finish");
        }

        public class testC
        {
            public int? id
            {
                get;set;
            }
        }

    }
}
