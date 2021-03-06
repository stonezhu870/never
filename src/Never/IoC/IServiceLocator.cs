﻿using System;

namespace Never.IoC
{
    /// <summary>
    /// 服务定位器
    /// </summary>
    public interface IServiceLocator
    {
        /// <summary>
        /// 跟踪者
        /// </summary>
        ILifetimeScopeTracker ScopeTracker { get; }

        /// <summary>
        /// 手动开启一个范围，请使用Using(var scope = this.BeginLifetimeScope())
        /// </summary>
        /// <returns></returns>
        ILifetimeScope BeginLifetimeScope();

        /// <summary>
        /// 返回所有T对象的实体
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <returns></returns>
        TService[] ResolveAll<TService>();

        /// <summary>
        /// 返回所有T对象的实体
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        object[] ResolveAll(Type serviceType);

        /// <summary>
        /// 返回T对象的实体
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <returns></returns>
        TService Resolve<TService>();

        /// <summary>
        /// 返回T对象的实体
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <param name="key">key</param>
        /// <returns></returns>
        TService Resolve<TService>(string key);

        /// <summary>
        /// 返回T对象的实体
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <returns></returns>
        TService ResolveOptional<TService>();

        /// <summary>
        /// 返回某对象实体
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        object Resolve(Type serviceType);

        /// <summary>
        /// 返回某对象实体
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="key">key</param>
        /// <returns></returns>
        object Resolve(Type serviceType, string key);

        /// <summary>
        /// 返回某对象实体
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        object ResolveOptional(Type serviceType);

        /// <summary>
        /// 尝试返回已注册的T对象实体
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <param name="instance">服务对象</param>
        /// <returns></returns>
        bool TryResolve<TService>(ref TService instance);

        /// <summary>
        /// 尝试返回已注册的T对象实体
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <param name="instance">服务对象</param>
        /// <param name="key">key</param>
        /// <returns></returns>
        bool TryResolve<TService>(ref TService instance, string key);

        /// <summary>
        /// 尝试返回已注册的serviceType对象实体
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="instance">服务对象</param>
        /// <returns></returns>
        bool TryResolve(Type serviceType, ref object instance);

        /// <summary>
        /// 尝试返回已注册的serviceType对象实体
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="instance">服务对象</param>
        /// <param name="key">key</param>
        /// <returns></returns>
        bool TryResolve(Type serviceType, ref object instance, string key);
    }
}