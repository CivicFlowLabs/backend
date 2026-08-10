using CivicFlow.Shared.Api;

namespace CivicFlow.Tests.Api;

public class PageRequestTests
{
    [Fact]
    public void MacDinh_TraVeTrangDauVaKichThuocMacDinh()
    {
        var request = new PageRequest();

        Assert.Equal(1, request.Page);
        Assert.Equal(PageRequest.DefaultPageSize, request.PageSize);
        Assert.Equal(0, request.Skip);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void TrangKhongHopLe_BiKepVeMot(int page)
    {
        var request = new PageRequest { Page = page };

        Assert.Equal(1, request.Page);
    }

    [Fact]
    public void KichThuocTrangVuotTran_BiKepVeMaxPageSize()
    {
        var request = new PageRequest { PageSize = PageRequest.MaxPageSize + 500 };

        Assert.Equal(PageRequest.MaxPageSize, request.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void KichThuocTrangKhongHopLe_QuayVeMacDinh(int pageSize)
    {
        var request = new PageRequest { PageSize = pageSize };

        Assert.Equal(PageRequest.DefaultPageSize, request.PageSize);
    }

    [Fact]
    public void Skip_TinhTheoTrangVaKichThuoc()
    {
        var request = new PageRequest { Page = 3, PageSize = 20 };

        Assert.Equal(40, request.Skip);
    }
}

public class PaginationMetaTests
{
    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    [InlineData(100, 30, 4)]
    public void TotalPages_LamTronLen(long totalItems, int pageSize, int expected)
    {
        var meta = new PaginationMeta { Page = 1, PageSize = pageSize, TotalItems = totalItems };

        Assert.Equal(expected, meta.TotalPages);
    }

    [Fact]
    public void TrangDau_KhongCoTrangTruoc_NhungCoTrangSau()
    {
        var meta = new PaginationMeta { Page = 1, PageSize = 10, TotalItems = 25 };

        Assert.False(meta.HasPrevious);
        Assert.True(meta.HasNext);
    }

    [Fact]
    public void TrangCuoi_CoTrangTruoc_KhongCoTrangSau()
    {
        var meta = new PaginationMeta { Page = 3, PageSize = 10, TotalItems = 25 };

        Assert.True(meta.HasPrevious);
        Assert.False(meta.HasNext);
    }
}
