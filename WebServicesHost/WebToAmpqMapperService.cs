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
    /// Service that handles web requests. It delegates the requests to AMQP services and returns responses recieved from the services.
    /// </summary>
    public class WebToAmqpMapperService : Service
    {
        #region Member Vars

        private readonly ConnectionFactory _rabbitMqConnectionFactory;
        private readonly ConcurrentDictionary<Type, DtoConfigurationSettings> _dtoConfigurationSettings;
        private readonly MqConfiguration _rabbitMqConfiguration; 

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="WebToAmqpMapperService"/>.
        /// </summary>
        public WebToAmqpMapperService()
        {
            _dtoConfigurationSettings = new ConcurrentDictionary<Type, DtoConfigurationSettings>();

            // AppSettings reads from the app.config, which is configured to include settings from the settings.xml
            // at the root solution folder level.
            IAppSettings settings = new AppSettings();
            _rabbitMqConnectionFactory = Utilities.CreateMqConnectionFactory(settings);
            _rabbitMqConfiguration = Utilities.GetMqConfiguration(settings);
        } 
        #endregion

        #region Methods

        #region Public Methods

        #region ProcessRequest
        /// <summary>
        /// A generic method for performing queries via RabbitMq broker.
        /// </summary>
        /// <typeparam name="TRequest">Request DTO type.</typeparam>
        /// <typeparam name="TResponse">Response DTO type.</typeparam>
        /// <param name="request">Request DTO object.</param>
        /// <returns>Returns the response for the request. The returned object can be an instance of DTO object corresponding to 
        /// <typeparamref name="TRequest"/> request type, or an HttpError object in case of an error.</returns>
        public object ProcessRequest<TRequest, TResponse>(TRequest request)
        {
            var dtoConfSettings = this.GetCachedDtoConfigurationSettings<TRequest>();

            try
            {
                using (var rpcCallHelper = new RabbitMqRpcHelper<TRequest, TResponse>(_rabbitMqConnectionFactory, _rabbitMqConfiguration))
                {
                    if (dtoConfSettings.LogQueries.Value)
                        this.GetType().DebugFormat("Web request for {0} on process id {1}, thread id {2}",
                            request.ToJson(), Process.GetCurrentProcess().Id, Thread.CurrentThread.ManagedThreadId);

                    TResponse response;
                    Exception error;
                    if (rpcCallHelper.Call(request, out response, TimeSpan.FromMilliseconds(dtoConfSettings.AmqpResponseTimeOut.Value), out error))
                        return response;
                    else
                        return new HttpError(System.Net.HttpStatusCode.RequestTimeout, "Time out recieving mq message.");
                }
            }
            // BrokerUnreachableException is raised when RabbitMq broker is not running.
            catch (BrokerUnreachableException exception)
            {
                return new HttpError(System.Net.HttpStatusCode.ServiceUnavailable,
                    "Unable to connect to RabbitMQ broker. Ensure it's running and correct adress is specified (settings.xml).", exception.Message);
            }
        }
        #endregion

        #endregion

        #region Private Methods

        #region GetCachedDtoConfigurationSettings
        /// <summary>
        /// Gets the DTO specific configuration from the application settings file.
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <returns></returns>
        private DtoConfigurationSettings GetCachedDtoConfigurationSettings<TDto>()
        {
            DtoConfigurationSettings ret;
            if (!_dtoConfigurationSettings.TryGetValue(typeof(TDto), out ret))
            {
                // AppSettings reads from the app.config, which is configured to include settings from the settings.xml
                // at the root solution folder level.
                IAppSettings settings = new AppSettings();
                ret = settings.Get<DtoConfigurationSettings>(typeof(TDto).Name, new DtoConfigurationSettings());
                ret.AmqpResponseTimeOut = ret.AmqpResponseTimeOut ?? _rabbitMqConfiguration.DefaultAmqpResponseTimeOut ?? 10000;
                ret.LogQueries = ret.LogQueries ?? true;
                _dtoConfigurationSettings[typeof(TDto)] = ret;
            }

            return ret;
        }
        #endregion

        #endregion 

        #endregion
    }
}