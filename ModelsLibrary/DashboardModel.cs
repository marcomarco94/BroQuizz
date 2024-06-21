using System.ComponentModel.DataAnnotations;

namespace ModelsLibrary;

public class DashboardModel
{
    [Key] public int Id { get; set; }

    public string? UserId { get; set; }

    public int QuestionId { get; set; }
    public QuestionModel? Question { get; set; }
    public int WrongAnswersCount { get; set; }
    public int CorrectAnswersCount { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}