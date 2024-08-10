using AngleSharp.Dom;
using AngleSharp.Io;
using System;
using System.Diagnostics;
using YoutubeDownloaderCS.Helpers.Services;
using YoutubeDownloaderCS.Models;
using YoutubeDownloaderCS.Models.Video;
using YoutubeDownloaderCS.Services;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloaderCS.Repositories
{
    public class VideoRepository : IVideoService
    {
        private readonly YoutubeClient _client;
        private readonly IVideoHelper _videoHelper;
        private readonly IResponseStatus _response;

        public VideoRepository(YoutubeClient client, IResponseStatus response, IVideoHelper videoHelper)
        {
            _client = client;
            _response = response;
            _videoHelper = videoHelper;
        }
        public async Task<ResponseStatus> DownloadList(List<string> urls, string quality)
        {
            foreach (var url in urls)
            {
                var response = await _videoHelper.DownloadAsync(url, quality);
                if (response.StatusCode != 200) return _response.Custom(response.StatusCode, response.Message!);
            }
            return _response.Ok("Downloaded successfully");
        }
        public async Task<ResponseStatus> DownloadVideo(string url, string quality)
        {
            var response = await _videoHelper.DownloadAsync(url, quality);
            if (response.StatusCode != 200) return _response.Custom(response.StatusCode, response.Message!);
            return _response.Ok("Downloaded successfully");
        }

        public async Task<ResponseStatus> DownloadPlayList(string url, string quality)
        {
            var playlist = _client.Playlists.GetVideosAsync(url);
            if (playlist is null) return _response.BadRequest("Playlist is not found");
            await foreach (var i in playlist)
            {
                var response = await _videoHelper.DownloadAsync(i.Url, quality);
                if (response.StatusCode != 200) return _response.Custom(response.StatusCode, response.Message!);
            }
            return _response.Ok("Downloaded successfully");
        }
        public async Task<ResponseStatus> GetVideo(string url)
        {
            var video = await _client.Videos.GetAsync(url);
            if (video is null) return _response.NotFound("The Video does not exist");

            var thumbnail = video.Thumbnails.LastOrDefault(x => x.Resolution.Width > 700);
            var stream = await _client.Videos.Streams.GetManifestAsync(video.Id);

            VideoDTO videoDTO = new VideoDTO
            {
                URL = video.Url,
                Title = video.Title,
                UploadDate = video.UploadDate,
                Duration = video.Duration,
                Qualities = _videoHelper.GetQuality(stream),
                Thumbnail = thumbnail,
                Author = new Author
                {
                    ChannelTitle = video.Author.ChannelTitle,
                    ChannelURL = video.Author.ChannelUrl,
                }
            };

            return _response.Ok(videoDTO);
        }
        
    }
}
