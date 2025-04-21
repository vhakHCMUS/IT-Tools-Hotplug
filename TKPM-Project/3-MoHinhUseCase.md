# Mô hình Use Case

## 1. Tác nhân (Actors)

1. **Người dùng thường (User)**

   - Đăng ký tài khoản
   - Đăng nhập hệ thống
   - Xem danh sách công cụ
   - Tìm kiếm công cụ
   - Sử dụng công cụ miễn phí
   - Yêu cầu tài khoản Premium
2. **Người dùng Premium**

   - Có tất cả quyền của User
   - Sử dụng công cụ Premium
   - Xem trạng thái Premium
   - Gia hạn Premium
3. **Quản trị viên (Admin)**

   - Quản lý người dùng
   - Quản lý công cụ
   - Quản lý tài khoản Premium
   - Xem báo cáo hệ thống

## 2. Use Case

### 2.1 Quản lý Người dùng

1. **Đăng ký tài khoản**

   - Tác nhân: User
   - Mô tả: Người dùng đăng ký tài khoản mới
   - Luồng sự kiện:
     1. Người dùng nhập thông tin đăng ký
     2. Hệ thống kiểm tra thông tin
     3. Tạo tài khoản mới
     4. Gán quyền User
2. **Đăng nhập**

   - Tác nhân: User, Premium User
   - Mô tả: Người dùng đăng nhập vào hệ thống
   - Luồng sự kiện:
     1. Người dùng nhập thông tin đăng nhập
     2. Hệ thống xác thực
     3. Tạo phiên đăng nhập
3. **Đăng xuất**

   - Tác nhân: User, Premium User
   - Mô tả: Người dùng đăng xuất khỏi hệ thống
   - Luồng sự kiện:
     1. Người dùng yêu cầu đăng xuất
     2. Hệ thống hủy phiên đăng nhập

### 2.2 Quản lý Công cụ

1. **Xem danh sách công cụ**

   - Tác nhân: User, Premium User
   - Mô tả: Xem danh sách các công cụ có sẵn
   - Luồng sự kiện:
     1. Người dùng truy cập trang công cụ
     2. Hệ thống hiển thị danh sách công cụ
     3. Người dùng có thể lọc theo danh mục
2. **Tìm kiếm công cụ**

   - Tác nhân: User, Premium User
   - Mô tả: Tìm kiếm công cụ theo tên hoặc danh mục
   - Luồng sự kiện:
     1. Người dùng nhập từ khóa tìm kiếm
     2. Hệ thống tìm kiếm và hiển thị kết quả
     3. Người dùng có thể sắp xếp kết quả
3. **Sử dụng công cụ**

   - Tác nhân: User, Premium User
   - Mô tả: Sử dụng một công cụ cụ thể
   - Luồng sự kiện:
     1. Người dùng chọn công cụ
     2. Hệ thống kiểm tra quyền truy cập
     3. Hiển thị giao diện công cụ
     4. Người dùng nhập dữ liệu
     5. Hệ thống xử lý và hiển thị kết quả
4. **Quản lý công cụ (Admin)**

   - Tác nhân: Admin
   - Mô tả: Quản lý danh sách công cụ
   - Luồng sự kiện:
     1. Import công cụ mới
     2. Xóa công cụ
     3. Đánh dấu công cụ Premium
     4. Bật/tắt công cụ

### 2.3 Quản lý Premium

1. **Yêu cầu Premium**

   - Tác nhân: User
   - Mô tả: Yêu cầu nâng cấp tài khoản Premium
   - Luồng sự kiện:
     1. Người dùng yêu cầu Premium
     2. Hệ thống kiểm tra điều kiện
     3. Cấp quyền Premium trong 7 ngày
     4. Thông báo kết quả
2. **Kiểm tra trạng thái Premium**

   - Tác nhân: Premium User
   - Mô tả: Kiểm tra thời hạn Premium
   - Luồng sự kiện:
     1. Người dùng yêu cầu kiểm tra
     2. Hệ thống hiển thị thông tin Premium
3. **Quản lý Premium (Admin)**

   - Tác nhân: Admin
   - Mô tả: Quản lý tài khoản Premium
   - Luồng sự kiện:
     1. Xem danh sách Premium
     2. Quản lý thời hạn Premium
     3. Xử lý yêu cầu Premium

### 2.4 Quản lý Yêu thích

1. **Thêm vào danh sách yêu thích**

   - Tác nhân: User, Premium User
   - Mô tả: Đánh dấu công cụ yêu thích
   - Luồng sự kiện:
     1. Người dùng chọn công cụ
     2. Thêm vào danh sách yêu thích
2. **Xem danh sách yêu thích**

   - Tác nhân: User, Premium User
   - Mô tả: Xem các công cụ đã yêu thích
   - Luồng sự kiện:
     1. Người dùng truy cập danh sách yêu thích
     2. Hệ thống hiển thị danh sách

## 3. Biểu đồ Use Case

```
[User] ---> (Đăng ký)
[User] ---> (Đăng nhập)
[User] ---> (Đăng xuất)
[User] ---> (Xem danh sách công cụ)
[User] ---> (Tìm kiếm công cụ)
[User] ---> (Sử dụng công cụ miễn phí)
[User] ---> (Yêu cầu Premium)

[Premium User] ---> (Sử dụng công cụ Premium)
[Premium User] ---> (Xem trạng thái Premium)
[Premium User] ---> (Gia hạn Premium)

[Admin] ---> (Quản lý người dùng)
[Admin] ---> (Quản lý công cụ)
[Admin] ---> (Quản lý Premium)
[Admin] ---> (Xem báo cáo)
```
