# Bảng ghi nhận thay đổi tài liệu

| Ngày | Phiên bản | Mô tả | Người thay đổi |
|------|-----------|-------|----------------|
| 16/04/2024 | 1.0.0 | Khởi tạo dự án ASP.NET Core với cấu trúc MVC | [Tên người thực hiện] |
| 16/04/2024 | 1.0.1 | Thiết lập Entity Framework Core với SQLite | [Tên người thực hiện] |
| 16/04/2024 | 1.0.2 | Tích hợp Identity Framework cho xác thực | [Tên người thực hiện] |
| 16/04/2024 | 1.0.3 | Triển khai các tính năng cốt lõi của hệ thống | [Tên người thực hiện] |

# Phát biểu bài toán

## 1. Khảo sát hiện trạng

### 1.1 Nhu cầu thực tế
- Cần xây dựng một hệ thống quản lý công cụ (tools) hiệu quả
- Yêu cầu bảo mật thông tin và phân quyền người dùng
- Cần tích hợp với cơ sở dữ liệu SQLite
- Yêu cầu giao diện web thân thiện với người dùng
- Cần hệ thống quản lý tài khoản premium

### 1.2 Hiện trạng đơn vị
- Đơn vị đang sử dụng hệ thống quản lý thủ công
- Cần chuyển đổi sang hệ thống tự động hóa
- Yêu cầu tích hợp với các hệ thống hiện có
- Cần bảo mật thông tin người dùng
- Cần quản lý hiệu quả các công cụ và tương tác người dùng

### 1.3 Hệ thống hiện có
- Chưa có hệ thống quản lý tự động
- Các thao tác đang được thực hiện thủ công
- Khó khăn trong việc theo dõi và quản lý dữ liệu
- Thiếu tính bảo mật và phân quyền
- Chưa có hệ thống quản lý tài khoản premium

## 2. Yêu cầu hệ thống

### 2.1 Yêu cầu chức năng
1. **Quản lý Người dùng**
   - Đăng ký và đăng nhập
   - Quản lý thông tin cá nhân
   - Phân quyền người dùng
   - Quản lý tài khoản premium

2. **Quản lý Công cụ**
   - Thêm, sửa, xóa công cụ
   - Tìm kiếm và lọc công cụ
   - Like/unlike công cụ
   - Quản lý danh sách công cụ yêu thích

3. **Hệ thống Premium**
   - Đăng ký và quản lý tài khoản premium
   - Các tính năng đặc biệt cho người dùng premium
   - Tự động gia hạn và dọn dẹp tài khoản premium

4. **Giao diện và Trải nghiệm**
   - Giao diện web responsive
   - Thông báo và xử lý lỗi
   - Tương tác người dùng thân thiện

### 2.2 Yêu cầu phi chức năng
- Bảo mật thông tin người dùng
- Hiệu suất hệ thống cao
- Dễ dàng mở rộng và bảo trì
- Tương thích với các trình duyệt web phổ biến
- Khả năng mở rộng cho các tính năng mới

### 2.3 Yêu cầu kỹ thuật
- Sử dụng ASP.NET Core 8.0
- SQLite làm cơ sở dữ liệu
- Entity Framework Core làm ORM
- Identity Framework cho xác thực
- Dịch vụ background cho các tác vụ định kỳ
- Hệ thống phân quyền linh hoạt 