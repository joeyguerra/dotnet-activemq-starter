namespace Consumer;
using System;
using Apache.NMS;
using Apache.NMS.Util;
public class Consumer {
    protected static AutoResetEvent semaphore = new AutoResetEvent(false);
    public static void Main(string[] args) {
        Console.WriteLine("Hello World!");
        Uri connecturi = new Uri("activemq:tcp://localhost:61616");
        Console.WriteLine("About to connect to " + connecturi);
        IConnectionFactory factory = new NMSConnectionFactory(connecturi);
        using IConnection connection = factory.CreateConnection("artemis", "artemis");
        using ISession session = connection.CreateSession();
        IDestination destination = SessionUtil.GetDestination(session, "queue://FOO.BAR");
        Console.WriteLine("Using destination: " + destination);
        using IMessageConsumer consumer = session.CreateConsumer(destination);
        consumer.Listener += new MessageListener(OnMessage);
        connection.Start();

        IDestination destination2 = SessionUtil.GetDestination(session, "queue://BAR.FOO");
        sendAcknowledgements(session, destination2);
        do {
            semaphore.WaitOne(1000);
        } while (true);
    }
    private static void sendAcknowledgements(ISession session, IDestination destination) {
        using IMessageProducer producer = session.CreateProducer(destination);
        producer.DeliveryMode = MsgDeliveryMode.Persistent;
        producer.RequestTimeout = TimeSpan.FromMilliseconds(500);
        producer.Send(session.CreateTextMessage("Hello World!"));
    }
    private static void OnMessage(IMessage message)
    {
        if (message is ITextMessage textMessage)
        {
            Console.WriteLine("Received message with ID:   " + textMessage.NMSMessageId);
            Console.WriteLine("Received message with text: " + textMessage.Text);
            Console.WriteLine("Received message with header: " + textMessage.Properties["myHeader"]);
            semaphore.Set();
        }
        else
        {
            Console.WriteLine("Received a non-text message: " + message);
        }
    }
}

