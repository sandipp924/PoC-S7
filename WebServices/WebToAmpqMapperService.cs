using AG.Services.Dtos;
using Funq;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Messaging;
using ServiceStack.RabbitMq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AG.WebServices
{
    public class WebToAmpqMapperService : Service
    {
        private static readonly string AmqpServerUri = ConfigUtils.GetConnectionString("AmqpServer");
        private static readonly TimeSpan TimeOut = TimeSpan.FromSeconds(10);

        private readonly RabbitMqServer _mqServer;
        private readonly IMessageQueueClient _mqClient;
        private readonly string _mqReplyToQueue;

        public WebToAmpqMapperService()
        {
            _mqServer = new RabbitMqServer(AmqpServerUri);
            _mqServer.Start();

            _mqClient = _mqServer.CreateMessageQueueClient();
            _mqReplyToQueue = _mqClient.GetTempQueueName();
        }

        public object Any(SymbologyInfoQuery symbologyInfoQuery)
        {
            return GetIml(symbologyInfoQuery);
        }

        private object GetIml<T>(T queryObject)
        {


            _mqClient.Publish(new Message<T>(queryObject)
            {
                ReplyTo = _mqReplyToQueue,
                RetryAttempts = 1,
            });

            var reply = _mqClient.Get<SymbologyInfo>(_mqReplyToQueue);
            if (reply != null)
            {
                _mqClient.Ack(reply);
                return reply.GetBody();
            }
            else
            {
                return null; // mq service time-out.
            }
        }
    }
}