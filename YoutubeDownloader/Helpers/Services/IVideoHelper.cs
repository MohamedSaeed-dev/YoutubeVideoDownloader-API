using YoutubeDownloaderCS.Models;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using YoutubeDownloaderCS.Models.Video;

namespace YoutubeDownloaderCS.Helpers.Services
{
    public interface IVideoHelper
    {
        Task<ResponseStatus> DownloadPlayList(string url, string quality);
        Task<(VideoOnlyStreamInfo?, AudioOnlyStreamInfo?, Video?)> GetVideoAndAudio(string url, string quality);
        Task<(string, string)> DownloadVideoAudio(VideoOnlyStreamInfo streamInfoVideo, AudioOnlyStreamInfo streamInfoAudio);
        Task<ResponseStatus> DownloadAsync(string url, string quality);
        Task MergeAudioVideo(VideoOnlyStreamInfo streamInfoVideo, Video video, string videoFilePath, string audioFilePath);
        List<VideoQualities> GetQuality(StreamManifest stream);
    }
}
