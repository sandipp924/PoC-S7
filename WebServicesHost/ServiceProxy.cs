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
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;
using RabbitMQ.Client.Exceptions;

namespace WebServices
{
    /// <summary>
    /// Base class for a generic service type where each instance handles requests for a single DTO type. Essentially a single
    /// service proxy will be created to handle each REST endpoint. The request is delegated to AMQP service using <see cref="WebToAmqpMapperService"/>.
    /// </summary>
    internal class ServiceProxy : IService
    {
    }

    /// <summary>
    /// A generic service type that handles requests for <typeparamref name="TRequest"/> DTO type. Essentially a single
    /// instance will be created to handle each REST endpoint. The request is delegated to AMQP service using <see cref="WebToAmqpMapperService"/>.
    /// </summary>
    /// <typeparam name="TRequest">Request DTO type.</typeparam>
    /// <typeparam name="TResponse">Response DTO type.</typeparam>
    internal class ServiceProxy<TRequest, TResponse> : ServiceProxy
    {
        // NOTE: The reason the return type is object instead of TResponse is that the WebToAmqpMapperService
        // can return an HttpError object as well instead of the TResponse.
        private Func<TRequest, object> _handler;

        public ServiceProxy(Func<TRequest, object> handler)
        {
            _handler = handler;
        }
        
        public object Get(TRequest request)
        {
            return _handler(request);
        }
    }
}