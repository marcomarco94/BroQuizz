using System.Collections.Concurrent;

namespace BlazorApp.Hub;

public class SignalRGameServiceProvider
{
    private readonly ConcurrentDictionary<string, SignalRGameService> _gameServices;
    private readonly IServiceProvider _serviceProvider;

    public SignalRGameServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _gameServices = new ConcurrentDictionary<string, SignalRGameService>();
    }

    public (SignalRGameService GameService, bool GameStarted) GetGameService(string? roomId)
    {
        if (roomId != null && _gameServices.TryGetValue(roomId, out var existingGameService))
            return (existingGameService, existingGameService.GameStarted);

        var gameService = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<SignalRGameService>();
        gameService.OnDispose = () => DisposeGameService(roomId).Wait();
        if (roomId != null) _gameServices[roomId] = gameService;
        return (gameService, gameService.GameStarted);
    }

    private async Task DisposeGameService(string? roomId)
    {
        if (roomId != null && _gameServices.TryRemove(roomId, out var gameService)) await gameService.DisposeAsync();
    }
}