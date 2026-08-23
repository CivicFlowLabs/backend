namespace CivicFlow.Shared.Auth;

/// <summary>
/// Các hằng số về Vai trò (Roles) và Chính sách xác thực (Policies) toàn hệ thống.
/// </summary>
public static class AuthConstants
{
    public static class Roles
    {
        public const string Citizen = "citizen";
        public const string Officer = "officer";
        public const string Leader = "leader";
        public const string Admin = "admin";
    }

    public static class Policies
    {
        public const string RequireAdmin = "RequireAdmin";
        public const string RequireLeader = "RequireLeader";
        public const string RequireOfficer = "RequireOfficer";
        public const string RequireCitizen = "RequireCitizen";
    }

    public static class Claims
    {
        public const string AdministrativeUnitId = "administrativeUnitId";
    }
}
