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
    /// <summary>
    /// RabbitMq queue used for receiving messages.
    /// </summary>
    /// <typeparam name="TMessage">Type of messages that will be recieved on this queue.</typeparam>
    public class RabbitMqReadMessageQueue<TMessage> : RabbitMqMessageQueueBase<TMessage>
    {
        #region Member Vars

        private readonly QueueingBasicConsumer _consumer; 
        
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="RabbitMqReadMessageQueue"/>.
        /// </summary>
        /// <param name="connectionFactory">RabbitMq connection factory used to create connections to the RabbitMq broker.</param>
        /// <param name="mqConfiguration">Configuration parameters for creation of RabbitMq entities such as exchanges, queues and messages.</param>
        /// <param name="useTempQueue">Whether to use temporary queue instead of a pre-defined queue name for <typeparamref name="TMessage"/> type. If true a
        /// RabbitMq temp queue is created where messages will be read from or written to. If false, a queue by a name predfined for <i>TMessage</i> type is used.</param>
        public RabbitMqReadMessageQueue(ConnectionFactory connectionFactory, MqConfiguration mqConfiguration, bool useTempQueue = false)
            : base(connectionFactory, mqConfiguration, useTempQueue)
        {
            _consumer = new QueueingBasicConsumer(this.Channel);
            this.Channel.BasicConsume(_queueName, mqConfiguration.NoAck, _consumer);
        } 
        #endregion

        #region Methods

        #region ReadNextMessage
        /// <summary>
        /// Reads the next message in queue. If no message then blocks until a message arrives or time-out expires.
        /// </summary>
        /// <param name="timeOut">Time to wait for a message to arrive. Specify null to indicate never to time-out and wait until the next message arrives.</param>
        /// <param name="message">This out parameter will be set to the next message that is received. If there's a time-out or an error, this will be set to null.</param>
        /// <param name="messageEventArgs">RabbitMq event args associated with the read message. Contains information such as ReplyTo queue name and other message metadata.</param>
        /// <returns>True to indicate successful reception of a message. False to indicate either a time-out or an error.</returns>
        public bool ReadNextMessage(TimeSpan? timeOut, out TMessage message, out BasicDeliverEventArgs messageEventArgs)
        {
            if (!timeOut.HasValue)
            {
                messageEventArgs = _consumer.Queue.Dequeue();
            }
            else
            {
                if (!_consumer.Queue.Dequeue(timeOut.HasValue ? (int)timeOut.Value.TotalMilliseconds : 0, out messageEventArgs))
                {
                    message = default(TMessage);
                    return false;
                }
            }

            message = _messageEncoder.Decode(messageEventArgs.Body);
            return true;
        }
        #endregion 

        #endregion
    }
}
