using canvasync.Containers;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class PDFImagesController : ControllerBase
{
    private readonly StateContainer _stateContainer;
    public PDFImagesController(StateContainer stateContainer)
    {
        _stateContainer = stateContainer;
    }

    [HttpGet]
    public IActionResult Get() => Ok(_stateContainer.imageUrls);
}
