using System.Net.Sockets;
using Microsoft.AspNetCore.SignalR;

public class BpnHub : Hub
{
  public async Task JoinEntityView(string name, string id)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, $"{name}-{id}");
    await Clients.Caller.SendAsync("ReceiveMessage", $"You have joined the {name} group for {id}");
  }
  public async Task LeaveEntityView(string name, string id)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{name}-{id}");
    await Clients.Caller.SendAsync("ReceiveMessage", $"You have left the {name} group for {id}");
  }
  public async Task JoinGroupView(string name)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, $"{name}");
    await Clients.Caller.SendAsync("ReceiveMessage", $"You have joined the {name} group");
  }
  public async Task LeaveGroupView(string name)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{name}");
    await Clients.Caller.SendAsync("ReceiveMessage", $"You have left the {name} group");
  }
}
