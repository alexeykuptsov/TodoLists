using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoLists.App.Models;
using TodoLists.App.Templates;

namespace TodoLists.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TemplatesController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<TemplateDto>> GetTemplates() =>
        Ok(TemplateCatalog.All.Select(t => new TemplateDto(t.Id, t.Name)));
}
