using CivicFlow.Api.Filters;
using CivicFlow.Shared.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace CivicFlow.Tests.Api;

public class ApiResponseWrappingFilterTests
{
    private sealed record SampleDto(string Name);

    [Fact]
    public async Task GiaTriThuong_DuocDuaVaoData()
    {
        var result = await RunFilterAsync(new OkObjectResult(new SampleDto("Phường Bến Nghé")));

        var response = Assert.IsType<ApiResponse<object?>>(Assert.IsAssignableFrom<ObjectResult>(result).Value);
        Assert.Equal(new SampleDto("Phường Bến Nghé"), response.Data);
        Assert.Null(response.Error);
        Assert.Null(response.Pagination);
    }

    [Fact]
    public async Task PagedResult_TachThanhDataVaPagination()
    {
        var paged = new PagedResult<SampleDto>
        {
            Items = [new SampleDto("A"), new SampleDto("B")],
            Pagination = new PaginationMeta { Page = 2, PageSize = 2, TotalItems = 5 }
        };

        var result = await RunFilterAsync(new OkObjectResult(paged));

        var response = Assert.IsType<ApiResponse<object?>>(Assert.IsAssignableFrom<ObjectResult>(result).Value);
        Assert.Equal(paged.Items, response.Data);
        Assert.NotNull(response.Pagination);
        Assert.Equal(2, response.Pagination.Page);
        Assert.Equal(5, response.Pagination.TotalItems);
        Assert.Equal(3, response.Pagination.TotalPages);
        Assert.Null(response.Error);
    }

    [Fact]
    public async Task GiaTriDaLaPhongBi_KhongBocHaiLan()
    {
        var already = ApiResponse.Success(new SampleDto("Đã bọc sẵn"));

        var result = await RunFilterAsync(new OkObjectResult(already));

        Assert.Same(already, Assert.IsAssignableFrom<ObjectResult>(result).Value);
    }

    [Fact]
    public async Task KetQuaKhongCoThanVoiMaLoi_DuocDoiThanhPhongBiLoi()
    {
        var result = await RunFilterAsync(new NotFoundResult(), correlationId: "trace-1");

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

        var response = Assert.IsType<ApiResponse<object>>(objectResult.Value);
        Assert.NotNull(response.Error);
        Assert.Equal(ErrorCodes.NotFound, response.Error.Code);
        Assert.Equal("trace-1", response.Error.CorrelationId);
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task ProblemDetails_DuocDoiSangPhongBiLoi()
    {
        var problem = new ProblemDetails { Detail = "Mã đơn vị đã tồn tại." };
        var result = await RunFilterAsync(new ObjectResult(problem) { StatusCode = StatusCodes.Status409Conflict });

        var response = Assert.IsType<ApiResponse<object>>(Assert.IsType<ObjectResult>(result).Value);
        Assert.NotNull(response.Error);
        Assert.Equal(ErrorCodes.Conflict, response.Error.Code);
        Assert.Equal("Mã đơn vị đã tồn tại.", response.Error.Message);
    }

    [Fact]
    public async Task PhanHoi204_GiuNguyen_ViChuanHttpKhongChoPhepCoThan()
    {
        var noContent = new NoContentResult();

        var result = await RunFilterAsync(noContent);

        Assert.Same(noContent, result);
    }

    private static async Task<IActionResult> RunFilterAsync(IActionResult result, string? correlationId = null)
    {
        var httpContext = new DefaultHttpContext();
        if (correlationId is not null)
        {
            httpContext.Items[HttpConstants.CorrelationIdItemKey] = correlationId;
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var filters = new List<IFilterMetadata>();
        var controller = new object();
        var context = new ResultExecutingContext(actionContext, filters, result, controller);

        await new ApiResponseWrappingFilter().OnResultExecutionAsync(
            context,
            () => Task.FromResult(new ResultExecutedContext(actionContext, filters, context.Result, controller)));

        return context.Result;
    }
}
