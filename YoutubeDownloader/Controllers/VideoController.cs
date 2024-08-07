using AngleSharp.Dom;
using Microsoft.AspNetCore.Mvc;
using YoutubeDownloaderCS.Services;
using YoutubeExplode;
using YoutubeExplode.Playlists;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace YoutubeDownloaderCS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly IVideoService _video;
        private readonly IAudioService _audio;
        public VideoController(IVideoService video, IAudioService audio)
        {
            _video = video;
            _audio = audio;
        }
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? url)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return BadRequest(new { message = "Url is missing" });
                var result = await _video.GetVideo(url);
                return StatusCode(result.StatusCode, new { message = result.Message});
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong", Error = $"{ex.Message}", InnerError = $"{ex.InnerException?.Message}" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Download([FromQuery] string url, [FromQuery] string quality = "360p")
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return BadRequest(new { message = "Url is missing" });
                var result = await _video.DownloadVideo(url, quality);
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong", Error = $"{ex.Message}", InnerError = $"{ex.InnerException?.Message}" });
            }
        }
        [HttpPost("Playlist")]
        public async Task<IActionResult> DownloadPlayList([FromQuery] string url, [FromQuery] string quality = "360p")
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return BadRequest(new { message = "Url is missing" });
                var result = await _video.DownloadPlayList(url, quality);
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong", Error = $"{ex.Message}", InnerError = $"{ex.InnerException?.Message}" });
            }
        }
        [HttpPost("List")]
        public async Task<IActionResult> DownloadList([FromBody] List<string> urls, [FromQuery] string quality = "360p")
        {
            try
            {
                if (urls.Count < 1) return BadRequest(new { message = "Url is missing" });
                var result = await _video.DownloadList(urls, quality);
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong", Error = $"{ex.Message}", InnerError = $"{ex.InnerException?.Message}" });
            }
        }
    }
}
