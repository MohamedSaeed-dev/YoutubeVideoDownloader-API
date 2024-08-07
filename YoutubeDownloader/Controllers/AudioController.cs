using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YoutubeDownloaderCS.Services;

namespace YoutubeDownloaderCS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AudioController : ControllerBase
    {
        private readonly IAudioService _audio;
        public AudioController(IAudioService audio)
        {
            _audio = audio;
        }
        [HttpGet]
        public async Task<IActionResult> GetAudio([FromQuery] string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return BadRequest(new { message = "Url is missing" });
                var result = await _audio.GetAudio(url);
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong", Error = $"{ex.Message}", InnerError = $"{ex.InnerException?.Message}" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DownloadAudio([FromQuery] string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return BadRequest(new { message = "Url is missing" });
                var result = await _audio.DownloadAudio(url);
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong", Error = $"{ex.Message}", InnerError = $"{ex.InnerException?.Message}" });
            }
        }
    }
}
