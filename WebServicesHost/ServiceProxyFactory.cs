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
    /// Class that defines the factory method for creating service proxies.
    /// </summary>
    internal abstract class ServiceProxyFactory
    {
        public static ServiceProxy Create(Type requestType, Type responseType, WebToAmqpMapperService amqpService)
        {
            var serviceProxyType = typeof(ServiceProxyFactory<,>).MakeGenericType(requestType, responseType);
            var serviceProxyFactory = (ServiceProxyFactory)Activator.CreateInstance(serviceProxyType);
            return serviceProxyFactory.Create(amqpService);
        }

        protected abstract ServiceProxy Create(WebToAmqpMapperService amqpService);
    }

    /// <summary>
    /// A factory class used by <see cref="WebToAmqpMapperService"/> to create 
    /// <seealso cref="ServiceProxy&lt;TRequest, TResponse&gt;"/> instace that is registered with the ServiceStack host.
    /// </summary>
    /// <typeparam name="TRequest">Request type handled by the created service proxy.</typeparam>
    /// <typeparam name="TResponse">The type of response that the service returns when processing requests of <typeparamref name="TRequest"/> type.</typeparam>
    internal class ServiceProxyFactory<TRequest, TResponse> : ServiceProxyFactory
    {
        protected override ServiceProxy Create(WebToAmqpMapperService amqpService)
        {
            return new ServiceProxy<TRequest, TResponse>(amqpService.ProcessRequest<TRequest, TResponse>);
        }
    }
}