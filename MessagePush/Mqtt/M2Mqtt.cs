using Base.Events;
using Base.Misc;
using System;
using System.ComponentModel.Composition;
using System.Security.Cryptography;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace MessagePush.Mqtt
{

    [Export(typeof(IEventQueue))]
    public class M2Mqtt : IEventQueue
    {

        private static readonly Logger Log = Logger.GetLogger<M2Mqtt>();

        private MqttClient client;

        public M2Mqtt()
        {
            if (Configuration.Uri == null)
                return;
            // create client instance 
            client = new MqttClient(Configuration.Uri);

            // register to message received 
            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            client.MqttMsgPublished += Client_MqttMsgPublished;
            client.ConnectionClosed += Client_ConnectionClosed;
            client.MqttMsgSubscribed += Client_MqttMsgSubscribed;
            client.MqttMsgUnsubscribed += Client_MqttMsgUnsubscribed;

            string clientId = Configuration.ClientId;
            if (clientId == null)
            {
                clientId = Guid.NewGuid().ToString();
                Configuration.ClientId = clientId;
            }
            clientId = Configuration.GroupId + "@@@" + clientId;
            string userName = "Signature|" + Configuration.AccessKey + "|" + Configuration.InstanceId;
            string passWord = HMACSHA1(Configuration.SecretKey, clientId);
            client.Connect(clientId, userName, passWord, true, Configuration.KeepAlivePeriod);
        }

        public void Publish(IEvent @event, string msg)
        {
            if (client == null)
                return;
            // publish a message on "/home/temperature" topic with QoS 2 
            client.Publish(Topic(@event.Topic),
                Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        }

        public void Subscribe(IEvent @event)
        {
            if (client == null)
                return;
            // subscribe to the topic "/home/temperature" with QoS 2 
            client.Subscribe(new string[] { Topic(@event.Topic) }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
        }

        public void Unsubscribe(IEvent @event)
        {
            if (client == null)
                return;
            client.Unsubscribe(new string[] { Topic(@event.Topic) });
        }

        #region Event Handlers

        private void Client_MqttMsgUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
        {
            Log.w("MqttMsgUnsubscribed " + e.MessageId);
        }

        private void Client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            Log.w("MqttMsgSubscribed " + e.MessageId);
        }

        private void Client_ConnectionClosed(object sender, EventArgs e)
        {
            Log.w("ConnectionClosed");
        }

        private void Client_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
        {
            Log.w("MqttMsgPublished " + e.MessageId + " " + e.IsPublished);
        }

        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            Log.w("MqttMsgPublishReceived " + e.Topic + " " + e.Message);
            //PublishEvent(e.Topic, new string(Encoding.UTF8.GetChars(e.Message)));
        }

        #endregion

        private static string Topic(string topic)
        {
            return Configuration.ParentTopic + topic + "-topic";
        }

        private static string HMACSHA1(string key, string dataToSign)
        {
            byte[] secretBytes = Encoding.UTF8.GetBytes(key);
            HMACSHA1 hmac = new HMACSHA1(secretBytes);
            byte[] dataBytes = Encoding.UTF8.GetBytes(dataToSign);
            byte[] calcHash = hmac.ComputeHash(dataBytes);
            string calcHashString = Convert.ToBase64String(calcHash);
            return calcHashString;
        }

    }
}
