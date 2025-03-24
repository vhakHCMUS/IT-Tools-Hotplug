namespace TKPM_Project.Models.Tools;

public interface ITool : IDisposable
{
    string Name { get; }
    string Description { get; }
    bool IsPremium { get; }
    string Category { get; }
    Task<object> ExecuteAsync(params object[] inputs);

    // Thêm thuộc tính để trả về thông tin giao diện
    string CustomViewTemplate { get; } // Chuỗi HTML hoặc tên view tùy chỉnh
}