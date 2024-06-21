using ModelsLibrary;

namespace DataAccess;

/// <summary>
///     Interface for the data service, providing methods for interacting with quiz data.
/// </summary>
public interface IDataService
{
    /// <summary>
    ///     Retrieves all topics asynchronously.
    /// </summary>
    /// <returns>A collection of topics.</returns>
    Task<IEnumerable<TopicModel?>> GetTopicsAsync();

    /// <summary>
    ///     Adds a new topic asynchronously.
    /// </summary>
    /// <param name="topic">The topic to add.</param>
    Task AddTopicAsync(TopicModel? topic);

    /// <summary>
    ///     Retrieves all questions for a specific topic asynchronously.
    /// </summary>
    /// <param name="topicId">The ID of the topic.</param>
    /// <returns>A collection of questions.</returns>
    Task<IEnumerable<QuestionModel?>> GetQuestionsByTopicAsync(int topicId);

    /// <summary>
    ///     Retrieves a quiz by topic ID asynchronously.
    /// </summary>
    /// <param name="topicId">The ID of the topic.</param>
    /// <returns>A list of questions for the quiz.</returns>
    Task<List<QuestionModel>> GetQuizByTopicIdAsync(int topicId);

    /// <summary>
    ///     Adds a question to a specific topic asynchronously.
    /// </summary>
    /// <param name="topicId">The ID of the topic.</param>
    /// <param name="question">The question to add.</param>
    Task AddQuestionToTopic(int topicId, QuestionModel? question);

    /// <summary>
    ///     Retrieves all answers for a specific question asynchronously.
    /// </summary>
    /// <param name="questionId">The ID of the question.</param>
    /// <returns>A collection of answers.</returns>
    Task<IEnumerable<AnswerModel?>> GetAnswersByQuestionAsync(int questionId);

    /// <summary>
    ///     Adds an answer to a specific question asynchronously.
    /// </summary>
    /// <param name="questionId">The ID of the question.</param>
    /// <param name="answer">The answer to add.</param>
    Task AddAnswersToQuestionAsync(int questionId, AnswerModel? answer);

    /// <summary>
    ///     Sets a specific topic as archived asynchronously.
    /// </summary>
    /// <param name="topicId">The ID of the topic.</param>
    Task SetTopicArchivedAsync(int topicId);

    /// <summary>
    ///     Sets a specific question as archived asynchronously.
    /// </summary>
    /// <param name="questionId">The ID of the question.</param>
    Task SetQuestionArchivedAsync(int questionId);

    /// <summary>
    ///     Sets a specific answer as archived asynchronously.
    /// </summary>
    /// <param name="answerId">The ID of the answer.</param>
    Task SetAnswerArchivedAsync(int answerId);

    /// <summary>
    ///     Updates a specific topic asynchronously.
    /// </summary>
    /// <param name="topic">The updated topic.</param>
    Task UpdateTopicAsync(TopicModel? topic);

    /// <summary>
    ///     Updates a specific question asynchronously.
    /// </summary>
    /// <param name="question">The updated question.</param>
    Task UpdateQuestionAsync(QuestionModel? question);

    /// <summary>
    ///     Updates a specific answer asynchronously.
    /// </summary>
    /// <param name="answer">The updated answer.</param>
    Task UpdateAnswerAsync(AnswerModel? answer);

    /// <summary>
    ///     Updates the correct answer for a question asynchronously.
    /// </summary>
    /// <param name="answerId">The ID of the answer.</param>
    Task UpdateCorrectAnswer(int answerId);

    /// <summary>
    ///     Retrieves the dashboard for a specific user asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of dashboard models for the user.</returns>
    Task<List<DashboardModel?>> GetDashBoardByUserAsync(string? userId);

    /// <summary>
    ///     Updates the user's dashboard asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="questionId">The ID of the question.</param>
    /// <param name="correctAnswer">Whether the answer was correct.</param>
    Task UpdateUserDashboardAsync(string userId, int questionId, bool correctAnswer);

    /// <summary>
    ///     Retrieves a topic by question ID asynchronously.
    /// </summary>
    /// <param name="questionId">The ID of the question.</param>
    /// <returns>The topic of the question.</returns>
    Task<TopicModel?> GetTopicByQuestionId(int questionId);
}