﻿using Never.Remoting;
using Never.Remoting.Http;
using Never.Sockets.AsyncArgs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Never.Configuration.ConfigCenter.Remoting
{
    /// <summary>
    /// 客户端
    /// </summary>
    public class ConfigFileClient : IWorkService
    {
        private ClientRequestHadler requestHandler = null;
        private Action<ConfigFileClientCallbakRequest, string> saveFile = null;
        private IEnumerable<ConfigFileClientRequest> allFiles = null;
        private System.Threading.Timer timer = null;
        private EndPoint serverEndPoint = null;
        private Never.Threading.IRigidLocker locker = new Never.Threading.MonitorLocker();
        private TimeSpan keepAlive = TimeSpan.Zero;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverEndPoint"></param>
        public ConfigFileClient(EndPoint serverEndPoint)
        {
            this.serverEndPoint = serverEndPoint;
            this.requestHandler = new ClientRequestHadler(this.serverEndPoint, new Protocol());
            this.requestHandler.OnMessageReceived += this.RequestHandler_OnMessageReceived;
            this.timer = new System.Threading.Timer(Reconnect, this, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
        }

        private void RequestHandler_OnMessageReceived(object sender, IRemoteResponse response, OnReceivedSocketEventArgs args)
        {
            //你发送了test命令，对方响应了，这个时候你又收到了test消息，那么你这里又开始发消息，这个就形成了死循环，所以收到了消息只能是服务器主动发送的消息
            if (this.allFiles == null || this.saveFile == null)
                return;

            if (response.CommandType.IsEquals(ConfigFileCommand.Pull))
            {
                System.Threading.ThreadPool.QueueUserWorkItem(x =>
                {
                    foreach (var file in this.allFiles)
                    {
                        this.Push(file.FileName);
                    }
                });
            }
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="keepAlive">心跳</param>
        /// <param name="allFiles"></param>
        /// <param name="saveFile"></param>
        public ConfigFileClient Startup(TimeSpan keepAlive, IEnumerable<ConfigFileClientRequest> allFiles, Action<ConfigFileClientCallbakRequest, string> saveFile)
        {
            return this.Startup(keepAlive, TimeSpan.Zero, allFiles, saveFile);
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="keepAlive">心跳</param>
        /// <param name="reconnect">重连接</param>
        /// <param name="allFiles"></param>
        /// <param name="saveFile"></param>
        /// <returns></returns>
        public ConfigFileClient Startup(TimeSpan keepAlive, TimeSpan reconnect, IEnumerable<ConfigFileClientRequest> allFiles, Action<ConfigFileClientCallbakRequest, string> saveFile)
        {
            this.allFiles = allFiles;
            this.saveFile = saveFile;
            this.keepAlive = keepAlive;
            this.requestHandler.KeepAlive(this.keepAlive);
            this.requestHandler.Startup();
            if (reconnect > TimeSpan.Zero)
            {
                this.timer.Change(reconnect, reconnect);
            }

            return this;
        }

        /// <summary>
        /// 重连接
        /// </summary>
        /// <param name="state"></param>
        private void Reconnect(object state)
        {
            this.locker.EnterLock(true, () =>
            {
                try
                {
                    if (this.requestHandler.Socket == null || this.requestHandler.Socket.Connection == null || this.requestHandler.Socket.Connection.IsConnected == false)
                    {
                        this.requestHandler.Dispose();
                        this.requestHandler.OnMessageReceived -= this.RequestHandler_OnMessageReceived;
                        this.requestHandler = new ClientRequestHadler(this.serverEndPoint, this.requestHandler.Protocol);
                        this.requestHandler.OnMessageReceived += this.RequestHandler_OnMessageReceived;
                        this.requestHandler.KeepAlive(this.keepAlive);
                        this.requestHandler.Startup();
                    }
                }
                catch
                {

                }
                try
                {
                    var test = this.Test();
                    test.ContinueWith(ta =>
                    {
                        if (ta.Exception != null)
                        {
                            this.requestHandler.Dispose();
                            this.requestHandler.OnMessageReceived -= this.RequestHandler_OnMessageReceived;
                            this.requestHandler = new ClientRequestHadler(this.serverEndPoint, this.requestHandler.Protocol);
                            this.requestHandler.OnMessageReceived += this.RequestHandler_OnMessageReceived;
                            this.requestHandler.KeepAlive(this.keepAlive);
                            this.requestHandler.Startup();
                        }
                    });
                }
                catch
                {

                }

            });          
        }

        /// <summary>
        /// 启动
        /// </summary>
        void IWorkService.Startup()
        {
            if (this.allFiles == null && this.saveFile == null)
                throw new Exception("you must call startup(allfliles,saveFile) method");
        }


        /// <summary>
        /// 关闭
        /// </summary>
        public ConfigFileClient Shutdown()
        {
            this.requestHandler.Shutdown();
            return this;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        void IWorkService.Shutdown()
        {
            this.Shutdown();
        }

        /// <summary>
        /// 
        /// </summary>
        public IRequestHandler RequestHandler { get { return this.requestHandler; } }

        /// <summary>
        /// 获取数据，会调用Startup里面的回调
        /// </summary>
        /// <param name="name"></param>
        public Task<IRemoteResponse> Push(string name)
        {
            var task = this.requestHandler.Excute(new Request(Encoding.UTF8, ConfigFileCommand.Push)
            {
                Query = new System.Collections.Specialized.NameValueCollection() { { "file", name } }
            });

            return task.Task.ContinueWith(ta =>
            {
                var response = ta.Result as Response;
                var fileName = response.Query["file"];
                if (fileName.IsNotNullOrEmpty())
                {
                    if (response.Body != null)
                    {
                        var content = response.Encoding.GetString((response.Body as MemoryStream).ToArray());
                        this.saveFile(new ConfigFileClientCallbakRequest() { FileName = fileName, Encoding = response.Query["encoding"] }, content);
                    }
                    else
                    {
                        this.saveFile(new ConfigFileClientCallbakRequest() { FileName = fileName, Encoding = response.Query["encoding"] }, null);
                    }
                }

                return ta.Result;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<IRemoteResponse> Test()
        {
            var task = this.requestHandler.Excute(new Request(Encoding.UTF8, ConfigFileCommand.Test)
            {
            });

            return task.Task;
        }
    }
}
