using System.Net.Sockets;
using Microsoft.AspNetCore.SignalR;

public class BpnHub : Hub
{
  public async Task JoinBpnContext()
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, $"BpnContext");
    await Clients.Caller.SendAsync("ReceiveMessage", $"You have joined BpnContext");
  }
  public async Task LeaveBpnContext()
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"BpnContext");
    await Clients.Caller.SendAsync("ReceiveMessage", $"You have left BpnContext");
  }
  public async Task JoinBpnFeatureGroup(string bpnFeatureId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, $"BpnFeature-{bpnFeatureId}");
    await Clients.Caller.SendAsync("ReceiveMessage", $"You have joined BpnFeature group {bpnFeatureId}");
  }
  public async Task LeaveBpnFeatureGroup(string bpnFeatureId)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"BpnFeature-{bpnFeatureId}");
    await Clients.Caller.SendAsync("ReceiveMessage", $"You have left BpnFeature group {bpnFeatureId}");
  }
}
