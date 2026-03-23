using FFMpegCore;
using YAVD.Core.Models;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YAVD.Core.Services
{
    public class YoutubeManager
    {
        private readonly YoutubeClient _youtube;
        public YoutubeManager() => _youtube = new YoutubeClient();
        public async Task<(string Id, string Title, string Author)> GetVideoMetadataExtendedAsync(string videoUrl)
        {
            var video = await _youtube.Videos.GetAsync(videoUrl);
            return (video.Id.Value, video.Title, video.Author.Title);
        }
        public async Task<List<(string Id, string Title, string Author)>> GetPlaylistVideosExtendedAsync(string playlistUrl)
        {
            var videos = new List<(string Id, string Title, string Author)>();
            var playlistVideos = _youtube.Playlists.GetVideosAsync(playlistUrl);
            await foreach (var video in playlistVideos)
            {
                videos.Add((video.Id.Value, video.Title, video.Author.Title));
            }
            return videos;
        }
        public async Task DownloadAudioAsync(string videoId, string savePath, string title, string artist, AudioQuality quality, IProgress<double>? progress = null)
        {
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = AppDomain.CurrentDomain.BaseDirectory });
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            string tempAudio = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"temp_{videoId}.m4a");
            try
            {
                await _youtube.Videos.Streams.DownloadAsync(streamInfo, tempAudio, progress);
         
                await FFMpegArguments
                    .FromFileInput(tempAudio)
                    .OutputToFile(savePath, true, options => options
                        .WithAudioBitrate((int)quality)
                        .WithCustomArgument($"-id3v2_version 3 -metadata title=\"{title}\" -metadata artist=\"{artist}\" -metadata comment=\"Downloaded by YAVD\""))
                    .ProcessAsynchronously();
            }
            finally { if (File.Exists(tempAudio)) File.Delete(tempAudio); }
        }

        public async Task DownloadVideoWithFFmpegAsync(string videoId, string savePath, VideoResolution targetRes, IProgress<double>? progress = null)
        {
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = AppDomain.CurrentDomain.BaseDirectory });
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);

            var videoStreamInfo = streamManifest.GetVideoOnlyStreams()
                .Where(s => s.VideoQuality.MaxHeight <= (int)targetRes)
                .OrderByDescending(s => s.VideoQuality.MaxHeight)
                .FirstOrDefault() ?? streamManifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();

            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            string tempVideo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"temp_v_{videoId}.mp4");
            string tempAudio = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"temp_a_{videoId}.m4a");

            try
            {
                await _youtube.Videos.Streams.DownloadAsync(videoStreamInfo, tempVideo, progress);
                await _youtube.Videos.Streams.DownloadAsync(audioStreamInfo, tempAudio);

                await FFMpegArguments
                    .FromFileInput(tempVideo)
                    .AddFileInput(tempAudio)
                    .OutputToFile(savePath, true, options => options.CopyChannel())
                    .ProcessAsynchronously();
            }
            finally
            {
                if (File.Exists(tempVideo)) File.Delete(tempVideo);
                if (File.Exists(tempAudio)) File.Delete(tempAudio);
            }
        }
    }
}