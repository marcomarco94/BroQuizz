using DataAccess;
using Microsoft.EntityFrameworkCore;
using ModelsLibrary;
using Xunit;

namespace Tests;

public class DataServiceTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly DataService _dataService;

    public DataServiceTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options;

        _context = new QuizDbContext(options);
        _dataService = new DataService(_context);
    }

    public void Dispose()
    {
        ClearDatabase();
        _context.Dispose();
    }

    private void ClearDatabase()
    {
        foreach (var entity in _context.Topics) _context.Topics.Remove(entity);

        foreach (var entity in _context.Questions) _context.Questions.Remove(entity);

        foreach (var entity in _context.Answers) _context.Answers.Remove(entity);

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetTopicsAsync_ReturnsAllTopics()
    {
        // Arrange
        var topics = new List<TopicModel>
        {
            new() { Id = 1, Text = "Topic1", Archived = false },
            new() { Id = 2, Text = "Topic2", Archived = false }
        };

        foreach (var topic in topics) await _context.Topics.AddAsync(topic);

        await _context.SaveChangesAsync();

        // Act
        var result = await _dataService.GetTopicsAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task AddTopicAsync_AddsNewTopic()
    {
        // Arrange
        var newTopic = new TopicModel { Text = "NewTopic", Archived = false };

        // Act
        await _dataService.AddTopicAsync(newTopic);
        var result = await _context.Topics.FirstOrDefaultAsync(t => t.Text == "NewTopic");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NewTopic", result.Text);
    }

    [Fact]
    public async Task GetQuestionsByTopicAsync_ReturnsQuestionsForTopic()
    {
        // Arrange
        var topic = new TopicModel { Id = 1, Text = "Topic1", Archived = false };
        await _context.Topics.AddAsync(topic);
        await _context.SaveChangesAsync();

        var questions = new List<QuestionModel>
        {
            new() { Text = "Question1", TopicModelId = 1, Archived = false },
            new() { Text = "Question2", TopicModelId = 1, Archived = false }
        };

        foreach (var question in questions) await _context.Questions.AddAsync(question);

        await _context.SaveChangesAsync();

        // Act
        var result = await _dataService.GetQuestionsByTopicAsync(1);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task AddQuestionToTopic_AddsQuestionToTopic()
    {
        // Arrange
        var topic = new TopicModel { Id = 1, Text = "Topic1", Archived = false };
        await _context.Topics.AddAsync(topic);
        await _context.SaveChangesAsync();

        var newQuestion = new QuestionModel { Text = "NewQuestion", Archived = false };

        // Act
        await _dataService.AddQuestionToTopic(1, newQuestion);
        var result = await _context.Questions.FirstOrDefaultAsync(q => q.Text == "NewQuestion");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NewQuestion", result.Text);
        Assert.Equal(1, result.TopicModelId);
    }

    [Fact]
    public async Task GetQuizByTopicIdAsync_ReturnsQuizForTopic()
    {
        // Arrange
        var topic = new TopicModel { Id = 1, Text = "Topic1", Archived = false };
        await _context.Topics.AddAsync(topic);
        await _context.SaveChangesAsync();

        var questions = new List<QuestionModel>
        {
            new() { Id = 1, Text = "Question1", TopicModelId = 1, Archived = false },
            new() { Id = 2, Text = "Question2", TopicModelId = 1, Archived = false }
        };

        foreach (var question in questions) await _context.Questions.AddAsync(question);

        await _context.SaveChangesAsync();

        var answers = new List<AnswerModel>
        {
            new() { Id = 1, Text = "Answer1", QuestionnaireModelId = 1, CorrectAnswer = true, Archived = false },
            new() { Id = 2, Text = "Answer2", QuestionnaireModelId = 1, CorrectAnswer = false, Archived = false },
            new() { Id = 3, Text = "Answer3", QuestionnaireModelId = 2, CorrectAnswer = true, Archived = false },
            new() { Id = 4, Text = "Answer4", QuestionnaireModelId = 2, CorrectAnswer = false, Archived = false }
        };

        foreach (var answer in answers) await _context.Answers.AddAsync(answer);

        await _context.SaveChangesAsync();

        // Act
        var result = await _dataService.GetQuizByTopicIdAsync(1);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, q => Assert.Equal(2, q.Answers.Count));
    }

    [Fact]
    public async Task AddAnswersToQuestionAsync_AddsAnswerToQuestion()
    {
        // Arrange
        var question = new QuestionModel { Id = 1, Text = "Question1", TopicModelId = 1, Archived = false };
        await _context.Questions.AddAsync(question);
        await _context.SaveChangesAsync();

        var newAnswer = new AnswerModel { Text = "NewAnswer", CorrectAnswer = true, Archived = false };

        // Act
        await _dataService.AddAnswersToQuestionAsync(1, newAnswer);
        var result = await _context.Answers.FirstOrDefaultAsync(a => a.Text == "NewAnswer");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NewAnswer", result.Text);
        Assert.Equal(1, result.QuestionnaireModelId);
    }

    [Fact]
    public async Task UpdateTopicAsync_UpdatesTopic()
    {
        // Arrange
        var topic = new TopicModel { Id = 1, Text = "Topic1", Archived = false };
        await _context.Topics.AddAsync(topic);
        await _context.SaveChangesAsync();

        topic.Text = "UpdatedTopic";
        await _dataService.UpdateTopicAsync(topic);

        // Act
        var result = await _context.Topics.FindAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UpdatedTopic", result.Text);
    }

    [Fact]
    public async Task UpdateQuestionAsync_UpdatesQuestion()
    {
        // Arrange
        var question = new QuestionModel { Id = 1, Text = "Question1", TopicModelId = 1, Archived = false };
        await _context.Questions.AddAsync(question);
        await _context.SaveChangesAsync();

        question.Text = "UpdatedQuestion";
        await _dataService.UpdateQuestionAsync(question);

        // Act
        var result = await _context.Questions.FindAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UpdatedQuestion", result.Text);
    }

    [Fact]
    public async Task UpdateAnswerAsync_UpdatesAnswer()
    {
        // Arrange
        var answer = new AnswerModel
            { Id = 1, Text = "Answer1", QuestionnaireModelId = 1, CorrectAnswer = true, Archived = false };
        await _context.Answers.AddAsync(answer);
        await _context.SaveChangesAsync();

        answer.Text = "UpdatedAnswer";
        await _dataService.UpdateAnswerAsync(answer);

        // Act
        var result = await _context.Answers.FindAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UpdatedAnswer", result.Text);
    }

    [Fact]
    public async Task UpdateCorrectAnswer_UpdatesCorrectAnswer()
    {
        // Arrange
        var answer1 = new AnswerModel
            { Id = 1, Text = "Answer1", QuestionnaireModelId = 1, CorrectAnswer = true, Archived = false };
        var answer2 = new AnswerModel
            { Id = 2, Text = "Answer2", QuestionnaireModelId = 1, CorrectAnswer = false, Archived = false };
        await _context.Answers.AddRangeAsync(answer1, answer2);
        await _context.SaveChangesAsync();

        await _dataService.UpdateCorrectAnswer(2);

        // Act
        var result1 = await _context.Answers.FindAsync(1);
        var result2 = await _context.Answers.FindAsync(2);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.False(result1.CorrectAnswer);
        Assert.True(result2.CorrectAnswer);
    }

    [Fact]
    public async Task GetDashBoardByUserAsync_ReturnsDashboardForUser()
    {
        // Arrange
        var dashboard = new DashboardModel
            { Id = 1, UserId = "User1", QuestionId = 1, CorrectAnswersCount = 5, WrongAnswersCount = 3 };
        await _context.Dashboards.AddAsync(dashboard);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dataService.GetDashBoardByUserAsync("User1");

        // Assert
        Assert.Single(result);
        Assert.Equal(5, result[0].CorrectAnswersCount);
        Assert.Equal(3, result[0].WrongAnswersCount);
    }

    [Fact]
    public async Task UpdateUserDashboardAsync_UpdatesUserDashboard()
    {
        // Arrange
        var dashboard = new DashboardModel
            { Id = 1, UserId = "User1", QuestionId = 1, CorrectAnswersCount = 5, WrongAnswersCount = 3 };
        await _context.Dashboards.AddAsync(dashboard);
        await _context.SaveChangesAsync();

        await _dataService.UpdateUserDashboardAsync("User1", 1, true);

        // Act
        var result = await _context.Dashboards.FindAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.CorrectAnswersCount);
        Assert.Equal(3, result.WrongAnswersCount);
    }

    [Fact]
    public async Task GetDashBoardByUserAsync_ReturnsEmptyWhenNoDashboardForUser()
    {
        // Act
        var result = await _dataService.GetDashBoardByUserAsync("NonExistentUser");

        // Assert
        Assert.Empty(result);
    }
}