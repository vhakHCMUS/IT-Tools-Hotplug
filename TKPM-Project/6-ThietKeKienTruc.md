# Thiết kế Kiến trúc Hệ thống

## 1. Kiến trúc hệ thống

### 1.1 Hình vẽ kiến trúc tổng thể

```
+-------------------+
|   User Interface  |
|  (Razor Views)    |
+-------------------+
          |
          v
+-------------------+
|   Controllers     |
| (Auth, Tool, etc.)|
+-------------------+
          |
          v
+-------------------+
|   Services        |
| (ToolService,     |
|  PremiumCleanup)  |
+-------------------+
          |
          v
+-------------------+
|   Data Access     |
| (Repositories,    |
|  ApplicationDb)   |
+-------------------+
          |
          v
+-------------------+
|   Database        |
|  (SQLite)         |
+-------------------+
```

### 1.2 Danh sách các module, thành phần trong hệ thống

| Thành phần                        | Diễn giải                                         |
| ----------------------------------- | --------------------------------------------------- |
| **User Interface**            | Giao diện người dùng, sử dụng Razor Views     |
| **AuthController**            | Xử lý xác thực người dùng                    |
| **ToolController**            | Quản lý các công cụ                            |
| **PremiumController**         | Quản lý tài khoản premium                       |
| **UserController**            | Quản lý thông tin người dùng                  |
| **ToolService**               | Quản lý vòng đời và thực thi plugin          |
| **PremiumCleanupService**     | Dọn dẹp tài khoản premium đã hết hạn        |
| **ApplicationDbContext**      | Quản lý kết nối và truy vấn cơ sở dữ liệu |
| **ToolRepository**            | Quản lý truy cập dữ liệu công cụ             |
| **UserPremium**               | Thông tin tài khoản premium                      |
| **Tool**                      | Thông tin và logic của công cụ                 |
| **CustomAssemblyLoadContext** | Quản lý tải và gỡ bỏ assembly plugin          |
| **ILogger**                   | Hệ thống logging cho giám sát và debug         |

## 2. Mô tả chi tiết từng thành phần trong hệ thống

### 2.1 User Interface

- **Razor Views**: Hiển thị giao diện người dùng, hỗ trợ responsive design và tích hợp với các controller để hiển thị dữ liệu.

### 2.2 Controllers

- **AuthController**: Xử lý các yêu cầu liên quan đến xác thực, bao gồm đăng nhập, đăng ký và đăng xuất.
- **ToolController**: Quản lý các thao tác liên quan đến công cụ, bao gồm tìm kiếm, thực thi và quản lý công cụ.
- **PremiumController**: Quản lý các yêu cầu và trạng thái của tài khoản premium.
- **UserController**: Quản lý thông tin người dùng và các thao tác liên quan đến người dùng.

### 2.3 Services

- **ToolService**: Chịu trách nhiệm quản lý vòng đời của plugin, bao gồm tải, thực thi và gỡ bỏ plugin. Xử lý các input/output của plugin.
- **PremiumCleanupService**: Quản lý việc dọn dẹp các tài khoản premium đã hết hạn và cập nhật vai trò người dùng.

### 2.4 Data Access

- **ApplicationDbContext**: Quản lý kết nối và truy vấn cơ sở dữ liệu, sử dụng Entity Framework Core để tương tác với SQLite.
- **ToolRepository**: Quản lý truy cập dữ liệu công cụ, sử dụng Repository Pattern để tách biệt logic truy vấn khỏi business logic.

### 2.5 Database

- **SQLite**: Cơ sở dữ liệu nhẹ, được sử dụng để lưu trữ thông tin người dùng, công cụ và trạng thái premium.

### 2.6 Plugin Management

- **CustomAssemblyLoadContext**: Quản lý việc tải và gỡ bỏ assembly plugin, cho phép cách ly các assembly của plugin và hỗ trợ việc unload khi không cần thiết.

### 2.7 Logging

- **ILogger**: Hệ thống logging được sử dụng để giám sát và debug, ghi lại các sự kiện quan trọng trong hệ thống.

## 3. Plugin Architecture

### 3.1 Cấu trúc Plugin

```csharp
public interface ITool
{
    string Name { get; }
    string Description { get; }
    bool IsPremium { get; }
    string Category { get; }
    Task<object> ExecuteAsync(params object[] inputs);
    string CustomViewTemplate { get; }
    void Dispose();
}
```

### 3.2 Cơ chế Load Plugin

