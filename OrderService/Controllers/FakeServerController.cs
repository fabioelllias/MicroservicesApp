using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FakeServerController : ControllerBase
{
    private static int _callCount = 0;

    [HttpGet("fail")]
    public IActionResult Fail()
    {
        _callCount++;

        if (_callCount % 4 != 0)
            return StatusCode(500, $"🔴 Erro simulado na tentativa {_callCount}");

        return Ok($"✅ Sucesso simulado na tentativa {_callCount}");
    }
}
