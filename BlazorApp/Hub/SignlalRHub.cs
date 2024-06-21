using Microsoft.AspNetCore.SignalR;
using ModelsLibrary;

namespace BlazorApp.Hub;

/// <summary>
///     Provides methods for managing the SignalR hub.
/// </summary>
public class SignlalRHub : Microsoft.AspNetCore.SignalR.Hub
{
    public const string HubUrl = "/gaminghub";
    private static readonly List<PlayerModel?> ConnectedUsers = new();
    private readonly SignalRGameServiceProvider _gameServiceProvider;


    public SignlalRHub(SignalRGameServiceProvider gameServiceProvider)
    {
        _gameServiceProvider = gameServiceProvider;
    }

    /// <summary>
    ///     Gets the connected users.
    /// </summary>
    /// <returns>A list of connected users.</returns>
    public List<PlayerModel?> GetConnectedUsers()
    {
        return ConnectedUsers;
    }

    /// <summary>
    ///     Updates the group of the player and starts the game if there are enough players in the group.
    /// </summary>
    /// <param name="player">The player to update.</param>
    public async Task UpdateGroup(PlayerModel? player)
    {
        var oldPlayer = ConnectedUsers.FirstOrDefault(p => p?.ConnectionId == player?.ConnectionId);
        if (oldPlayer != null)
        {
            ConnectedUsers.Remove(oldPlayer);
            if (oldPlayer.RoomId != null)
            {
                if (oldPlayer.ConnectionId != null)
                    await Groups.RemoveFromGroupAsync(oldPlayer.ConnectionId, oldPlayer.RoomId);
                if (!ConnectedUsers.Any(c => c?.RoomId == oldPlayer.RoomId))
                    await StopGame(oldPlayer.RoomId, false);
                else await RemovePlayer(player, oldPlayer.RoomId);
            }
        }

        ConnectedUsers.Add(player);
        if (player?.RoomId != null && player.ConnectionId != null)
                await Groups.AddToGroupAsync(player.ConnectionId, player.RoomId);

        var playersInGroup = ConnectedUsers.Where(p => p?.RoomId == player?.RoomId).ToList();
        if (playersInGroup.Count > 0) await StartGame(playersInGroup, player?.RoomId);

        await Clients.All.SendAsync("ConnectedUsersChanged", ConnectedUsers);
    }

    /// <summary>
    ///     Removes the player from the group and stop the game if the player is the last one in the group.
    /// </summary>
    /// <param name="player">The player to remove from the group.</param>
    public async Task LeaveGroup(PlayerModel? player)
    {
        ConnectedUsers.Remove(player);
        if (player?.RoomId != null && player.ConnectionId != null)
                await Groups.RemoveFromGroupAsync(player.ConnectionId, player.RoomId);
        if (!ConnectedUsers.Any(c => c?.RoomId == player?.RoomId))
            await StopGame(player?.RoomId, false);
        else await RemovePlayer(player, player?.RoomId);

        await Clients.All.SendAsync("ConnectedUsersChanged", ConnectedUsers);
    }

    /// <summary>
    ///     Sends a message to the group.
    /// </summary>
    /// <param name="player">The player sending the message.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="roomId">The room ID.</param>
    public async Task SendToGroup(PlayerModel player, string message, string roomId)
    {
        await Clients.Group(roomId).SendAsync("SendToGroup", player, message);
    }


    /// <summary>
    ///     Starts a  game.
    /// </summary>
    /// <param name="playersInGroup"></param>
    /// <param name="roomId">The room ID.</param>
    private Task StartGame(List<PlayerModel?> playersInGroup, string? roomId)
    {
        var (gameService, gameStarted) = _gameServiceProvider.GetGameService(roomId);
        if (gameStarted) return Task.Run(() => gameService.SendGameStarted(playersInGroup));
        if (!gameStarted) return Task.Run(() => gameService.StartGame(playersInGroup, roomId));
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Starts a single player game.
    /// </summary>
    /// <param name="player">The player starting the game.</param>
    /// <param name="roomId">The room ID.</param>
    public Task StartSinglePlayerGame(PlayerModel? player, string? roomId)
    {
        var (gameService, gameStarted) = _gameServiceProvider.GetGameService(Context.ConnectionId);
        if (!gameStarted && player != null && roomId != null)
        {
            var playerList = new List<PlayerModel?> { player };
            return Task.Run(() => gameService.StartGame(playerList, roomId));
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Updates the game state.
    /// </summary>
    /// <param name="game">The updated game state.</param>
    /// <param name="roomId">The room ID.</param>
    public async Task UpdateGame(GameModel game, string roomId)
    {
        await Clients.Group(roomId).SendAsync("UpdateGame", game);
    }

    /// <summary>
    ///     Removes a player from the game.
    /// </summary>
    /// <param name="player">The player to remove.</param>
    /// <param name="roomId">The room ID.</param>
    public Task RemovePlayer(PlayerModel? player, string? roomId)
    {
        var (gameService, gameStarted) = _gameServiceProvider.GetGameService(roomId);
        if (gameStarted) gameService.RemovePlayer(player);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Stops the game.
    /// </summary>
    /// <param name="roomId">The room ID.</param>
    /// <param name="singleMode">Indicates whether the game is in single player mode.</param>
    public async Task StopGame(string? roomId, bool singleMode)
    {
        if (singleMode)
        {
            var (gameService, gameStarted) = _gameServiceProvider.GetGameService(Context.ConnectionId);
            if (gameStarted) await gameService.EndGame();
        }
        else
        {
            var (gameService, gameStarted) = _gameServiceProvider.GetGameService(roomId);
            if (gameStarted)
            {
                await gameService.EndGame();
            }
        }
    }

    /// <summary>
    ///     Updates the selected answer of a player.
    /// </summary>
    /// <param name="player">The player who selected the answer.</param>
    public void UpdateSelectedAnswer(PlayerModel player)
    {
        if (player.PlayingSingleMode)
        {
            var gameService = _gameServiceProvider.GetGameService(player.ConnectionId).GameService;
            gameService.UpdateSelectedAnswer(player);
        }
        else
        {
            var gameService = _gameServiceProvider.GetGameService(player.RoomId).GameService;
            gameService.UpdateSelectedAnswer(player);
        }
    }

    /// <summary>
    ///     Handles the event when a connection is established.
    /// </summary>
    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"{Context.ConnectionId} connected");
        return base.OnConnectedAsync();
    }

    /// <summary>
    ///     Handles the event when a connection is disconnected.
    /// </summary>
    /// <param name="e">The exception that caused the disconnection, if any.</param>
    public override async Task OnDisconnectedAsync(Exception? e)
    {
        var player = ConnectedUsers.FirstOrDefault(p => p?.ConnectionId == Context.ConnectionId);
        if (player != null) await LeaveGroup(player);
        Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
        await base.OnDisconnectedAsync(e);
    }
}