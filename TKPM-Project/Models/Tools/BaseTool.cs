namespace TKPM_Project.Models.Tools;

public abstract class BaseTool : ITool
{
    public virtual string Name => this.GetType().Name;
    public abstract string Description { get; }
    public virtual bool IsPremium => false;
    public abstract string Category { get; }
    public abstract Task<object> ExecuteAsync(params object[] inputs);
    public virtual string CustomViewTemplate => null; // Mặc định không có view tùy chỉnh
    public virtual void Dispose() { }
}