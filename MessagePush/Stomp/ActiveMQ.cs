using Apache.NMS;
using Base.Events;
using System;

namespace MessagePush.Stomp
{
    class ActiveMQ : IEventQueue
    {
        protected static TimeSpan receiveTimeout = TimeSpan.FromSeconds(10);

        private IConnectionFactory factory;
        private IConnection connection;
        private ISession session;

        public ActiveMQ()
        {
            Uri connecturi = new Uri("activemq:tcp://activemqhost:61616");
            Console.WriteLine("About to connect to " + connecturi);
            // NOTE: ensure the nmsprovider-activemq.config file exists in the executable folder.
            factory = new NMSConnectionFactory(connecturi);
            connection = factory.CreateConnection();
            session = connection.CreateSession();
        }

        public void Publish(IEvent @event, string msg)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(IEvent @event)
        {
            IDestination destination = session.GetTopic("FOO.BAR");
            IMessageConsumer consumer = session.CreateConsumer(destination);
            consumer.Listener += new MessageListener((m) => OnMessage(@event, m));
        }

        public void Unsubscribe(IEvent @event)
        {
            //session.DeleteDestination();
        }

        private void OnMessage(IEvent @event, IMessage message)
        {

        }

    }
}
