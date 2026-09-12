using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.Common;
using SchoolManagement.Application.DTOs.Audit;
using SchoolManagement.Application.Interfaces.Services;

namespace SchoolManagement.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] AuditLogFilterDto filter)
    {
        var (items, totalCount) = await _auditService.GetLogsAsync(filter);

        return Ok(new
        {
            totalCount,
            page = filter.Page,
            pageSize = filter.PageSize,
            items
        });
    }
}