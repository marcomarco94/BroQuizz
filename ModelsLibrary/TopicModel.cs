using System.ComponentModel.DataAnnotations;

namespace ModelsLibrary;

public class TopicModel : IQuestionnaire
{
    public List<QuestionModel>? Questions { get; set; }

    [Key] public int Id { get; set; }

    [Required] [Length(5, 100)] public string? Text { get; set; } = string.Empty;

    public bool Archived { get; set; } = false;
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}