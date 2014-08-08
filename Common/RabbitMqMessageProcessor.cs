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
    public class RabbitMqMessageProcessor<TMessage, TResponse> : IDisposable
    {
        private RabbitMqReadMessageQueue<TMessage> _inQueue;

        private readonly Func<TMessage, TResponse> _processMessage;
        private readonly IObjectEncoder<TResponse> _responseEncoder;
        private bool _stop;
        private readonly object _sendMessageLock = new object();

        public RabbitMqMessageProcessor(RabbitMqReadMessageQueue<TMessage> inQueue, Func<TMessage, TResponse> processMessage)
        {
            _processMessage = processMessage;
            _inQueue = inQueue;
            _responseEncoder = new JsonObjectEncoder<TResponse>();
        }

        private void ProcessSingleMessage(TimeSpan timeOut)
        {
            TMessage message;
            BasicDeliverEventArgs messageEventArgs;
            if (!_inQueue.Get(timeOut, out message, out messageEventArgs))
                return;

            "".ToString();

            Task.Run<TResponse>(() =>
            {
                try
                {
                    return _processMessage(message);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error in service: " + exception.Message);
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

                        //TODO: Lock may not be necessary if channel ops are thread-safe.
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
                        Console.WriteLine("Error sending MQ message: " + exception.Message);
                    }
                }
            });
        }

        public void Run()
        {
            while (!_stop)
            {
                this.ProcessSingleMessage(TimeSpan.FromMilliseconds(100));
            }
        }

        public void Dispose()
        {
            _stop = true;
            _inQueue.Dispose();
            _inQueue = null;
        }

        ~RabbitMqMessageProcessor()
        {
            this.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
