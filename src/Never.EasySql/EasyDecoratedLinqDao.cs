﻿using Never.EasySql.Client;
using Never.EasySql.Linq;
using Never.EasySql.Linq.Expressions;
using Never.EasySql.Text;
using Never.EasySql.Xml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Never.EasySql
{
    /// <summary>
    /// 对难用的语法进行一下装饰查询，更好的使用Idao接口，该对象每次执行一次都会释放IDao接口，请不要重复使用
    /// </summary>
    public class EasyDecoratedLinqDao<Parameter> : EasyDecoratedDao, IDao, IDisposable
    {
        #region field

        private readonly IDao dao = null;
        private readonly EasySqlParameter<Parameter> parameter = null;
        private string cacheId = null;
        #endregion field

        #region ctor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dao"></param>
        /// <param name="parameter"></param>
        public EasyDecoratedLinqDao(IDao dao, EasySqlParameter<Parameter> parameter) : base(dao)
        {
            this.dao = dao;
            this.parameter = parameter;
        }

        #endregion ctor

        #region context

        private Context GetContext()
        {
            if (this.dao.SqlExecuter is MySqlExecuter)
                return new MySqlContext() { CacheId = this.cacheId };

            if (this.dao.SqlExecuter is SqlServerExecuter)
                return new SqlServerContext() { CacheId = this.cacheId };

            if (this.dao.SqlExecuter is OdbcServerExecuter)
                return new OdbcServerContext() { CacheId = this.cacheId };

            if (this.dao.SqlExecuter is OleDbServerExecuter)
                return new OleDbServerContext() { CacheId = this.cacheId };

            if (this.dao.SqlExecuter is OracleServerExecuter)
                return new OracleServerContext() { CacheId = this.cacheId };

            if (this.dao.SqlExecuter is PostgreSqlExecuter)
                return new PostgreSqlContext() { CacheId = this.cacheId };

            throw new Exception("dao.SqlExecuter 无法识别，不能创建上下文");
        }

        #endregion

        #region trans

        /// <summary>
        /// 开启新事务
        /// </summary>
        /// <returns></returns>
        public EasyDecoratedLinqDao<Parameter> BeginTransaction()
        {
            this.dao.BeginTransaction();
            return this;
        }

        /// <summary>
        /// 开启新事务
        /// </summary>
        /// <param name="level"></param>
        public EasyDecoratedLinqDao<Parameter> BeginTransaction(IsolationLevel level)
        {
            this.dao.BeginTransaction(level);
            return this;
        }

        /// <summary>
        /// 提交
        /// </summary>
        public void CommitTransaction()
        {
            this.dao.CommitTransaction();
            this.dao.Dispose();
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="closeConnection">关闭连接</param>

        public void CommitTransaction(bool closeConnection)
        {
            this.dao.CommitTransaction(closeConnection);
            this.dao.Dispose();
        }

        /// <summary>
        /// 回滚
        /// </summary>
        public void RollBackTransaction()
        {
            this.dao.RollBackTransaction();
            this.dao.Dispose();
        }

        /// <summary>
        /// 回滚
        /// </summary>
        /// <param name="closeConnection">关闭连接</param>
        public void RollBackTransaction(bool closeConnection)
        {
            this.dao.RollBackTransaction(closeConnection);
            this.dao.Dispose();
        }

        #endregion

        #region crud

        /// <summary>
        /// 将sql语句分析好后缓存起来
        /// </summary>
        /// <param name="cacheId"></param>
        /// <returns></returns>
        public EasyDecoratedLinqDao<Parameter> Cached(string cacheId)
        {
            this.cacheId = cacheId;
            return this;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        public Update<Parameter> Update()
        {
            return new Update<Parameter>() { Context = this.GetContext() };
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        public Delete<Parameter> Delete()
        {
            return new Delete<Parameter>() { Context = this.GetContext() };
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <returns></returns>
        public Insert<Parameter> Insert()
        {
            return new Insert<Parameter>() { Context = this.GetContext() };
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <returns></returns>
        public Select<Parameter, T> Select<T>()
        {
            if (this.dao.SqlExecuter is MySqlExecuter)
                return new Linq.MySql.Select<Parameter, T>() { CacheId = this.cacheId };

            if (this.dao.SqlExecuter is SqlServerExecuter)
                return new SqlServerContext() { CacheId = this.cacheId };

            if (this.dao.SqlExecuter is OdbcServerExecuter)
                return new OdbcServerContext() { CacheId = this.cacheId };

            if (this.dao.SqlExecuter is OleDbServerExecuter)
                return new OleDbServerContext() { CacheId = this.cacheId };

            if (this.dao.SqlExecuter is OracleServerExecuter)
                return new OracleServerContext() { CacheId = this.cacheId };

            if (this.dao.SqlExecuter is PostgreSqlExecuter)
                return new PostgreSqlContext() { CacheId = this.cacheId };

            return new Select<Parameter, T>() { Context = this.GetContext() };
        }

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="callmode"></param>
        /// <returns></returns>
        public object Call(string sql, CallMode callmode)
        {
            var sqlTag = TextLabelBuilder.Build(sql, this.cacheId, this.dao);
            if (this.dao.CurrentSession != null)
            {
                return this.dao.Call<Parameter>(sqlTag, this.parameter, callmode);
            }

            using (this.dao)
            {
                return this.dao.Call<Parameter>(sqlTag, this.parameter, callmode);
            }
        }

        #endregion crud
    }
}
