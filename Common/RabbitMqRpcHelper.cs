﻿using System.Collections.Generic;
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
    public class RabbitMqRpcHelper<TMessage, TResponse> : IDisposable
    {
        private RabbitMqWriteMessageQueue<TMessage> _sendQueue;
        private RabbitMqReadMessageQueue<TResponse> _recieveQueue;

        public RabbitMqRpcHelper(ConnectionFactory connectionFactory)
        {
            _sendQueue = new RabbitMqWriteMessageQueue<TMessage>(connectionFactory, new MqConfiguration { ExchangeDurable = true, QueueDurable = true, MessageDurable = true, NoAck = false });
            _recieveQueue = new RabbitMqReadMessageQueue<TResponse>(connectionFactory, new MqConfiguration { ExchangeDurable = true, QueueDurable = false, MessageDurable = false, NoAck = true }, true);
        }

        public bool Call(TMessage message, out TResponse response, TimeSpan? timeOut, out Exception error)
        {
            _sendQueue.Write(message, _recieveQueue.QueueName);
            BasicDeliverEventArgs messageEventArgs;
            if (_recieveQueue.Get(timeOut, out response, out messageEventArgs))
            {
                error = null;
                return true;
            }

            //TODO: get more info here for the error.
            error = new Exception("Error sending/recieving from RabbitMq.");
            return false;
        }

        public void Dispose()
        {
            if (_sendQueue != null)
            {
                _sendQueue.Dispose();
                _sendQueue = null;
            }

            if (_recieveQueue != null)
            {
                _recieveQueue.Dispose();
                _recieveQueue = null;
            }
        }
    }
}
