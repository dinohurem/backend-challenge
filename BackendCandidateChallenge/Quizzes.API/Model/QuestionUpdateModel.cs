namespace Quizzes.API.Model;

public class QuestionUpdateModel
{
    public string Text { get; set; }
    public int CorrectAnswerId { get; set; }
}