using System.Security.Claims;
using BlazorApp.Hub;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using ModelsLibrary;

namespace BlazorApp.Services;

public class OnlineSerive : IAsyncDisposable
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly string? _hubUrl;
    private PlayerModel? _player;
    private HubConnectionState _previousState;

    public OnlineSerive(NavigationManager navigationManager, AuthenticationStateProvider authenticationStateProvider)
    {
        var baseUrl = navigationManager.BaseUri;
        _hubUrl = baseUrl.TrimEnd('/') + SignlalRHub.HubUrl;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public HubConnection? HubConnection { get; set; }

    public async ValueTask DisposeAsync()
    {
        if (HubConnection != null && HubConnection.State == HubConnectionState.Connected) await DisconnectAsync();
    }

    public event Action<HubConnectionState, HubConnectionState> ConnectionStateChanged;

    /// <summary>
    ///     Connects the player to the hub.
    /// </summary>
    /// <param name="player">The player to connect.</param>
    public async Task ConnectAsync(PlayerModel player)
    {
        HubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .Build();

        _previousState = HubConnection.State;
        await HubConnection.StartAsync();
        _player = player;
        var claim = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var id = claim.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (id != null) _player.UserId = id.Value;
        _player.ConnectionId = HubConnection.ConnectionId;

        if (_previousState != HubConnection.State)
        {
            ConnectionStateChanged?.Invoke(_previousState, HubConnection.State);
            _previousState = HubConnection.State;
        }
    }

    /// <summary>
    ///     Disconnects the player from the hub.
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (HubConnection != null && HubConnection.State == HubConnectionState.Connected)
        {
            _previousState = HubConnection.State;
            await HubConnection.StopAsync();
            await HubConnection.DisposeAsync();
        }
    }

    /// <summary>
    ///     Updates the group of the player.
    /// </summary>
    /// <param name="player">The player to update.</param>
    /// <param name="newRoomId">The new room ID.</param>
    public async Task UpdateGroupAsync(PlayerModel? player, string? newRoomId)
    {
        if (HubConnection != null && HubConnection.State == HubConnectionState.Connected)
        {
            player.RoomId = newRoomId;
            await HubConnection!.SendAsync("UpdateGroup", player);
        }
    }

    /// <summary>
    ///     Sends a message to the group.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="roomId">The room ID.</param>
    public async Task SendMessageAsync(string message, string? roomId)
    {
        if (HubConnection != null && HubConnection.State == HubConnectionState.Connected)
            await HubConnection.SendAsync("SendToGroup", _player, message, roomId);
    }

    /// <summary>
    ///     Sends the selected answer of the player.
    /// </summary>
    /// <param name="player">The player who selected the answer.</param>
    public async Task SendSelectedAnswerAsync(PlayerModel? player)
    {
        if (HubConnection != null && HubConnection.State == HubConnectionState.Connected)
            await HubConnection.SendAsync("UpdateSelectedAnswer", player);
    }
}