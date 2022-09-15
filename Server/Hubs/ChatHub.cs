using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace BlazorWebAssemblySignalRApp.Server.Hubs
{
    public interface IChatHub
    {
        Task ReceiveMessage(string user, string message);
    }

    public class ChatHubWrapper
    {
        private readonly IHubContext<ChatHub, IChatHub> chatHub;
        private readonly IConnectionsRepository connections;
        private readonly UserResolver userResolver;

        public ChatHubWrapper(IHubContext<ChatHub, IChatHub> chatHub, IConnectionsRepository connections, UserResolver userResolver)
        {
            this.chatHub = chatHub;
            this.connections = connections;
            this.userResolver = userResolver;
        }

        public IHubContext<ChatHub, IChatHub> Hub => chatHub;

        public IDictionary<string, IClientUser> UserConnections => connections.Connections.ToDictionary(o => o.Key, o => userResolver.GetFromPrincipal(o.Value));

        //public IDictionary<string, ClaimsPrincipal?> Connections => ChatHub.Instance?.Connections ?? new Dictionary<string, ClaimsPrincipal?>();
        //public IDictionary<string, IClientUser> UserConnections => ChatHub.Instance? ?? new Dictionary<string, ClaimsPrincipal?>();
    }

    public interface IConnectionsRepository
    {
        void Add(string connectionId, ClaimsPrincipal? user);
        void Remove(string connectionId);
        IDictionary<string, ClaimsPrincipal?> Connections { get; }
    }

    public class ConnectionsRepository : IConnectionsRepository
    {
        private ConcurrentDictionary<string, ClaimsPrincipal?> connections = new();
        private object lockObject = new();

        public void Add(string connectionId, ClaimsPrincipal? user)
        {
            connections.AddOrUpdate(connectionId, user, (key, val) => user);
        }
        public void Remove(string connectionId)
        {
            _ = connections!.Remove(connectionId, out _);
        }

        public IDictionary<string, ClaimsPrincipal?> Connections
        {
            get
            {
                lock (lockObject)
                {
                    return connections.ToDictionary(o => o.Key, o => o.Value);
                }
            }
        }
    }


    public interface IClientUser
    {
        bool IsAuthenticated { get; }
    }
    public class AuthenticatedUser : IClientUser
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public bool IsAuthenticated => true;
    }
    public class UnauthenticatedUser : IClientUser
    {
        public bool IsAuthenticated => false;
    }

    public class UserResolver
    {
        public IClientUser GetFromCurrentPrincipal() => GetFromPrincipal(ClaimsPrincipal.Current);

        public IClientUser GetFromPrincipal(ClaimsPrincipal? principal)
        {
            if (principal?.Identity?.IsAuthenticated != true)
                return new UnauthenticatedUser();

            return new AuthenticatedUser { DisplayName = principal.Identity?.Name ?? "Unknown" };
        }
    }


    public class ChatHub : Hub<IChatHub>
    {
        private static ChatHub? instanceSingletonProtector;
        private readonly IConnectionsRepository connectionsRepository;

        public static ChatHub? Instance => instanceSingletonProtector;
        public ChatHub(IConnectionsRepository connectionsRepository)
        {
            if (instanceSingletonProtector != null)
                throw new Exception("Singleton only!");
            instanceSingletonProtector = this;
            this.connectionsRepository = connectionsRepository;
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.ReceiveMessage(user, message);
        }

        public override Task OnConnectedAsync()
        {
            connectionsRepository.Add(Context.ConnectionId, Context.User);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            connectionsRepository.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}