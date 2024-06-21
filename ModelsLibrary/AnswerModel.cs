using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelsLibrary;

public class AnswerModel : IQuestionnaire
{
    [ForeignKey("QuestionnaireModel")] public int QuestionnaireModelId { get; set; }

    public bool CorrectAnswer { get; set; } = false;
    public int UpVoteCount { get; set; }
    public int DownVoteCount { get; set; }

    [Key] public int Id { get; set; }

    [Required] [Length(2, 500)] public string? Text { get; set; } = string.Empty;

    public bool Archived { get; set; } = false;
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}