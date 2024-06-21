using System.ComponentModel.DataAnnotations;

namespace ModelsLibrary;

public class PlayerModel
{
    public string? UserId { get; set; }
    public string? ConnectionId { get; set; }
    public string? RoomId { get; set; }

    [Required]
    [StringLength(32, MinimumLength = 3)]
    public string? Username { get; set; }

    public int Score { get; set; }
    public AnswerModel? SelectedAnswer { get; set; }

    public bool PlayingSingleMode { get; set; }
}