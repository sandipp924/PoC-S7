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
    #region RabbitMqMessageQueueBase Class

    /// <summary>
    /// Base class that manages a RabbitMq message queue. Can be used for reading or writing to the queue.
    /// </summary>
    /// <typeparam name="TMessage">Type of messages that will be recieved or written to on this queue.</typeparam>
    public abstract class RabbitMqMessageQueueBase<TMessage> : IDisposable
    {
        #region Member Vars

        public static readonly string ExchangeName = "mq-services-exchange";

        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        protected readonly string _queueName;
        protected readonly MqConfiguration _mqConfiguration;
        protected readonly IObjectEncoder<TMessage> _messageEncoder;

        #endregion
        
        #region Constructor

        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="RabbitMqMessageQueueBase"/>.
        /// </summary>
        /// <param name="connectionFactory">RabbitMq connection factory used to create connections to the RabbitMq broker.</param>
        /// <param name="mqConfiguration">Configuration parameters for creation of RabbitMq entities such as exchanges, queues and messages.</param>
        /// <param name="useTempQueue">Whether to use temporary queue instead of a pre-defined queue name for <typeparamref name="TMessage"/> type. If true a
        /// RabbitMq temp queue is created where messages will be read from or written to. If false, a queue by a name predfined for <i>TMessage</i> type is used.</param>
        protected RabbitMqMessageQueueBase(ConnectionFactory connectionFactory, MqConfiguration mqConfiguration, bool useTempQueue = false)
        {
            _connectionFactory = connectionFactory;
            _mqConfiguration = mqConfiguration;
            _messageEncoder = new JsonObjectEncoder<TMessage>();

            _connection = _connectionFactory.CreateConnection();
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

        #endregion

        #region Properties

        public IModel Channel { get { return _channel; } }
        public MqConfiguration MqConfiguration { get { return _mqConfiguration; } }
        public string QueueName { get { return _queueName; } } 

        #endregion

        #region Methods

        #region Dispose
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
        #endregion 

        #endregion

        #region Destructor
        // This may not be necessary if RabbitMq connection also implements finalizer.
        ~RabbitMqMessageQueueBase()
        {
            this.Dispose();
            GC.SuppressFinalize(this);
        } 
        #endregion
    } 

    #endregion
}