1. **Assembly Loading**

   - Sử dụng CustomAssemblyLoadContext
   - Isolate plugin assemblies
   - Hỗ trợ unload assemblies
2. **Plugin Management**

   - Load từ thư mục Plugins
   - Cache trong memory
   - Quản lý lifecycle
3. **Dependency Resolution**

   - Sử dụng IServiceProvider
   - Scope-based injection
   - Quản lý resources

### 3.3 Quy trình Load Plugin

1. **Khởi tạo**

   ```csharp
   public ToolService(IServiceProvider serviceProvider, ILogger<ToolService> logger)
   {
       _serviceProvider = serviceProvider;
       _logger = logger;
       Directory.CreateDirectory(_pluginPath);
       LoadExistingTools();
   }
   ```
2. **Load Plugin**

   ```csharp
   public void LoadToolFromDll(string dllPath)
   {
       byte[] dllBytes = File.ReadAllBytes(dllPath);
       using (var stream = new MemoryStream(dllBytes))
       {
           var context = new CustomAssemblyLoadContext();
           var assembly = context.LoadFromStream(stream);
           // ... load và khởi tạo tool
       }
   }
   ```
3. **Execute Plugin**

   ```csharp
   public async Task<object> ExecuteToolAsync(string toolName, object inputs)
   {
       var tool = GetToolByName(toolName);
       var processedInputs = ProcessToolInputs(inputs);
       return await tool.ExecuteAsync(processedInputs);
   }
   ```

## 4. Design Patterns

### 4.1 Repository Pattern

- **Interface**

  ```csharp
  public interface IToolRepository
  {
      Task<IEnumerable<Tool>> GetAllAsync();
      Task<Tool> GetByIdAsync(int id);
      Task AddAsync(Tool tool);
      Task UpdateAsync(Tool tool);
      Task DeleteAsync(int id);
  }
  ```
- **Implementation**

  ```csharp
  public class ToolRepository : IToolRepository
  {
      private readonly ApplicationDbContext _context;
      public ToolRepository(ApplicationDbContext context)
      {
          _context = context;
      }
      // ... implementations
  }
  ```

### 4.2 Factory Pattern

- **Tool Creation**
  ```csharp
  var tool = Activator.CreateInstance(toolType) as ITool;
  ```

### 4.3 Strategy Pattern

- **Input Processing**
  ```csharp
  private Dictionary<string, object> ProcessToolInputs(object inputs)
  {
      // Different strategies for different input types
      if (inputs is IFormCollection) { ... }
      if (inputs is IDictionary<string, object>) { ... }
      if (inputs is string) { ... }
  }
  ```

### 4.4 Observer Pattern

- **Logging System**
  ```csharp
  private readonly ILogger<ToolService> _logger;
  _logger.LogInformation($"Loaded tool {tool.Name} from {dllPath}");
  ```

## 5. Service Architecture

### 5.1 ToolService

- **Responsibilities**

  - Load/unload plugins
  - Execute tools
  - Manage tool lifecycle
  - Process inputs/outputs
- **Key Methods**

  ```csharp
  public void LoadToolFromDll(string dllPath)
  public async Task<object> ExecuteToolAsync(string toolName, object inputs)
  public async void UnloadTool(string toolName)
  ```

### 5.2 PremiumCleanupService

- **Responsibilities**
  - Clean expired premiums
  - Update user roles
  - Maintain premium status

## 6. Security Architecture

### 6.1 Authentication

- Identity Framework
- Cookie-based authentication
- JWT support

### 6.2 Authorization

- Role-based access control
- Policy-based authorization
- Custom authorization handlers

## 7. Data Flow

### 7.1 Tool Execution Flow

1. User request → Controller
2. Controller → ToolService
3. ToolService → Plugin
4. Plugin → Result
5. Result → View

### 7.2 Premium Management Flow

1. User request → PremiumController
2. PremiumController → PremiumService
3. PremiumService → Database
4. Database → User update

## 8. Performance Considerations

### 8.1 Plugin Loading

- Lazy loading
- Assembly isolation
- Memory management

### 8.2 Database Access

- Repository pattern
- Async operations
- Connection pooling

### 8.3 Caching

- In-memory caching
- Tool caching
- User session caching

## 9. Error Handling

### 9.1 Exception Handling

- Global exception handler
- Custom exceptions
- Logging system

### 9.2 Validation

- Input validation
- Business rule validation
- State validation
