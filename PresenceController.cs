using AttendenceProgram.Models;
using AttendenceProgram.Services;
using Microsoft.AspNetCore.Mvc;

namespace AttendenceProgram.Controllers
{
    [ApiController]
    [Route("presence")]
    public class PresenceController : ControllerBase
    {
        private readonly PresenceStore _store;
        public PresenceController(PresenceStore store) => _store = store;

        // GET /presence
        [HttpGet]
        public IActionResult GetAll() => Ok(_store.GetAll());

        // POST /presence/set
        [HttpPost("set")]
        public IActionResult Set([FromBody] PresenceSetRequest req)
        {
            if (_store.Set(req, out var error)) return Ok(new { ok = true });
            return BadRequest(new { ok = false, error });
        }
    }
}