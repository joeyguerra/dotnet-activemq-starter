namespace StarterActiveMqDotnet;

using System;
using System.Threading;
using Apache.NMS;
using Apache.NMS.Util;

public class Program
{
    protected static AutoResetEvent semaphore = new AutoResetEvent(false);
    protected static ITextMessage? message = null;
    protected static TimeSpan receiveTimeout = TimeSpan.FromSeconds(10);
    public static void Main(string[] args)
    {
        Uri connecturi = new Uri("activemq:tcp://localhost:61616");
        Console.WriteLine("About to connect to " + connecturi);
        IConnectionFactory factory = new NMSConnectionFactory(connecturi);
        using IConnection connection = factory.CreateConnection("artemis", "artemis");
        using ISession session = connection.CreateSession();
        IDestination destination = SessionUtil.GetDestination(session, "queue://FOO.BAR");
        Console.WriteLine("Using destination: " + destination);
        using IMessageProducer producer = session.CreateProducer(destination);        
        producer.DeliveryMode = MsgDeliveryMode.Persistent;
        ITextMessage request = session.CreateTextMessage("Hello World!");
        request.NMSCorrelationID = "abc";
        request.Properties["NMSXGroupID"] = "cheese";
        request.Properties["myHeader"] = "Cheddar";
        producer.Send(request);
        do {
            semaphore.WaitOne(1000);
        } while (true);
    }
}