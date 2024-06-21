using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelsLibrary;

public class QuestionModel : IQuestionnaire
{
    [ForeignKey("TopicModel")] public int TopicModelId { get; set; }

    public List<AnswerModel>? Answers { get; set; }

    [Key] public int Id { get; set; }

    [Required] [Length(5, 500)] public string? Text { get; set; } = string.Empty;

    public bool Archived { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}