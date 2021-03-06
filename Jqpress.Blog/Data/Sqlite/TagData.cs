﻿using System;
using System.Collections.Generic;
using Jqpress.Blog.Entity;
using Mono.Data.Sqlite;
using System.Data.SQLite;
using System.Data;
using Jqpress.Framework.DbProvider.SQLite;
using Jqpress.Framework.Configuration;
using Jqpress.Blog.Entity.Enum;

namespace Jqpress.Blog.Data.SQLite
{
    public partial class DataProvider
    {
        /// <summary>
        /// 检查别名是否重复
        /// </summary>
        /// <param name="cate"></param>
        /// <returns></returns>
        private static void CheckSlug(TagInfo cate)
        {
            while (true)
            {
                string cmdText = cate.TagId == 0 ? string.Format("select count(1) from [{2}category] where [Slug]='{0}' and [type]={1}", cate.Slug, (int)CategoryType.Tag,ConfigHelper.Tableprefix) : string.Format("select count(1) from [{3}category] where [Slug]='{0}'  and [type]={1} and [categoryid]<>{2}", cate.Slug, (int)CategoryType.Tag, cate.TagId, ConfigHelper.Tableprefix);
                int r = Convert.ToInt32(SQLiteHelper.ExecuteScalar(cmdText));
                if (r == 0)
                {
                    return;
                }
                cate.Slug += "-2";
            }
        }

        public int InsertTag(TagInfo tag)
        {
            CheckSlug(tag);

            string cmdText =string.Format(@"insert into [{0}category]
                            (
                            [Type],[ParentId],[CateName],[Slug],[Description],[SortNum],[PostCount],[CreateTime]
                            )
                            values
                            (
                            @Type,@ParentId,@CateName,@Slug,@Description,@SortNum,@PostCount,@CreateTime
                            )", ConfigHelper.Tableprefix);
            SQLiteParameter[] prams = { 
                                SQLiteHelper.MakeInParam("@Type",DbType.Int32,1,(int)CategoryType.Tag),
                                SQLiteHelper.MakeInParam("@ParentId",DbType.Int32,4,0),
								SQLiteHelper.MakeInParam("@CateName",DbType.String,255,tag.CateName),
                                SQLiteHelper.MakeInParam("@Slug",DbType.String,255,tag.Slug),
								SQLiteHelper.MakeInParam("@Description",DbType.String,255,tag.Description),
                                SQLiteHelper.MakeInParam("@SortNum",DbType.Int32,4,tag.SortNum),
								SQLiteHelper.MakeInParam("@PostCount",DbType.Int32,4,tag.PostCount),
								SQLiteHelper.MakeInParam("@CreateTime",DbType.Date,8,tag.CreateTime)
							};
            SQLiteHelper.ExecuteScalar(CommandType.Text, cmdText, prams);

            int newId = Convert.ToInt32(SQLiteHelper.ExecuteScalar(string.Format("select [categoryid] from [{0}category] order by [categoryid] desc limit 1",ConfigHelper.Tableprefix)));

            return newId;
        }

        public int UpdateTag(TagInfo tag)
        {
            CheckSlug(tag);

            string cmdText = string.Format(@"update [{0}category] set
                                [Type]=@Type,
                                [CateName]=@CateName,
                                [Slug]=@Slug,
                                [Description]=@Description,
                                [SortNum]=@SortNum,
                                [PostCount]=@PostCount,
                                [CreateTime]=@CreateTime
                                where categoryid=@categoryid",ConfigHelper.Tableprefix);
            SQLiteParameter[] prams = { 
                                SQLiteHelper.MakeInParam("@Type",DbType.Int32,1,(int)CategoryType.Tag),
								SQLiteHelper.MakeInParam("@CateName",DbType.String,255,tag.CateName),
                                SQLiteHelper.MakeInParam("@Slug",DbType.String,255,tag.Slug),
								SQLiteHelper.MakeInParam("@Description",DbType.String,255,tag.Description),
                                SQLiteHelper.MakeInParam("@SortNum",DbType.Int32,4,tag.SortNum),
								SQLiteHelper.MakeInParam("@PostCount",DbType.Int32,4,tag.PostCount),
								SQLiteHelper.MakeInParam("@CreateTime",DbType.Date,8,tag.CreateTime),
                                SQLiteHelper.MakeInParam("@categoryid",DbType.Int32,1,tag.TagId),
							};
            return Convert.ToInt32(SQLiteHelper.ExecuteScalar(CommandType.Text, cmdText, prams));
        }

        public int DeleteTag(int tagId)
        {
            string cmdText = string.Format("delete from [{0}category] where [categoryid] = @categoryid",ConfigHelper.Tableprefix);
            SQLiteParameter[] prams = { 
								SQLiteHelper.MakeInParam("@categoryid",DbType.Int32,4,tagId)
							};
            return SQLiteHelper.ExecuteNonQuery(CommandType.Text, cmdText, prams);


        }

        public TagInfo GetTag(int tagId)
        {
            string cmdText = string.Format("select * from [{0}category] where [categoryid] = @categoryid",ConfigHelper.Tableprefix);
            SQLiteParameter[] prams = { 
								SQLiteHelper.MakeInParam("@categoryid",DbType.Int32,4,tagId)
							};

            List<TagInfo> list = DataReaderToListTag(SQLiteHelper.ExecuteReader(CommandType.Text, cmdText, prams));
            return list.Count > 0 ? list[0] : null;
        }


        public List<TagInfo> GetTagList()
        {
            string condition = " [type]=" + (int)CategoryType.Tag;

            string cmdText = string.Format("select * from [{0}category] where " + condition + "  order by [SortNum] asc ,[categoryid] asc",ConfigHelper.Tableprefix);

            return DataReaderToListTag(SQLiteHelper.ExecuteReader(cmdText));

        }

        public List<TagInfo> GetTagList(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return new List<TagInfo>();
            }

            string cmdText = string.Format("select * from [{0}category] where  [categoryid] in (" + ids + ")",ConfigHelper.Tableprefix);

            //  throw new Exception(cmdText);

            return DataReaderToListTag(SQLiteHelper.ExecuteReader(cmdText));
        }

        /// <summary>
        /// 转换实体
        /// </summary>
        /// <param CateName="read">SQLiteDataReader</param>
        /// <param name="read"></param>
        /// <returns>TagInfo</returns>
        private static List<TagInfo> DataReaderToListTag(SQLiteDataReader read)
        {
            var list = new List<TagInfo>();
            while (read.Read())
            {
                var tag = new TagInfo
                              {
                                  TagId = Convert.ToInt32(read["categoryid"]),
                                  CateName = Convert.ToString(read["CateName"]),
                                  Slug = Convert.ToString(read["Slug"]),
                                  Description = Convert.ToString(read["Description"]),
                                  SortNum = Convert.ToInt32(read["SortNum"]),
                                  PostCount = Convert.ToInt32(read["PostCount"]),
                                  CreateTime = Convert.ToDateTime(read["CreateTime"])
                              };
                //  tag.Type = Convert.ToInt32(read["Type"]);

                list.Add(tag);
            }
            read.Close();
            return list;
        }

    }
}
