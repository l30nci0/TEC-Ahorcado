using System;
using System.ServiceModel;
using LetterClashServer.Contracts;

namespace LetterClashClient.Services {
  public static class ServiceProxyManager {
    private static readonly string BASE_URL = 
      System.Configuration.ConfigurationManager.AppSettings["ServerBaseUrl"] ?? "http://localhost:64294/Services/";

    private static BasicHttpBinding GetBasicHttpBinding() {
      return new BasicHttpBinding {
        MaxReceivedMessageSize = 2147483647,
        MaxBufferSize = 2147483647,
        SendTimeout = TimeSpan.FromSeconds(20),
        ReceiveTimeout = TimeSpan.FromSeconds(20)
      };
    }

    private static WSDualHttpBinding GetWSDualHttpBinding() {
      return new WSDualHttpBinding {
        MaxReceivedMessageSize = 2147483647,
        SendTimeout = TimeSpan.FromSeconds(20),
        ReceiveTimeout = TimeSpan.FromMinutes(10), // Keep duplex channel alive longer
        Security = { Mode = WSDualHttpSecurityMode.None }
      };
    }

    public static IAutenticacionService GetAutenticacionService() {
      var binding = GetBasicHttpBinding();
      var endpoint = new EndpointAddress(new Uri(new Uri(BASE_URL), "AutenticacionService.svc"));
      var factory = new ChannelFactory<IAutenticacionService>(binding, endpoint);
      return factory.CreateChannel();
    }

    public static IJugadorService GetJugadorService() {
      var binding = GetBasicHttpBinding();
      var endpoint = new EndpointAddress(new Uri(new Uri(BASE_URL), "JugadorService.svc"));
      var factory = new ChannelFactory<IJugadorService>(binding, endpoint);
      return factory.CreateChannel();
    }

    public static ILobbyService GetLobbyService() {
      var binding = GetBasicHttpBinding();
      var endpoint = new EndpointAddress(new Uri(new Uri(BASE_URL), "LobbyService.svc"));
      var factory = new ChannelFactory<ILobbyService>(binding, endpoint);
      return factory.CreateChannel();
    }

    public static IPalabraService GetPalabraService() {
      var binding = GetBasicHttpBinding();
      var endpoint = new EndpointAddress(new Uri(new Uri(BASE_URL), "PalabraService.svc"));
      var factory = new ChannelFactory<IPalabraService>(binding, endpoint);
      return factory.CreateChannel();
    }

    public static IGameService GetGameService(IGameServiceCallback callbackHandler) {
      var binding = GetWSDualHttpBinding();
      var endpoint = new EndpointAddress(new Uri(new Uri(BASE_URL), "GameService.svc"));
      var instanceContext = new InstanceContext(callbackHandler);
      var factory = new DuplexChannelFactory<IGameService>(instanceContext, binding, endpoint);
      return factory.CreateChannel();
    }
  }
}
