using YoutubeDownloaderCS.Models;
using YoutubeDownloaderCS.Models.Audio;
using YoutubeDownloaderCS.Services;
using YoutubeExplode;

namespace YoutubeDownloaderCS.Repositories
{
    public class AudioRepository : IAudioService
    {
        private readonly YoutubeClient _client;
        private readonly IResponseStatus _response;

        public AudioRepository(YoutubeClient client, IResponseStatus response)
        {
            _client = client;
            _response = response;
        }
        public async Task<ResponseStatus> DownloadAudio(string url)
        {
            var audio = await _client.Videos.GetAsync(url);
            if (audio is null) return _response.BadRequest("The Video is not found");
            var stream = await _client.Videos.Streams.GetManifestAsync(url);
            var audioStream = stream.GetAudioOnlyStreams()
                .OrderByDescending(x => x.Bitrate)
                .FirstOrDefault();
            var dateNow = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss");
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var outputFilePath = @$"{downloadsPath}\{audio.Title}_{dateNow}.mp3";
            await _client.Videos.Streams.DownloadAsync(audioStream!, outputFilePath);
            return _response.Ok("Downloaded Successfully");
        }

        public async Task<ResponseStatus> DownloadList(List<string> urls)
        {
            foreach (var url in urls)
            {
                var response = await DownloadAudio(url);
                if(response.StatusCode != 200) return _response.Custom(response.StatusCode, response.Message!);
            }
            return _response.Ok("Downloaded Successfully");
        }

        public async Task<ResponseStatus> DownloadPlayList(string url)
        {
            var playlist = _client.Playlists.GetVideosAsync(url);
            if (playlist is null) return _response.BadRequest("Playlist is not found");
            await foreach (var i in playlist)
            {
                var response = await DownloadAudio(i.Url);
                if (response.StatusCode != 200) return _response.Custom(response.StatusCode, response.Message!);
            }
            return _response.Ok("Downloaded successfully");
        }

        public async Task<ResponseStatus> GetAudio(string url)
        {
            var audio = await _client.Videos.Streams.GetManifestAsync(url);
            if (audio is null) return _response.BadRequest("The Video is not found");
            var stream = audio.GetAudioOnlyStreams().Select(x => new AudioDTO
            {
                Type = x.Container.Name,
                Size = x.Size.MegaBytes,
                Bitrate=  x.Bitrate.MegaBitsPerSecond
            });
            return _response.Ok(stream);
        }
    }
}
