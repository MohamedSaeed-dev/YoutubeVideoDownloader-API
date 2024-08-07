using YoutubeDownloaderCS.Models;

namespace YoutubeDownloaderCS.Services
{
    public interface IVideoService
    {
        Task<ResponseStatus> GetVideo(string url);
        Task<ResponseStatus> DownloadVideo(string url, string quality);
        Task<ResponseStatus> DownloadPlayList(string url, string quality);
        Task<ResponseStatus> DownloadList(List<string> urls, string quality);
    }
}
