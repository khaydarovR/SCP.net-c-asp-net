using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace SCP.Application.Services;

public class RabbitMqService
{
    private ConnectionFactory _factory;
    private IConnection _connection;
    private IModel _channel;


    public RabbitMqService()
    {

        try
        {
            _factory = new ConnectionFactory() { HostName = "localhost", UserName = "admin", Password = "admin" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _ = _channel.QueueDeclare(queue: "MyQueue",
                           durable: false,
                           exclusive: false,
                           autoDelete: false,
                           arguments: null);
        }
        catch (Exception ex)
        {
            Console.WriteLine("RabbitMQ is DOWN");
        }
    }

    public void SendMessage(object obj)
    {
        var message = JsonConvert.SerializeObject(obj);
        SendMessage(message);
    }

    public void SendMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "",
                       routingKey: "MyQueue",
                       basicProperties: null,
                       body: body);
    }
}
