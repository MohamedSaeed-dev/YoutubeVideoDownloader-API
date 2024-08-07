using YoutubeExplode.Common;

namespace YoutubeDownloaderCS.Models.Video
{
    public class VideoDTO
    {
        public string Title { get; set; } = string.Empty;
        public string URL { get; set; } = string.Empty;
        public List<VideoQualities>? Qualities { get; set; }
        public Thumbnail? Thumbnail { get; set; }
        public Author? Author { get; set; }
        public DateTimeOffset UploadDate { get; set; }
        public TimeSpan? Duration { get; set; }

    }
}
