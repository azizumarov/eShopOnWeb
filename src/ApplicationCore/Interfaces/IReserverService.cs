using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

public interface IReserverService
{
    Task ReserveOrderItemsAsync(string path);

    Task DeliverOrderAsync(int orderId);

    Task SendMessageToServiceBus(int orderId);
}
