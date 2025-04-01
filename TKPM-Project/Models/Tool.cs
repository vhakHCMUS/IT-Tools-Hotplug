using TKPM_Project.Models.Tools;

public class Tool : ITool
{
    public int Id { get; set; }
    public string Name { get; set; } = "Any tool";
    public string Description { get; set; } = "No description";
    public bool IsPremium { get; set; } = false;
    public string Category { get; set; } = "General";
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public async Task<object> ExecuteAsync(params object[] inputs)
    {
        return await Task.FromResult(new { Message = "Execution completed" });
    }

    public string CustomViewTemplate { get; set; } = "defaultTemplate";
    public void Dispose() { }
}
