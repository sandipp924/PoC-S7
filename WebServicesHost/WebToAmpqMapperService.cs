using Services.Dtos;
using Common;
using Funq;
using RabbitMQ.Client;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Messaging;
using ServiceStack.RabbitMq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebServices
{
    public class WebToAmpqMapperService : Service
    {
        private static readonly string AmqpServerUri = ConfigUtils.GetConnectionString("AmqpServer");
        private static readonly TimeSpan TimeOut = TimeSpan.FromSeconds(10);
        private readonly ConnectionFactory _rabbitMqConnectionFactory;

        public WebToAmpqMapperService()
        {
            _rabbitMqConnectionFactory = new ConnectionFactory
            {
                Uri = AmqpServerUri
            };
        }

        public object Any(SymbologyInfoQuery symbologyInfoQuery)
        {
            return GetIml<SymbologyInfoQuery, object>(symbologyInfoQuery);
        }

        private object GetIml<TMessage, TResponse>(TMessage queryObject)
        {
            using (var rpcCallHelper = new RabbitMqRpcHelper<TMessage, TResponse>(_rabbitMqConnectionFactory))
            {
                TResponse response;
                Exception error;
                if (rpcCallHelper.Call(queryObject, out response, TimeOut, out error))
                    return response;
                else
                    return new HttpError(System.Net.HttpStatusCode.RequestTimeout, "Time out recieving mq message.");
            }
        }
    }
}