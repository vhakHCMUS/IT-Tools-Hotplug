# Phân tích Hệ thống

## 1. Kiến trúc Hệ thống

### 1.1 Mô hình 3 lớp
- **Presentation Layer (Controllers)**
  - Xử lý các request HTTP
  - Tương tác với người dùng
  - Chuyển dữ liệu giữa View và Service

- **Business Logic Layer (Services)**
  - Xử lý logic nghiệp vụ
  - Quản lý dữ liệu
  - Tích hợp các tính năng

- **Data Access Layer (Models/Repositories)**
  - Tương tác với cơ sở dữ liệu
  - Quản lý entity
  - Xử lý truy vấn dữ liệu

### 1.2 Các thành phần chính
1. **Controllers**
   - AuthController: Xác thực người dùng
   - ToolController: Quản lý công cụ
   - PremiumController: Quản lý tài khoản premium
   - UserController: Quản lý người dùng
   - RoleController: Quản lý phân quyền

2. **Services**
   - ToolService: Xử lý logic công cụ
   - PremiumCleanupService: Dọn dẹp tài khoản premium

3. **Models**
   - ApplicationUser: Thông tin người dùng
   - Tool: Thông tin công cụ
   - UserPremium: Thông tin tài khoản premium
   - UserLikedTool: Quan hệ người dùng và công cụ yêu thích

## 2. Cơ sở dữ liệu

### 2.1 Entity Framework Core
- Sử dụng SQLite làm cơ sở dữ liệu
- Code-First approach
- Migration tự động

### 2.2 Các bảng chính
1. **AspNetUsers**
   - Thông tin người dùng
   - Tích hợp Identity Framework

2. **Tools**
   - Id: Khóa chính
   - Name: Tên công cụ
   - Description: Mô tả
   - IsPremium: Cờ premium
   - Category: Danh mục
   - IsAvailable: Trạng thái
   - CreatedAt: Ngày tạo

3. **UserLikedTools**
   - UserId: Khóa ngoại đến Users
   - ToolId: Khóa ngoại đến Tools
   - Quan hệ many-to-many

4. **UserPremiums**
   - Id: Khóa chính
   - UserId: Khóa ngoại đến Users
   - StartDate: Ngày bắt đầu
   - ExpireDate: Ngày hết hạn

### 2.3 Quan hệ dữ liệu
- User - Tool: Many-to-Many (qua UserLikedTool)
- User - Premium: One-to-Many
- Role - User: Many-to-Many (qua Identity)

## 3. Phân quyền và Bảo mật

### 3.1 Các vai trò
1. **Anonymous**
   - Xem công cụ miễn phí
   - Đăng ký tài khoản

2. **User**
   - Đăng nhập
   - Sử dụng công cụ miễn phí
   - Yêu cầu premium

3. **Premium**
   - Sử dụng công cụ premium
   - Xem trạng thái premium
   - Gia hạn premium

4. **Admin**
   - Quản lý người dùng
   - Quản lý công cụ
   - Quản lý premium

### 3.2 Bảo mật
- Xác thực qua Identity Framework
- Phân quyền dựa trên Role
- Bảo vệ API endpoints
- Kiểm tra quyền truy cập công cụ

## 4. Tính năng Công cụ

### 4.1 Cấu trúc Công cụ
- Interface ITool
- Class Tool cơ sở
- CustomViewTemplate cho giao diện
- ExecuteAsync cho xử lý

### 4.2 Quản lý Công cụ
- Import công cụ từ DLL
- Bật/tắt công cụ
- Đánh dấu premium
- Tìm kiếm và lọc

### 4.3 Tương tác Công cụ
- Like/Unlike
- Danh sách yêu thích
- Thực thi công cụ
- Hiển thị kết quả

## 5. Hệ thống Premium

### 5.1 Quản lý Premium
- Yêu cầu premium
- Kiểm tra trạng thái
- Gia hạn tự động
- Dọn dẹp tài khoản hết hạn

### 5.2 Tính năng Premium
- Truy cập công cụ premium
- Thời hạn 7 ngày
- Tự động gia hạn
- Quản lý trạng thái

## 6. Điểm mạnh và Hạn chế

### 6.1 Điểm mạnh
- Kiến trúc rõ ràng, phân tách tốt
- Bảo mật mạnh mẽ với Identity
- Hệ thống plugin linh hoạt
- Quản lý premium hiệu quả

### 6.2 Hạn chế
- Phụ thuộc vào SQLite
- Giới hạn thời gian premium
- Thiếu hệ thống thanh toán
- Chưa có API documentation

## 7. Hướng phát triển

### 7.1 Ngắn hạn
- Thêm hệ thống thanh toán
- Cải thiện giao diện người dùng
- Tối ưu hiệu suất
- Thêm API documentation

### 7.2 Dài hạn
- Hỗ trợ nhiều loại cơ sở dữ liệu
- Mở rộng hệ thống plugin
- Thêm tính năng phân tích
- Phát triển mobile app 