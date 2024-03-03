using LogIndexerService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SCP.Domain.Entity;
using System.Text;

public class RabbitMqListener : BackgroundService
{
    private IConnection _connection;
    private IModel _channel;
    private ElastService elastService;

    public RabbitMqListener(ElastService elastService)
    {
        // Не забудьте вынести значения "localhost" и "MyQueue"
        // в файл конфигурации
        var factory = new ConnectionFactory() { HostName = "localhost", UserName = "log", Password = "log" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _ = _channel.QueueDeclare(queue: "MyQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        this.elastService = elastService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (ch, ea) =>
        {
            var content = Encoding.UTF8.GetString(ea.Body.ToArray());

            // Каким-то образом обрабатываем полученное сообщение
            Console.WriteLine($"Получено сообщение: {content}");
            var r = new Record { Title = content, Id = Guid.NewGuid() };
            var l = new ActivityLog { LogText = content, Id= Guid.NewGuid(), At = DateTime.UtcNow, Record = r, RecordId = r.Id};

            elastService.Insert(l, Service.GetIndexName());

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _ = _channel.BasicConsume("MyQueue", false, consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}