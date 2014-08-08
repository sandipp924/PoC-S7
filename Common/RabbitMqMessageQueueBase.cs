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
    public abstract class RabbitMqMessageQueueBase<TMessage> : IDisposable
    {
        public static readonly string ExchangeName = "mq-services-exchange";

        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        protected readonly string _queueName;
        protected readonly MqConfiguration _mqConfiguration;
        protected readonly IObjectEncoder<TMessage> _messageEncoder;

        protected RabbitMqMessageQueueBase(ConnectionFactory connectionFactory, MqConfiguration mqConfiguration, bool useTempQueue = false)
        {
            _connectionFactory = connectionFactory;
            _mqConfiguration = mqConfiguration;
            _messageEncoder = new JsonObjectEncoder<TMessage>();

            try
            {
                _connection = _connectionFactory.CreateConnection();
            }
            catch (Exception exception)
            {
                throw new Exception("Unable to connect to RabbitMQ server. Ensure it's running at the right address."
                    + " Address is configured via ApplicationServicesHost project's App.config.", exception);
            }

            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName, "topic", _mqConfiguration.ExchangeDurable);

            if (useTempQueue)
            {
                _queueName = _channel.QueueDeclare().QueueName;
            }
            else
            {
                _queueName = QueueNames<TMessage>.In;
                _channel.QueueDeclare(_queueName, _mqConfiguration.QueueDurable, false, false, null);
            }

            _channel.QueueBind(_queueName, ExchangeName, _queueName);
        }

        public IModel Channel { get { return _channel; } }
        public MqConfiguration MqConfiguration { get { return _mqConfiguration; } }
        public string QueueName { get { return _queueName; } }

        public void Dispose()
        {
            if (_channel != null)
            {
                _channel.Dispose();
                _channel = null;
            }

            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }

        ~RabbitMqMessageQueueBase()
        {
            this.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
