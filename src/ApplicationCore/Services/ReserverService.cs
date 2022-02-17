using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class ReserverService : IReserverService
{
    private readonly string _reserverApiUrl = string.Empty;
    private readonly string _sbConnectinString = string.Empty;
    private readonly HttpClient _httpClient;
    private readonly IRepository<Basket> _basketRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<CatalogItem> _itemRepository;


    public ReserverService(IRepository<Basket> basketRepository,
        IRepository<CatalogItem> itemRepository,
        IRepository<Order> orderRepository,
        IUriComposer uriComposer)
    {
        _orderRepository = orderRepository;
        _basketRepository = basketRepository;
        _itemRepository = itemRepository;
        _reserverApiUrl = uriComposer.ReserverFunctionUrl();
        _sbConnectinString = uriComposer.SBConnectionString();
        _httpClient = new HttpClient();
    }
    public async Task ReserveOrderItemsAsync(string path)
    {
        MultipartFormDataContent httpContent = new MultipartFormDataContent();

        FileStream fs = File.OpenRead(path);
        httpContent.Add(new StreamContent(fs), "file", Path.GetFileName(path));

        var response = await _httpClient.PostAsync(_reserverApiUrl, httpContent);

        // throw exception if sending failed
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                "failed to upload file"
            );
        }
        else
        {
            Console.WriteLine(
                    "Uploaded new file"
            );
        }
    }


    public async Task DeliverOrderAsync(int orderId)
    {
        var orderSpec = new OrderWithItemsByIdSpec(orderId);
        var order = await _orderRepository.GetBySpecAsync(orderSpec);

        var objecttoSave = new
        {
            id = Guid.NewGuid(),
            document = order
        };

        var httpContent = new StringContent(JsonConvert.SerializeObject(objecttoSave), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_reserverApiUrl, httpContent);

        // throw exception if sending failed
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                "failed to upload file " + response.ReasonPhrase
            );
        }
        else
        {
            Console.WriteLine(
                    "Uploaded new file"
            );
        }
    }

    public async Task SendMessageToServiceBus(int orderId)
    {
        var client = new ServiceBusClient(_sbConnectinString);
        var sender = client.CreateSender("eshoponwebq1");

        var orderSpec = new OrderWithItemsByIdSpec(orderId);
        var order = await _orderRepository.GetBySpecAsync(orderSpec);

        var objecttoSave = new
        {
            id = Guid.NewGuid(),
            document = order
        };


        var httpContent = JsonConvert.SerializeObject(objecttoSave);
        await sender.SendMessageAsync(new ServiceBusMessage(httpContent), CancellationToken.None);
    }
}
