using BilbolStack.Erc20Snapshot.Repository;
using Microsoft.AspNetCore.Mvc;

namespace erc20_snapshot.Controllers
{
    [ApiController]
    [Route("")]
    public class SnapshotController : ControllerBase
    {

        private IERC20Repository _erc20Repository;

        public SnapshotController(IERC20Repository erc20Repository)
        {
            _erc20Repository = erc20Repository;
        }

        [HttpGet("snapShot")]
        public IActionResult Get([FromQuery] long? untilBlock)
        {
            var stream = _erc20Repository.GenerateReport(untilBlock, out long blockNumber);
            return File(stream, "application/zip", $"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}_{blockNumber}.zip");
        }
    }
}
