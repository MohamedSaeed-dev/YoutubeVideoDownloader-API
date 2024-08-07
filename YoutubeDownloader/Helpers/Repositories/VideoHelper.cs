using AngleSharp.Io;
using System.Diagnostics;
using YoutubeDownloaderCS.Helpers.Services;
using YoutubeDownloaderCS.Models;
using YoutubeDownloaderCS.Models.Video;
using YoutubeDownloaderCS.Services;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloaderCS.Helpers.Repositories
{
    public class VideoHelper : IVideoHelper
    {
        private readonly YoutubeClient _client;
        private readonly IResponseStatus _response;
        public VideoHelper(IResponseStatus response, YoutubeClient client)
        {
            _response = response;
            _client = client;
        }
        public async Task<ResponseStatus> DownloadAsync(string url, string quality)
        {
            var (streamInfoVideo, streamInfoAudio, video) = await GetVideoAndAudio(url, quality);
            if (streamInfoVideo is null || streamInfoAudio is null || video is null) return _response.NotFound("Could not find video or audio streams in the desired quality.");
            var (audioFilePath, videoFilePath) = await DownloadVideoAudio(streamInfoVideo, streamInfoAudio);
            await MergeAudioVideo(streamInfoVideo, video, videoFilePath, audioFilePath);
            File.Delete(videoFilePath);
            File.Delete(audioFilePath);
            return _response.Ok("Success");
        }

        public Task<ResponseStatus> DownloadPlayList(string url, string quality)
        {
            throw new NotImplementedException();
        }

        public async Task<(string, string)> DownloadVideoAudio(VideoOnlyStreamInfo streamInfoVideo, AudioOnlyStreamInfo streamInfoAudio)
        {
            var audioFilePath = $"audio.{streamInfoAudio.Container.Name}";
            var videoFilePath = $"video.{streamInfoVideo.Container.Name}";

            await _client.Videos.Streams.DownloadAsync(streamInfoVideo, videoFilePath);
            await _client.Videos.Streams.DownloadAsync(streamInfoAudio, audioFilePath);

            return (audioFilePath, videoFilePath);
        }

        public List<VideoQualities> GetQuality(StreamManifest stream)
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

        public async Task<(VideoOnlyStreamInfo?, AudioOnlyStreamInfo?, Video?)> GetVideoAndAudio(string url, string quality)
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

        public async Task MergeAudioVideo(VideoOnlyStreamInfo streamInfoVideo, Video video, string videoFilePath, string audioFilePath)
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
    }
}
