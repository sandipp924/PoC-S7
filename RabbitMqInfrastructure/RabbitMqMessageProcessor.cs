using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Util;
using ServiceStack;
using ServiceStack.Messaging;
using RabbitMQ.Client.Events;
using System;

namespace Common
{
    public interface IRabbitMqMessageProcessor : IDisposable
    {
        void Run();
    }

    public static class RabbitMqMessageProcessorFactory
    {
        public interface IFactory
        {
            IRabbitMqMessageProcessor Create(Func<object, object> processMessage, ConnectionFactory connectionFactory, MqConfiguration mqConfig);
        }

        public class Factory<TRequest, TResponse> : IFactory
        {
            public IRabbitMqMessageProcessor Create(Func<object, object> processMessage, ConnectionFactory connectionFactory, MqConfiguration mqConfig)
            {
                var messageReadQueue = new RabbitMqReadMessageQueue<TRequest>(connectionFactory, mqConfig);
                return new RabbitMqMessageProcessor<TRequest, TResponse>(messageReadQueue, request => (TResponse)processMessage(request));
            }
        }

        public static IRabbitMqMessageProcessor Create(Type messageType, Type responseType, 
            ConnectionFactory connectionFactory, MqConfiguration mqConfig, Func<object, object> processMessage)
        {
            return typeof(Factory<,>).MakeGenericType(messageType, responseType).CreateInstance<IFactory>().Create(processMessage, connectionFactory, mqConfig);
        }
    }

    /// <summary>
    /// Process a messages of <typeparamref name="TMessage"/> type and sends response back on the ReplyTo queue specified
    /// on the incoming message.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class RabbitMqMessageProcessor<TMessage, TResponse> : IRabbitMqMessageProcessor
    {
        #region Member Vars

        private RabbitMqReadMessageQueue<TMessage> _inQueue;
        private readonly Func<TMessage, TResponse> _processMessage;
        private readonly IObjectEncoder<TResponse> _responseEncoder;
        private readonly object _sendMessageLock = new object();
        private volatile bool _stop;

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="RabbitMqMessageProcessor"/>.
        /// </summary>
        /// <param name="inQueue">Queue from which to receive messages.</param>
        /// <param name="processMessage">Handler for processing a message and forming a response.</param>
        public RabbitMqMessageProcessor(RabbitMqReadMessageQueue<TMessage> inQueue, Func<TMessage, TResponse> processMessage)
        {
            _processMessage = processMessage;
            _inQueue = inQueue;
            _responseEncoder = new JsonObjectEncoder<TResponse>();
        } 
        #endregion

        #region Methods

        #region Private Methods

        #region ProcessSingleMessage
        private void ProcessSingleMessage(TimeSpan timeOut)
        {
            TMessage message;
            BasicDeliverEventArgs messageEventArgs;
            if (!_inQueue.ReadNextMessage(timeOut, out message, out messageEventArgs) || _stop)
                return;

            Task.Run<TResponse>(() =>
            {
                try
                {
                    return _processMessage(message);
                }
                catch (Exception exception)
                {
                    this.GetType().ErrorFormat("Error in service: {0}", exception.Message);
                    return default(TResponse);
                }
            }).ContinueWith(t =>
            {
                var response = t.Result;
                if (response != null)
                {
                    try
                    {
                        var sourceProps = messageEventArgs.BasicProperties;

                        var channel = _inQueue.Channel;
                        var publishProps = channel.CreateBasicProperties();
                        if (sourceProps.CorrelationId != null)
                            publishProps.CorrelationId = sourceProps.CorrelationId;

                        publishProps.SetPersistent(false);

                        //Lock may not be necessary if channel ops are thread-safe.
                        lock (_sendMessageLock)
                        {
                            channel.BasicPublish(RabbitMqReadMessageQueue<TMessage>.ExchangeName, sourceProps.ReplyTo, publishProps, _responseEncoder.Encode(response));
                            if (!_inQueue.MqConfiguration.NoAck)
                            {
                                channel.BasicAck(messageEventArgs.DeliveryTag, false);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        this.GetType().ErrorFormat("Error sending MQ message: {0}", exception.Message);
                    }
                }
            });
        }
        #endregion

        #endregion

        #region Public Methods

        #region Run
        public void Run()
        {
            while (!_stop)
            {
                this.ProcessSingleMessage(TimeSpan.FromMilliseconds(100));
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            _stop = true;
            if (_inQueue != null)
            {
                _inQueue.Dispose();
                _inQueue = null;
            }
        }
        #endregion

        #endregion 

        #endregion

        #region Destructor
        ~RabbitMqMessageProcessor()
        {
            this.Dispose();
            GC.SuppressFinalize(this);
        } 
        #endregion
    }
}
