using AngleSharp.Dom;
using AngleSharp.Io;
using System;
using System.Diagnostics;
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
        private readonly IResponseStatus _response;

        public VideoRepository(YoutubeClient client, IResponseStatus response)
        {
            _client = client;
            _response = response;
        }

        private async Task<(VideoOnlyStreamInfo?, AudioOnlyStreamInfo?, Video?)> GetVideoAndAudio(string url, string quality)
        {
            var video = await _client.Videos.GetAsync(url);
            var streamManifest = await _client.Videos.Streams.GetManifestAsync(url);

            var streamInfoVideo = streamManifest.GetVideoOnlyStreams()
                .Where(x => x.VideoQuality.Label.Contains(quality))
                .OrderByDescending(x => x.Bitrate)
                .FirstOrDefault();

            var streamInfoAudio = streamManifest.GetAudioOnlyStreams()
                .OrderByDescending(x => x.Bitrate)
                .FirstOrDefault();
            return (streamInfoVideo, streamInfoAudio, video);

        }

        private async Task<(string, string)> DownloadVideoAudio(VideoOnlyStreamInfo streamInfoVideo, AudioOnlyStreamInfo streamInfoAudio)
        {
            var audioFilePath = $"audio.{streamInfoAudio.Container.Name}";
            var videoFilePath = $"video.{streamInfoVideo.Container.Name}";

            await _client.Videos.Streams.DownloadAsync(streamInfoVideo, videoFilePath);
            await _client.Videos.Streams.DownloadAsync(streamInfoAudio, audioFilePath);

            return (audioFilePath, videoFilePath);

        }

        private async Task<ResponseStatus> DownloadAsync(string url, string quality)
        {
            var (streamInfoVideo, streamInfoAudio, video) = await GetVideoAndAudio(url, quality);
            if (streamInfoVideo is null || streamInfoAudio is null || video is null) return _response.NotFound("Could not find video or audio streams in the desired quality.");
            var (audioFilePath, videoFilePath) = await DownloadVideoAudio(streamInfoVideo, streamInfoAudio);
            await MergeAudioVideo(streamInfoVideo, video, videoFilePath, audioFilePath);
            File.Delete(videoFilePath);
            File.Delete(audioFilePath);
            return _response.Ok("Success");
        }



        public async Task<ResponseStatus> DownloadVideo(string url, string quality)
        {
            var response = await DownloadAsync(url, quality);
            if(response.StatusCode != 200) return _response.NotFound("Could not find video or audio streams in the desired quality.");
            return _response.Ok("Downloaded successfully");
        }

        public async Task<ResponseStatus> DownloadPlayList(string url, string quality)
        {
            var playlist = _client.Playlists.GetVideosAsync(url);
            if (playlist is null) return _response.BadRequest("Playlist is not found");
            await foreach(var i in playlist)
            {
               var response = await DownloadAsync(i.Url, quality);
               if (response.StatusCode != 200) return _response.Custom(response.StatusCode, response.Message!);
            }
            return _response.Ok("Downloaded successfully");
        }
        public async Task<ResponseStatus> GetVideo(string url)
        {
            var video = await _client.Videos.GetAsync(url);
            if (video == null) return _response.NotFound("The Video does not exist");

            var thumbnail = video.Thumbnails.LastOrDefault(x => x.Resolution.Width > 700);
            var stream = await _client.Videos.Streams.GetManifestAsync(video.Id);

            VideoDTO videoDTO = new VideoDTO
            {
                URL = video.Url,
                Title = video.Title,
                UploadDate = video.UploadDate,
                Duration = video.Duration,
                Qualities = GetQuality(stream),
                Thumbnail = thumbnail,
                Author = new Author
                {
                    ChannelTitle = video.Author.ChannelTitle,
                    ChannelURL = video.Author.ChannelUrl,
                }
            };

            return _response.Ok(videoDTO);
        }

        private async Task MergeAudioVideo(VideoOnlyStreamInfo streamInfoVideo, Video video, string videoFilePath, string audioFilePath)
        {
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string sanitizedTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));
            var outputFilePath = @$"{downloadsPath}\{sanitizedTitle}_{streamInfoVideo.VideoQuality.Label}.mp4";

            var ffmpegPath = @"C:\ffmpeg\ffmpeg-master-latest-win64-gpl\bin\ffmpeg.exe";
            var ffmpegArgs = $"-i \"{videoFilePath}\" -i \"{audioFilePath}\" -c:v copy -c:a aac \"{outputFilePath}\"";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = ffmpegArgs,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            Console.WriteLine($"{outputFilePath} - Downloaded Successfully");
        }

        private List<VideoQualities> GetQuality(StreamManifest stream)
        {
            var qualityList = new List<VideoQualities>();
            var streamInfoAudio = stream.GetAudioOnlyStreams()
                .OrderByDescending(x => x.Bitrate)
                .FirstOrDefault();
            var videoList = stream.GetVideoOnlyStreams()
                .Where(x => x.Container == Container.Mp4)
                .Select(x => new VideoQualities
                {
                    Quality = x.VideoQuality.Label,
                    Type = "MP4",
                    Size = (x.Size.MegaBytes + streamInfoAudio!.Size.MegaBytes).ToString()
                }).ToList();
            qualityList.AddRange(videoList);
            return qualityList;
        }

        public async Task<ResponseStatus> DownloadList(List<string> urls, string quality)
        {
            foreach (var url in urls)
            {
                var response = await DownloadAsync(url, quality);
                if (response.StatusCode != 200) return _response.Custom(response.StatusCode, response.Message!);
            }
            return _response.Ok("Downloaded successfully");
        }
    }
}
