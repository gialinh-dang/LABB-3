using System.Net;
using System.Net.Http;
using System.Text.Json;
using LABB_3.Model;

namespace LABB_3.Services;

public class OpenTriviaService
{
    // One HttpClient for the whole app (creating a new one every time is bad practice)
    private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

    // The API uses names like "response_code", this maps them to ResponseCode
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<List<TriviaCategory>> GetCategoriesAsync()
    {
        string json = await _httpClient.GetStringAsync("https://opentdb.com/api_category.php");
        var response = JsonSerializer.Deserialize<TriviaCategoryResponse>(json, _jsonOptions);

        if (response == null)
            throw new TriviaApiException("The server sent an empty answer.");

        return response.TriviaCategories.OrderBy(c => c.Name).ToList();
    }

    public async Task<List<Question>> GetQuestionsAsync(int categoryId, Difficulty difficulty, int amount)
    {
        string url = $"https://opentdb.com/api.php?amount={amount}&category={categoryId}" +
                     $"&difficulty={difficulty.ToString().ToLower()}&type=multiple";

        string json = await _httpClient.GetStringAsync(url);
        var response = JsonSerializer.Deserialize<TriviaQuestionResponse>(json, _jsonOptions);

        if (response == null)
            throw new TriviaApiException("The server sent an empty answer.");

        if (response.ResponseCode != 0)
            throw new TriviaApiException(GetResponseCodeMessage(response.ResponseCode));

        return response.Results.Select(ConvertToQuestion).ToList();
    }

    private static Question ConvertToQuestion(TriviaQuestion triviaQuestion)
    {
        // The API sends text like "Who&#039;s" - HtmlDecode turns it into "Who's"
        string query = WebUtility.HtmlDecode(triviaQuestion.Question);
        string correct = WebUtility.HtmlDecode(triviaQuestion.CorrectAnswer);
        var wrong = triviaQuestion.IncorrectAnswers.Select(a => WebUtility.HtmlDecode(a)).ToList();

        return new Question(query, correct, wrong[0], wrong[1], wrong[2]);
    }

    private static string GetResponseCodeMessage(int code)
    {
        return code switch
        {
            1 => "There are not enough questions for this category and difficulty. Try a smaller number.",
            2 => "The request had an invalid parameter.",
            3 => "Session token not found.",
            4 => "All questions for this session have already been used.",
            5 => "Too many requests. Please wait 5 seconds and try again.",
            _ => $"Unknown error from the server (code {code})."
        };
    }
}