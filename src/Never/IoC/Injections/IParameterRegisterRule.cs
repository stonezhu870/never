﻿namespace Never.IoC.Injections
{
    /// <summary>
    /// 参数注册规则
    /// </summary>
    public interface IParameterRegisterRule
    {
        /// <summary>
        /// 构造函数参数
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <param name="key">注册key</param>
        /// <returns></returns>
        IParameterRegisterRule WithParameter<TService>(string key);
    }
}