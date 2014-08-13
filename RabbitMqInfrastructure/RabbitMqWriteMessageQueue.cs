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
    /// RabbitMq queue used for sending messages.
    /// </summary>
    /// <typeparam name="TMessage">Type of messages that will be sent on this queue.</typeparam>
    public class RabbitMqWriteMessageQueue<TMessage> : RabbitMqMessageQueueBase<TMessage>
    {
        #region Constructor
        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="RabbitMqWriteMessageQueue"/>.
        /// </summary>
        /// <param name="connectionFactory">RabbitMq connection factory used to create connections to the RabbitMq broker.</param>
        /// <param name="mqConfiguration">Configuration parameters for creation of RabbitMq entities such as exchanges, queues and messages.</param>
        public RabbitMqWriteMessageQueue(ConnectionFactory connectionFactory, MqConfiguration mqConfiguration)
            : base(connectionFactory, mqConfiguration, false)
        {
        } 
        #endregion

        #region Public Methods

        #region WriteMessage
        /// <summary>
        /// Sends a message on the queue associated with this object.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="replyQueueName">Specifies the name of the queue to which the reply will be sent by the processor of the message.</param>
        public void WriteMessage(TMessage message, string replyQueueName)
        {
            var publishProps = this.Channel.CreateBasicProperties();
            publishProps.ReplyTo = replyQueueName;
            publishProps.SetPersistent(this.MqConfiguration.MessageDurable);

            var messageExpiration = this.MqConfiguration.MessageExpiration;
            if (messageExpiration.HasValue)
                publishProps.Expiration = messageExpiration.Value.ToString();

            this.Channel.BasicPublish(ExchangeName, _queueName, publishProps, _messageEncoder.Encode(message));
        }
        #endregion 

        #endregion
    }
}
