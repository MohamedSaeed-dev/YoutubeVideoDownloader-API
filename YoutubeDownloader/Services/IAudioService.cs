using YoutubeDownloaderCS.Models;

namespace YoutubeDownloaderCS.Services
{
    public interface IAudioService
    {
        Task<ResponseStatus> GetAudio(string url);
        Task<ResponseStatus> DownloadAudio(string url);
        Task<ResponseStatus> DownloadPlayList(string url);
        Task<ResponseStatus> DownloadList(List<string> urls);
    }
}
