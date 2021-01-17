using HtmlAgilityPack;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ContentClawlet1
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                MyDbContext myDbContext = new MyDbContext();
                channel.QueueDeclare(queue: "amqcrawler",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var link = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" ContentCrawler1 Received {0}", link);
                    var url = link;

                    var httpClient = new HttpClient();
                    var html = await httpClient.GetStringAsync(url);
                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(html);



                    var divs = htmlDocument.DocumentNode.Descendants("section").Where(node => node.GetAttributeValue("class", "").Equals("section page-detail top-detail")).FirstOrDefault();
                    var title = divs.Descendants("div").FirstOrDefault().Descendants("h1").FirstOrDefault().InnerText;
                    var dess = divs.Descendants("div").FirstOrDefault().Descendants("article").FirstOrDefault().Descendants("p").ToList();
                    var description = "";
                    foreach (var des in dess)
                    {

                        description += des.InnerText;

                    }
                    var thumbnail = "";
                    if (divs.Descendants("div")
                        .FirstOrDefault()
                        .Descendants("figure").FirstOrDefault()
                        != null)
                    {
                        thumbnail = divs.Descendants("div")
                                        .FirstOrDefault()
                                        .Descendants("figure")
                                        .FirstOrDefault()
                                        .Descendants("img")
                                        .FirstOrDefault()
                                        .ChildAttributes("data-src")
                                        .FirstOrDefault().Value;

                    }




                    var news = new News
                    {
                        Title = title,
                        Thumb = thumbnail,
                        Description = description
                    };


                    
                    if (!myDbContext.News.Any(o => o.Title == news.Title))
                    {
                        myDbContext.News.Add(news);
                        myDbContext.SaveChanges();
                    }

                };
                channel.BasicConsume(queue: "amqcrawler",
                                             autoAck: true,
                                             consumer: consumer);
                Console.ReadLine();
            }
        }
    }
}

