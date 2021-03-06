using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System;
using Quizzes.Domain.Dtos;
using Quizzes.Core;
using System.Threading.Tasks;

namespace Quizzes.API.Controllers;

// TODO: Add named controller which should result in route name and controller name as following: 
// [Route("api/[controller]")]
// [QuizzesController]

// TODO: Have one endpoint to update, one to create, one to get. EF should be taking care of the rest with correct mapping in the auto mapper.
[Route("api/quizzes")]
public class QuizController : Controller
{
    private readonly IQuizService _quizService;

    // TODO: I would remove this (no need for dependency injection) for all other endpoints (like it was done in the first 2 GET requests).
    private readonly IDbConnection _connection;

    public QuizController(IDbConnection connection, IQuizService quizService)
    {
        _connection = connection;
        _quizService = quizService;
    }

    // GET api/quizzes
    // TODO: Raw queries do have their pros and cons, but for small apps, I would say there are only cons.
    // TODO: Cons examples: Column names matching, must of getting data for all properties of an entity, related data needs to be included with Include operator.
    [HttpGet]
    public async Task<ActionResult<List<QuizResponseModel>>> Get()
    {
        return await _quizService.GetAllAsync();
    }

    // GET api/quizzes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<QuizResponseModel>> Get(int id)
    {
        try
        {
            return await _quizService.GetAsync(id);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    // TODO: Move all this logic to appropriate services.
    // POST api/quizzes
    [HttpPost]
    public IActionResult Post([FromBody] QuizCreateModel value)
    {
        var sql = $"INSERT INTO Quiz (Title) VALUES('{value.Title}'); SELECT LAST_INSERT_ROWID();";
        var id = _connection.ExecuteScalar(sql);
        return Created($"/api/quizzes/{id}", null);
    }

    // PUT api/quizzes/5
    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] QuizUpdateModel value)
    {
        const string sql = "UPDATE Quiz SET Title = @Title WHERE Id = @Id";
        int rowsUpdated = _connection.Execute(sql, new { Id = id, Title = value.Title });
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        const string sql = "DELETE FROM Quiz WHERE Id = @Id";
        int rowsDeleted = _connection.Execute(sql, new { Id = id });
        if (rowsDeleted == 0)
            return NotFound();
        return NoContent();
    }

    // POST api/quizzes/5/questions
    [HttpPost]
    [Route("{id}/questions")]
    public IActionResult PostQuestion(int id, [FromBody] QuestionCreateModel value)
    {
        try
        {
            const string sql = "INSERT INTO Question (Text, QuizId) VALUES(@Text, @QuizId); SELECT LAST_INSERT_ROWID();";
            var questionId = _connection.ExecuteScalar(sql, new { Text = value.Text, QuizId = id });
            return Created($"/api/quizzes/{id}/questions/{questionId}", null);
        }
        catch (Exception)
        {
            return NotFound();
            throw;
        }
    }

    // PUT api/quizzes/5/questions/6
    [HttpPut("{id}/questions/{qid}")]
    public IActionResult PutQuestion(int id, int qid, [FromBody] QuestionUpdateModel value)
    {
        const string sql = "UPDATE Question SET Text = @Text, CorrectAnswerId = @CorrectAnswerId WHERE Id = @QuestionId";
        int rowsUpdated = _connection.Execute(sql, new { QuestionId = qid, Text = value.Text, CorrectAnswerId = value.CorrectAnswerId });
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5/questions/6
    [HttpDelete]
    [Route("{id}/questions/{qid}")]
    public IActionResult DeleteQuestion(int id, int qid)
    {
        const string sql = "DELETE FROM Question WHERE Id = @QuestionId";
        _connection.ExecuteScalar(sql, new { QuestionId = qid });
        return NoContent();
    }

    // POST api/quizzes/5/questions/6/answers
    [HttpPost]
    [Route("{id}/questions/{qid}/answers")]
    public IActionResult PostAnswer(int id, int qid, [FromBody] AnswerCreateModel value)
    {
        const string sql = "INSERT INTO Answer (Text, QuestionId) VALUES(@Text, @QuestionId); SELECT LAST_INSERT_ROWID();";
        var answerId = _connection.ExecuteScalar(sql, new { Text = value.Text, QuestionId = qid });
        return Created($"/api/quizzes/{id}/questions/{qid}/answers/{answerId}", null);
    }

    // PUT api/quizzes/5/questions/6/answers/7
    [HttpPut("{id}/questions/{qid}/answers/{aid}")]
    public IActionResult PutAnswer(int id, int qid, int aid, [FromBody] AnswerUpdateModel value)
    {
        const string sql = "UPDATE Answer SET Text = @Text WHERE Id = @AnswerId";
        int rowsUpdated = _connection.Execute(sql, new { AnswerId = qid, Text = value.Text });
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5/questions/6/answers/7
    [HttpDelete]
    [Route("{id}/questions/{qid}/answers/{aid}")]
    public IActionResult DeleteAnswer(int id, int qid, int aid)
    {
        const string sql = "DELETE FROM Answer WHERE Id = @AnswerId";
        _connection.ExecuteScalar(sql, new { AnswerId = aid });
        return NoContent();
    }
}