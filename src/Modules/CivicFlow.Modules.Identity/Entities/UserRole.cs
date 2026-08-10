namespace CivicFlow.Modules.Identity.Entities;

/// <summary>Vai trò người dùng. Quyết định phạm vi dữ liệu được truy cập.</summary>
public enum UserRole
{
    /// <summary>Người dân — chỉ thấy phản ánh của mình và dữ liệu công khai.</summary>
    Citizen,

    /// <summary>Cán bộ — thấy toàn bộ dữ liệu thuộc đơn vị hành chính của mình.</summary>
    Officer,

    /// <summary>Lãnh đạo — xem số liệu tổng hợp, không sửa dữ liệu nghiệp vụ.</summary>
    Leader,

    /// <summary>Quản trị — toàn hệ thống.</summary>
    Admin
}
