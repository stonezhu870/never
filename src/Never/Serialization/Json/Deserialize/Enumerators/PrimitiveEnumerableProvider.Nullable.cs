﻿using Never.Serialization.Json.MethodProviders;
using System;
using System.Collections.Generic;

namespace Never.Serialization.Json.Deserialize.Enumerators
{
    /// <summary>
    /// 基元类型的集合写入流
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NullablePrimitiveEnumerableProvider<T> where T : struct
    {
        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveEnumerableProvider{T}"/> class.
        /// </summary>
        /// <param name="methodProvider">The method provider.</param>
        public NullablePrimitiveEnumerableProvider(IConvertMethodProvider<Nullable<T>> methodProvider)
        {
            this.MethodProvider = methodProvider;
        }

        #endregion ctor

        #region IMethodProvider

        /// <summary>
        /// 写入流中
        /// </summary>
        /// <param name="name">The writer.</param>
        /// <param name="setting">The setting.</param>
        /// <param name="reader">The array.</param>
        /// <param name="arrayLevel">下一个数组层次，如果为1，则表示数组连续，比如2维数据</param>
        public virtual Nullable<T>[] Parse(IDeserializerReader reader, JsonDeserializeSetting setting, string name, int arrayLevel)
        {
            var node = reader.Read(name);
            if (node == null)
            {
                if (name != null)
                    return new Nullable<T>[0];

                if (reader.ContainerSignal != ContainerSignal.Array)
                    return new Nullable<T>[0];

                var temp = new Nullable<T>[reader.Count];
                for (var i = 0; i < reader.Count; i++)
                {
                    var item = reader.MoveNext();
                    if (item == null)
                        break;

                    temp[i] = this.MethodProvider.Parse(reader,setting,  item, true);
                }

                return temp;
            }

            if (node.NodeType != ContentNodeType.Array)
                return new Nullable<T>[0];

            var nodes = node.Node as IList<JsonContentNode>;
            if (nodes == null)
                return new Nullable<T>[0];

            if (nodes.Count == 1)
            {
                var subNodes = nodes[0].Node as IList<JsonContentNode>;
                if (subNodes == null || subNodes.Count <= 0)
                {
                    var nodeValue = nodes[0] == null ? ArraySegmentValue.Empty : nodes[0].GetValue();
                    if (nodeValue.IsNullOrEmpty)
                        return new Nullable<T>[0];
                }
            }

            if (nodes[0].ArrayLevel != arrayLevel)
                return new Nullable<T>[0];

            var list = new Nullable<T>[nodes.Count];
            for (var i = 0; i < nodes.Count; i++)
            {
                var nodeValue = nodes[i] == null ? ArraySegmentValue.Empty : nodes[i].GetValue();
                if (nodes[i].NodeType == ContentNodeType.String)
                {
                    if (StringMethodProvider.Default.IsNullValue(nodeValue))
                    {
                        list[i] = null;
                        continue;
                    }
                }

                if (nodeValue.Length == 0)
                {
                    list[i] = null;
                    continue;
                }

                list[i] = this.MethodProvider.Parse(reader,setting,  nodes[i], true);
            }

            return list;
        }

        /// <summary>
        /// 获取方法转换
        /// </summary>
        /// <returns>IMethodProvider&lt;T&gt;.</returns>
        public IConvertMethodProvider<Nullable<T>> MethodProvider { get; protected set; }

        #endregion IMethodProvider
    }
}