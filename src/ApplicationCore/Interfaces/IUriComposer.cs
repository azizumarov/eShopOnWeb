namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

public interface IUriComposer
{
    string ComposePicUri(string uriTemplate);

    string ReserverFunctionUrl();

    string SBConnectionString();

}
