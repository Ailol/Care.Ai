using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Care.AI.Workflows;
using System.Globalization;

namespace Care.AI.Api.Controllers;

[ApiController]
[Route("api/workflow")]
public class WorkflowController : ControllerBase
{
    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromBody] WorkflowExecuteRequest request)
    {
        var userId = User.FindFirst("UserId")?.Value ?? "anonymous";
        
        var context = request.Context ?? new Dictionary<string, object>();
        context["userId"] = userId;
        
        var result = await Workflow.ExecuteAsync(request.Workflow, context);
        
        return Ok(result);
    }
    
    [HttpPost("execute-yaml")]
    public async Task<IActionResult> ExecuteYaml([FromBody] YamlExecuteRequest request)
    {
        var userId = User.FindFirst("UserId")?.Value ?? "anonymous";
        
        var context = request.Context ?? new Dictionary<string, object>();
        context["userId"] = userId;
        
        var result = await Workflow.ExecuteYamlAsync(request.Yaml, context);
        
        return Ok(result);
    }
    
    [HttpPost("execute-file")]
    public async Task<IActionResult> ExecuteFile([FromBody] FileExecuteRequest request)
    {
        var userId = User.FindFirst("UserId")?.Value ?? "anonymous";
        
        var context = request.Context ?? new Dictionary<string, object>();
        context["userId"] = userId;
        
        var result = await Workflow.ExecuteFileAsync(request.FilePath, context);
        
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var workflow = await Workflow.GetAsync(id);
        if (workflow == null)
        {
            return NotFound();
        }
        
        return Ok(workflow);
    }
    
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] WorkflowDefinition workflow)
    {
        var id = await Workflow.SaveAsync(workflow);
        return Ok(new { id });
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await Workflow.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }
        
        return NoContent();
    }
    
    [HttpPost("action/create")]
    public async Task<IActionResult> CreateWorkflowAction([FromBody] WorkflowActionRequest? request)
    {
        return await CreateWorkflow(request);
    }
    
    [HttpPost("action/schedule")]
    public async Task<IActionResult> ScheduleWorkflowAction([FromBody] WorkflowActionRequest? request)
    {
        return await ScheduleWorkflow(request);
    }
    
    private async Task<IActionResult> CreateWorkflow(WorkflowActionRequest? request)
    {
        var workflow = new WorkflowDefinition
        {
            Name = request?.Name ?? "New Workflow",
            Description = request?.Description ?? "Created via API",
            Steps = request?.Steps ?? new List<WorkflowStep>()
        };
        
        var id = await Workflow.SaveAsync(workflow);
        
        return Ok(new { 
            message = "Workflow created successfully",
            id,
            workflow 
        });
    }
    
    private async Task<IActionResult> ScheduleWorkflow(WorkflowActionRequest? request)
    {
        // In real implementation, this would schedule the workflow
        await Task.CompletedTask;
        
        return Ok(new
        {
            message = "Workflow scheduled",
            schedule = request?.Schedule ?? "daily",
            workflowId = request?.WorkflowId
        });
    }
}

public class WorkflowExecuteRequest
{
    public WorkflowDefinition Workflow { get; set; } = new();
    public Dictionary<string, object>? Context { get; set; }
}

public class YamlExecuteRequest
{
    public string Yaml { get; set; } = "";
    public Dictionary<string, object>? Context { get; set; }
}

public class FileExecuteRequest
{
    public string FilePath { get; set; } = "";
    public Dictionary<string, object>? Context { get; set; }
}

public class WorkflowActionRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<WorkflowStep>? Steps { get; set; }
    public string? Schedule { get; set; }
    public string? WorkflowId { get; set; }
}
