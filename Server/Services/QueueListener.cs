using Azure.Storage.Queues;
using BlazorWebAssemblySignalRApp.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BlazorWebAssemblySignalRApp.Server.Services
{
    public class QueueListener
    {
        private readonly QueueClient client;
        //private readonly IChatHub chatHub; IHubContext<ChatHub, IChatHub> chatHub;
        private readonly ChatHubWrapper chatHub;

        public QueueListener(ChatHubWrapper chatHub)
        {
            client = new QueueClient("UseDevelopmentStorage=true", "problemsource-sync");
            this.chatHub = chatHub;
        }

        public async Task Receive(CancellationToken cancellationToken)
        {
            try
            {
                var accessResolver = new AccessResolver();
                var response = await client.ReceiveMessagesAsync(cancellationToken);
                var msgs = response.Value;
                foreach (var msg in msgs)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var trainingId = "AB";
                    var body = System.Text.Encoding.UTF8.GetString(msg.Body);

                    // find which clients should receive events
                    var sendToIds = chatHub.UserConnections
                        .Where(kv => accessResolver.HasReadAccess(kv.Value, trainingId))
                        .Select(kv => kv.Key);

                    await chatHub.Hub.Clients.Clients(sendToIds).ReceiveMessage(trainingId, body);

                    client.DeleteMessage(msg.MessageId, msg.PopReceipt);
                }
            }
            catch (Exception ex)
            { }
        }
    }

    public class AccessResolver
    {
        public bool HasReadAccess(IClientUser user, string trainingId)
        {
            return true;
        }
    }
}
