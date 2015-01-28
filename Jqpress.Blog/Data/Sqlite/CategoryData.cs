﻿using System;
using System.Collections.Generic;
using Jqpress.Blog.Entity;
using Mono.Data.Sqlite;
using System.Data;
using Jqpress.Framework.DbProvider.Sqlite;
using Jqpress.Framework.Configuration;
using Jqpress.Blog.Entity.Enum;

namespace Jqpress.Blog.Data.Sqlite
{
    public partial class DataProvider
    {
        /// <summary>
        /// 检查别名是否重复
        /// </summary>
        /// <param name="cate"></param>
        /// <returns></returns>
        private static void CheckSlug(CategoryInfo cate)
        {
            while (true)
            {
                string cmdText = cate.CategoryId == 0 ? string.Format("select count(1) from [{2}category] where [Slug]='{0}' and [type]={1}", cate.Slug, (int)CategoryType.Category,ConfigHelper.Tableprefix) : string.Format("select count(1) from [{3}category] where [Slug]='{0}'  and [type]={1} and [categoryid]<>{2}", cate.Slug, (int)CategoryType.Category, cate.CategoryId,ConfigHelper.Tableprefix);
                int r = Convert.ToInt32(SqliteHelper.ExecuteScalar(cmdText));
                if (r == 0)
                {
                    return;
                }
                cate.Slug += "-2";
            }
        }

        /// <summary>
        /// 添加分类
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int InsertCategory(CategoryInfo category)
        {
            CheckSlug(category);

            string cmdText = string.Format(@"insert into [{0}category]
                            ([Type],[ParentId],[CateName],[Slug],[Description],[SortNum],[PostCount],[CreateTime])
                            values
                            ( @Type,@ParentId,@CateName,@Slug,@Description,@SortNum,@PostCount,@CreateTime)", ConfigHelper.Tableprefix);
            SqliteParameter[] prams = { 
                                SqliteHelper.MakeInParam("@Type",DbType.Int32,1,(int)CategoryType.Category),
                                SqliteHelper.MakeInParam("@ParentId",DbType.Int32,4,category.ParentId),
								SqliteHelper.MakeInParam("@CateName",DbType.String,255,category.CateName),
                                SqliteHelper.MakeInParam("@Slug",DbType.String,255,category.Slug),
								SqliteHelper.MakeInParam("@Description",DbType.String,255,category.Description),
                                SqliteHelper.MakeInParam("@SortNum",DbType.Int32,4,category.SortNum),
								SqliteHelper.MakeInParam("@PostCount",DbType.Int32,4,category.PostCount),
								SqliteHelper.MakeInParam("@CreateTime",DbType.Date,8,category.CreateTime)
							};
            SqliteHelper.ExecuteScalar(CommandType.Text, cmdText, prams);

            int newId = Convert.ToInt32(SqliteHelper.ExecuteScalar(string.Format("select [categoryid] from [{0}category] order by [categoryid] desc limit 1",ConfigHelper.Tableprefix)));

            return newId;
        }

        public int UpdateCategory(CategoryInfo category)
        {
            CheckSlug(category);

            string cmdText = string.Format(@"update [{0}category] set
                                [Type]=@Type,
                                [ParentId]=@ParentId,
                                [CateName]=@CateName,
                                [Slug]=@Slug,
                                [Description]=@Description,
                                [SortNum]=@SortNum,
                                [PostCount]=@PostCount,
                                [CreateTime]=@CreateTime
                                where categoryid=@categoryid", ConfigHelper.Tableprefix);
            SqliteParameter[] prams = { 
                                SqliteHelper.MakeInParam("@Type",DbType.Int32,1,(int)CategoryType.Category),
                                SqliteHelper.MakeInParam("@ParentId",DbType.Int32,4,category.ParentId),
								SqliteHelper.MakeInParam("@CateName",DbType.String,255,category.CateName),
                                SqliteHelper.MakeInParam("@Slug",DbType.String,255,category.Slug),
								SqliteHelper.MakeInParam("@Description",DbType.String,255,category.Description),
                                SqliteHelper.MakeInParam("@SortNum",DbType.Int32,4,category.SortNum),
								SqliteHelper.MakeInParam("@PostCount",DbType.Int32,4,category.PostCount),
								SqliteHelper.MakeInParam("@CreateTime",DbType.Date,8,category.CreateTime),
                                SqliteHelper.MakeInParam("@categoryid",DbType.Int32,1,category.CategoryId),
							};
            return Convert.ToInt32(SqliteHelper.ExecuteScalar(CommandType.Text, cmdText, prams));
        }

        public int DeleteCategory(int categoryId)
        {
            string cmdText = string.Format("delete from [{0}category] where [categoryid] = @categoryid",ConfigHelper.Tableprefix);
            SqliteParameter[] prams = { 
								SqliteHelper.MakeInParam("@categoryid",DbType.Int32,4,categoryId)
							};
            return SqliteHelper.ExecuteNonQuery(CommandType.Text, cmdText, prams);


        }

        public CategoryInfo GetCategory(int categoryId)
        {
            string cmdText = string.Format("select * from [{0}category] where [categoryid] = @categoryid",ConfigHelper.Tableprefix);
            SqliteParameter[] prams = { 
								SqliteHelper.MakeInParam("@categoryid",DbType.Int32,4,categoryId)
							};

            List<CategoryInfo> list = DataReaderToListCate(SqliteHelper.ExecuteReader(CommandType.Text, cmdText, prams));
            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>
        /// 获取全部分类
        /// </summary>
        /// <returns></returns>
        public List<CategoryInfo> GetCategoryList()
        {
            string condition = " [type]=" + (int)CategoryType.Category;

            string cmdText = string.Format("select * from [{0}category] where " + condition + "  order by [SortNum] asc,[categoryid] asc",ConfigHelper.Tableprefix);

            return DataReaderToListCate(SqliteHelper.ExecuteReader(cmdText));

        }

        /// <summary>
        /// 转换实体
        /// </summary>
        /// <param name="read">SqliteDataReader</param>
        /// <returns>CategoryInfo</returns>
        private static List<CategoryInfo> DataReaderToListCate(SqliteDataReader read)
        {
            var list = new List<CategoryInfo>();
            while (read.Read())
            {
                var category = new CategoryInfo
                                   {
                                       CategoryId = Convert.ToInt32(read["categoryid"]),
                                       ParentId = Jqpress.Framework.Utils.TypeConverter.ObjectToInt(read["ParentId"],0),
                                       CateName = Convert.ToString(read["CateName"]),
                                       Slug = Convert.ToString(read["Slug"]),
                                       Description = Convert.ToString(read["Description"]),
                                       SortNum = Convert.ToInt32(read["SortNum"]),
                                       PostCount = Convert.ToInt32(read["PostCount"]),
                                       CreateTime = Convert.ToDateTime(read["CreateTime"])
                                   };
                //  category.Type = Convert.ToInt32(read["Type"]);

                list.Add(category);
            }
            read.Close();
            return list;
        }
    }
}
