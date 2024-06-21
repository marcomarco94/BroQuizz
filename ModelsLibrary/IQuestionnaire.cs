namespace ModelsLibrary;

public interface IQuestionnaire
{
    public int Id { get; set; }
    public string? Text { get; set; }
    public bool Archived { get; set; }
    public DateTime DateCreated { get; set; }
}