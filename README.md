# Civic Flow — Backend

Nền tảng quản lý và cung cấp dịch vụ hành chính công ở cấp xã/phường.

Đây là ASP.NET Core Web API dùng chung cho hai ứng dụng khách:

- **Ứng dụng di động — Người dân**: gửi phản ánh kèm ảnh và toạ độ, theo dõi tiến độ xử lý, tra cứu văn bản, hỏi trợ lý ảo.
- **Ứng dụng máy tính — Cán bộ**: tiếp nhận và phân loại phản ánh, giao việc, xử lý, theo dõi SLA, xem dashboard và bản đồ điểm nóng.

Backend không phụ thuộc vào một loại client cụ thể, nên có thể phục vụ thêm client mới mà không phải sửa nghiệp vụ lõi.

## Quy trình nghiệp vụ

```
Người dân gửi phản ánh
        ↓
Tiếp nhận & phân loại
        ↓
Giao việc + SLA
        ↓
Xử lý
        ↓
Kiểm tra / nghiệm thu
        ↓
Thông báo cho người dân
        ↓
Người dân phản hồi
        ↓
Cập nhật dashboard
```

## Công nghệ

| Nhóm | Thành phần |
| --- | --- |
| Nền tảng | .NET 10, ASP.NET Core Web API, C# |
| Dữ liệu | Entity Framework Core 10, Npgsql, PostgreSQL 17 |
| Không gian | PostGIS 3.5, NetTopologySuite |
| Tìm kiếm ngữ nghĩa | pgvector 0.8 |
| Xác thực | JWT, refresh token, phân quyền theo vai trò và địa bàn |
| Khác | Hangfire, Serilog, FluentValidation, Swashbuckle, Semantic Kernel |

## Yêu cầu môi trường

