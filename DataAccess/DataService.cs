using DataAccess.Repository;
using ModelsLibrary;

namespace DataAccess;

public class DataService : IDataService
{
    private readonly BaseRepository<AnswerModel> _answerRepository;
    private readonly BaseRepository<DashboardModel> _dashboardRepository;
    private readonly BaseRepository<QuestionModel> _questionRepository;
    private readonly BaseRepository<TopicModel> _topicRepository;

    public DataService(QuizDbContext context)
    {
        _answerRepository = new BaseRepository<AnswerModel>(context);
        _questionRepository = new BaseRepository<QuestionModel>(context);
        _topicRepository = new BaseRepository<TopicModel>(context);
        _dashboardRepository = new BaseRepository<DashboardModel>(context);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TopicModel?>> GetTopicsAsync()
    {
        return await _topicRepository.GetByConditionAsync(t => t != null && t.Archived == false);
    }

    /// <inheritdoc />
    public async Task AddTopicAsync(TopicModel? topic)
    {
        await _topicRepository.AddAsync(topic);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QuestionModel?>> GetQuestionsByTopicAsync(int topicId)
    {
        return await _questionRepository.GetByConditionAsync(t => t != null
                                                                  && t.TopicModelId == topicId
                                                                  && t.Archived == false);
    }

    /// <inheritdoc />
    public async Task<List<QuestionModel>> GetQuizByTopicIdAsync(int topicId)
    {
        List<QuestionModel> quiz = new();
        var questions = await GetQuestionsByTopicAsync(topicId);
        foreach (var question in questions)
        {
            var allAnswers = await GetAnswersByQuestionAsync(question.Id);
            var correctAnswer = allAnswers.FirstOrDefault(a => a.CorrectAnswer);
            var incorrectAnswers = allAnswers.Where(a => !a.CorrectAnswer)
                .OrderBy(a => Guid.NewGuid())
                .Take(3)
                .ToList();

            if (correctAnswer != null && incorrectAnswers.Count >= 1)
            {
                var answers = new List<AnswerModel?> { correctAnswer };
                answers.AddRange(incorrectAnswers);
                answers = answers.OrderBy(a => Guid.NewGuid()).ToList();
                var quizModel = new QuestionModel
                {
                    Id = question.Id,
                    Text = question.Text,
                    Answers = answers
                };
                quiz.Add(quizModel);
            }
        }
        return quiz;
    }


    /// <inheritdoc />
    public async Task AddQuestionToTopic(int topicId, QuestionModel? question)
    {
        if (question != null)
        {
            question.TopicModelId = topicId;
            await _questionRepository.AddAsync(question);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AnswerModel?>> GetAnswersByQuestionAsync(int questionId)
    {
        return await _answerRepository.GetByConditionAsync(a => a != null
                                                                && a.QuestionnaireModelId == questionId
                                                                && a.Archived == false);
    }

    /// <inheritdoc />
    public async Task<TopicModel?> GetTopicByQuestionId(int questionId)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question != null)
        {
            var topic = await _topicRepository.GetByIdAsync(question.TopicModelId);
            return topic;
        }

        return null;
    }

    /// <inheritdoc />
    public async Task AddAnswersToQuestionAsync(int questionId, AnswerModel? answer)
    {
        if (answer != null)
        {
            answer.QuestionnaireModelId = questionId;
            await _answerRepository.AddAsync(answer);
        }
    }

    /// <inheritdoc />
    public async Task SetTopicArchivedAsync(int topicId)
    {
        var topic = await _topicRepository.GetByIdAsync(topicId);
        if (topic != null)
        {
            topic.Archived = true;
            await _topicRepository.UpdateAsync(topic);
        }

        var questions = await _questionRepository.GetByConditionAsync(q => q != null && q.TopicModelId == topicId);
        foreach (var question in questions)
            if (question != null)
                await SetQuestionArchivedAsync(question.Id);
    }

    /// <inheritdoc />
    public async Task SetQuestionArchivedAsync(int questionId)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);
        if (question != null)
        {
            question.Archived = true;
            await _questionRepository.UpdateAsync(question);
        }

        var answers = await _answerRepository.GetByConditionAsync(a => a != null && a.QuestionnaireModelId == questionId);
        foreach (var answer in answers)
            if (answer != null)
                await SetAnswerArchivedAsync(answer.Id);
    }

    /// <inheritdoc />
    public async Task SetAnswerArchivedAsync(int answerId)
    {
        var answer = await _answerRepository.GetByIdAsync(answerId);
        if (answer != null)
        {
            answer.Archived = true;
            await _answerRepository.UpdateAsync(answer);
        }
    }

    /// <inheritdoc />
    public async Task UpdateTopicAsync(TopicModel? topic)
    {
        await _topicRepository.UpdateAsync(topic);
    }

    /// <inheritdoc />
    public async Task UpdateQuestionAsync(QuestionModel? question)
    {
        await _questionRepository.UpdateAsync(question);
    }

    /// <inheritdoc />
    public async Task UpdateAnswerAsync(AnswerModel? answer)
    {
        await _answerRepository.UpdateAsync(answer);
    }

    /// <inheritdoc />
    public async Task UpdateCorrectAnswer(int answerId)
    {
        var questionId = ((await _answerRepository.GetByIdAsync(answerId))!).QuestionnaireModelId;
        var oldAnswers = await GetAnswersByQuestionAsync(questionId);
        var oldCorrectAnswers = oldAnswers.Where(a => a != null && a.CorrectAnswer).ToList();
        foreach (var answer in oldCorrectAnswers)
        {
            if (answer != null)
            {
                answer.CorrectAnswer = false;
                await _answerRepository.UpdateAsync(answer);
            }
        }

        var newCorrectAnswer = await _answerRepository.GetByIdAsync(answerId);
        if (newCorrectAnswer != null)
        {
            newCorrectAnswer.CorrectAnswer = true;
            await _answerRepository.UpdateAsync(newCorrectAnswer);
        }
    }

    /// <inheritdoc />
    public async Task<List<DashboardModel?>> GetDashBoardByUserAsync(string? userId)
    {
        var result = await _dashboardRepository.GetByConditionAsync(d => d != null && d.UserId == userId);
        return result.ToList();
    }

    /// <inheritdoc />
    public async Task UpdateUserDashboardAsync(string userId, int questionId, bool correctAnswer)
    {
        var dashboard =
            (await _dashboardRepository.GetByConditionAsync(d => d != null && d.UserId == userId && d.QuestionId == questionId))
            .FirstOrDefault();
        if (dashboard != null)
        {
            if (correctAnswer)
                dashboard.CorrectAnswersCount++;
            else
                dashboard.WrongAnswersCount++;
            await _dashboardRepository.UpdateAsync(dashboard);
        }
        else
        {
            dashboard = new DashboardModel
            {
                UserId = userId,
                QuestionId = questionId,
                CorrectAnswersCount = correctAnswer ? 1 : 0,
                WrongAnswersCount = correctAnswer ? 0 : 1
            };
            Console.WriteLine("Dashboard: " + dashboard);
            await _dashboardRepository.AddAsync(dashboard);
            Console.WriteLine("Dashboard finish: " + dashboard);
        }
    }
}