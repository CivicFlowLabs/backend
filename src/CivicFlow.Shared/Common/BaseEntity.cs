namespace CivicFlow.Shared.Common;

/// <summary>
/// Lớp cơ sở cho mọi entity trong hệ thống. Khoá chính dùng uuid do
/// PostgreSQL sinh (gen_random_uuid) để tránh lộ số thứ tự bản ghi.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
}