- [.NET SDK 10](https://dotnet.microsoft.com/download) (dự án đang dùng `10.0.302`)
- Docker + Docker Compose
- EF Core CLI:

```bash
dotnet tool install --global dotnet-ef
```

## Chạy dự án

### 1. Tạo file cấu hình cục bộ

```bash
cp .env.example .env
```

Mở `.env` và đặt mật khẩu của riêng bạn — sinh nhanh bằng `openssl rand -base64 24`. Nhớ để `POSTGRES_PASSWORD` khớp với phần `Password=` trong chuỗi kết nối.

`.env` đã được `.gitignore` chặn nên không bao giờ bị commit. **Không đặt mật khẩu thật vào `.env.example`.**

### 2. Nạp biến môi trường vào shell

```bash
set -a && source .env && set +a
```

Chạy lệnh này trong mỗi phiên terminal mới, trước khi gọi `dotnet ef` hay `dotnet run`. Docker Compose tự đọc `.env` nên không cần bước này.

### 3. Khởi động hạ tầng

```bash
docker compose up -d
```

Lệnh này dựng PostgreSQL 17 (đã kèm PostGIS và pgvector) cùng Redis. Nếu thiếu `POSTGRES_PASSWORD`, Compose sẽ dừng ngay kèm thông báo thay vì lặng lẽ dựng CSDL bằng mật khẩu mặc định.

Nếu máy đang chạy PostgreSQL khác ở cổng 5432, đổi ánh xạ cổng trong `docker-compose.yml` thành `"5433:5432"` rồi sửa lại cổng trong chuỗi kết nối ở `.env`.

### 4. Áp dụng migration

Mỗi module có migration riêng, nên phải chỉ rõ project chứa migration và project khởi động:

```bash
dotnet ef database update --project src/Modules/CivicFlow.Modules.Identity --startup-project src/CivicFlow.Api
```

Xem danh sách migration của một module:

```bash
dotnet ef migrations list --project src/Modules/CivicFlow.Modules.Identity --startup-project src/CivicFlow.Api
```

### 5. Chạy API

```bash
dotnet run --project src/CivicFlow.Api
```

| Địa chỉ | Mô tả |
| --- | --- |
| `http://localhost:5141` | API |
| `http://localhost:5141/swagger` | Swagger UI (chỉ ở môi trường Development) |
| `http://localhost:5141/api/v1/health/db` | Kiểm tra kết nối CSDL và phiên bản PostGIS |

### 6. Chạy kiểm thử

```bash
dotnet test
```

Bộ test không cần cơ sở dữ liệu nên chạy được mà không phải nạp `.env`.

## Cấu hình

Toàn bộ thông tin nhạy cảm nằm trong `.env`, không nằm trong mã nguồn.

| Biến | Dùng ở đâu |
| --- | --- |
| `POSTGRES_PASSWORD` | `docker-compose.yml` khi dựng PostgreSQL |
| `ConnectionStrings__Default` | API và lệnh `dotnet ef` |

Dấu gạch dưới đôi là quy ước của .NET: `ConnectionStrings__Default` được ánh xạ sang khoá cấu hình `ConnectionStrings:Default`, nhờ đó API và EF Core tooling dùng chung đúng một biến.

Design-time factory **không có chuỗi kết nối mặc định**. Thiếu biến môi trường thì `dotnet ef` báo lỗi kèm hướng dẫn, thay vì âm thầm chạy bằng thông tin đăng nhập nhúng sẵn trong mã nguồn.

> **Không commit mật khẩu, khoá JWT hay thông tin đăng nhập dịch vụ ngoài.**
> Mã nguồn nằm trong kho công khai — mọi thứ đã commit đều tồn tại vĩnh viễn trong lịch sử Git, kể cả khi xoá đi ở commit sau.
> Môi trường triển khai dùng biến môi trường hoặc trình quản lý bí mật, không dùng file `.env`.

## Cấu trúc dự án

```
BE/
├── CivicFlow.sln
├── Directory.Build.props
├── docker-compose.yml
│
├── src/
│   ├── CivicFlow.Api/              # Điểm vào HTTP, đăng ký module
│   ├── CivicFlow.Shared/           # Kiểu dùng chung, hằng số, hợp đồng
│   ├── CivicFlow.Infrastructure/   # Cấu hình kỹ thuật dùng chung (Npgsql, PostGIS)
│   │
│   └── Modules/
│       ├── CivicFlow.Modules.Identity/     # Người dùng, đơn vị hành chính, phiên đăng nhập
│       ├── CivicFlow.Modules.Reports/      # Phản ánh của người dân
│       ├── CivicFlow.Modules.WorkOrders/   # Giao việc và SLA
│       ├── CivicFlow.Modules.Knowledge/    # Kho văn bản và RAG
│       └── CivicFlow.Modules.Engagement/   # Phản hồi sau xử lý
│
└── tests/
    └── CivicFlow.Tests/
```

## Kiến trúc

Dự án theo mô hình **Modular Monolith**, giữ khả năng tách thành microservice về sau.

Hướng phụ thuộc:

```
Api            →  Modules, Infrastructure, Shared
Module         →  Infrastructure, Shared
Infrastructure →  Shared
Module         ✕  Module
```

**Module không được tham chiếu lẫn nhau.** Cụ thể là không tham chiếu project, entity, `DbContext`, repository hay service nội bộ của module khác. Khi cần trao đổi giữa các module, dùng hợp đồng dùng chung hoặc domain event.

Mỗi module sở hữu dữ liệu của mình:

| Module | Schema |
| --- | --- |
| Identity | `identity` |
| Reports | `reports` |
| WorkOrders | `workorders` |
| Knowledge | `knowledge` |
| Engagement | `engagement` |

Mỗi module có `DbContext` riêng, bộ migration riêng và bảng lịch sử migration nằm trong schema của chính nó. Khoá ngoại vật lý chỉ được tạo trong cùng một schema; tham chiếu xuyên module dùng ID logic và kiểm tra ở tầng ứng dụng.

## Quy ước

### Cơ sở dữ liệu

- Tên bảng và cột dùng `snake_case`, không dùng tiền tố.
- Khoá chính kiểu `uuid`, mặc định `gen_random_uuid()`.
- Thời gian dùng `timestamptz` ở PostgreSQL và `DateTimeOffset` ở C#.
- Enum lưu dạng chuỗi `snake_case`, kèm ràng buộc `CHECK` khi phù hợp.
- Dữ liệu không gian dùng SRID `4326`: `geometry(Point, 4326)` cho vị trí phản ánh, `geometry(MultiPolygon, 4326)` cho ranh giới hành chính, kèm index GIST.

### Định dạng phản hồi

Mọi endpoint trả về cùng một phong bì, kể cả lỗi phát sinh trước khi vào controller (đường dẫn sai, sai phương thức HTTP).

Thành công:

```json
{ "data": { "database": "ok", "postGis": "3.5 USE_GEOS=1", "administrativeUnits": 3 } }
```

Danh sách có phân trang:

```json
{
  "data": [ { "id": "..." } ],
  "pagination": { "page": 1, "pageSize": 20, "totalItems": 137, "totalPages": 7, "hasPrevious": false, "hasNext": true }
}
```

Thất bại:

```json
{
  "data": null,
  "error": {
    "code": "validation_error",
    "message": "Dữ liệu gửi lên không hợp lệ.",
    "details": { "email": ["Email không hợp lệ."] },
    "correlationId": "9f2c1ab4e7d24f0c8b5a"
  }
}
```

Quy ước: `data` luôn xuất hiện, `error` chỉ có khi thất bại, `pagination` chỉ có ở endpoint danh sách. Client chỉ cần kiểm tra sự tồn tại của `error`.

Controller trả về DTO thuần hoặc `PagedResult<T>`; việc dựng phong bì do bộ lọc toàn cục lo. Mã lỗi ổn định nằm trong `ErrorCodes` — client nên bắt theo `code` thay vì so khớp `message`.

### Mã tương quan

Mọi phản hồi đều kèm header `X-Correlation-Id`. Client có thể tự gửi mã lên để nối log hai phía; mã không hợp lệ (quá 64 ký tự hoặc chứa ký tự ngoài chữ, số và dấu gạch ngang) sẽ bị thay bằng mã mới. Phản hồi lỗi lặp lại mã này trong `error.correlationId` để người dùng gửi cho bộ phận hỗ trợ.

### API

- REST, gắn phiên bản theo tiền tố `/api/v1` — khai báo qua hằng `ApiRoutes.Controller`.
- Không trả entity EF Core trực tiếp — luôn qua Request/Response DTO.
- Mọi endpoint danh sách đều phải phân trang.
- Dùng FluentValidation để kiểm tra dữ liệu đầu vào.
- Dùng bất đồng bộ xuyên suốt và luôn truyền tiếp `CancellationToken`. Không dùng `.Result` hay `.Wait()`.
- Phân quyền theo **vai trò + địa bàn**, lọc ngay ở tầng truy vấn, không dựa vào client.

### Nhánh

| Nhánh | Vai trò |
| --- | --- |
| `main` | Nhánh production |
| `dev` | Nhánh tích hợp |
| `feature/<tên>` | Nhánh tính năng, cắt ra từ `dev` |

Đặt tên nhánh bằng tiếng Anh: `feature/<tên>`, `fix/<tên>`, `refactor/<tên>`, `docs/<tên>`, `release/<phiên-bản>`.

### Commit

Theo [Conventional Commits](https://www.conventionalcommits.org/), viết bằng tiếng Anh:

```
feat(backend): scaffold modular monolith foundation
```

Các loại thường dùng: `feat`, `fix`, `refactor`, `docs`, `style`, `test`, `perf`, `chore`, `build`, `ci`.

## Nguyên tắc

- Module sở hữu dữ liệu của mình và không phụ thuộc lẫn nhau.
- PostgreSQL/PostGIS là nguồn sự thật cho dữ liệu không gian.
- Migration của EF Core là nguồn sự thật cho lược đồ ứng dụng; Docker chỉ dựng hạ tầng.
- AI đóng vai trò trợ lý, con người là người quyết định. RAG phải tôn trọng phạm vi truy cập và luôn dẫn nguồn.
- Không bao giờ trả dữ liệu nội bộ về cho client của người dân.
