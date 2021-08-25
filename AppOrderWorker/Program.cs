using AppOrderWorker.Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace AppOrderWorker
{
    class Program
    {
        static void Main(string[] args)
        {

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "orderQueue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        //convert byte em string
                        var message = Encoding.UTF8.GetString(body);
                        // convert string em objeto
                        var order = System.Text.Json.JsonSerializer.Deserialize<Order>(message);

                        Console.WriteLine($"Order {order.OrderNumber}|{order.ItemName} | {order.Price:N2}");

                    }
                    catch (Exception ex)
                    {

                        channel.BasicNack(ea.DeliveryTag, false, true); // deliveryTag carimbo de entrega, false : para dizer se é multiplo, true: recolocar o item na fila e deixar disponível
                    }
                   
                };
                channel.BasicConsume(queue: "orderQueue",
                                     autoAck: false, // se der algum erro não perde a mensagem
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
