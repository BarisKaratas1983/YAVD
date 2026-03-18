using FFMpegCore;
using YAVD.Core.Models;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YAVD.Core.Services
{
    public class YoutubeManager
    {
        private readonly YoutubeClient _youtube;

        public YoutubeManager()
        {
            _youtube = new YoutubeClient();
        }
        public async Task<Channel> GetChannelMetadataAsync(string videoUrl)
        {
            var video = await _youtube.Videos.GetAsync(videoUrl);

            return new Channel
            {
                YoutubeId = video.Author.ChannelId,
                Name = video.Author.Title,
                Url = video.Author.ChannelUrl,
                LastCheckedDate = DateTime.Now
            };
        }
        public async Task<List<YAVD.Core.Models.Video>> GetNewVideosFromChannelAsync(Channel channel)
        {
            var videosFound = new List<YAVD.Core.Models.Video>();

            var uploads = _youtube.Channels.GetUploadsAsync(channel.YoutubeId);

            await foreach (var playlistVideo in uploads)
            {
                var videoDetails = await _youtube.Videos.GetAsync(playlistVideo.Id);

                if (videoDetails.UploadDate.LocalDateTime <= channel.LastCheckedDate)
                    break;

                if (videoDetails.Duration != null && videoDetails.Duration.Value.TotalSeconds <= 60)
                    continue;

                videosFound.Add(new YAVD.Core.Models.Video
                {
                    YoutubeId = videoDetails.Id,
                    Title = videoDetails.Title,
                    PublishedAt = videoDetails.UploadDate.LocalDateTime,
                    ChannelId = channel.Id,
                    IsDownloaded = false
                });
            }

            return videosFound;
        }
        public async Task DownloadAudioAsync(string videoId, string destinationPath, IProgress<double> progress = null)
        {
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            if (audioStreamInfo != null)
            {
                // progress parametresini buraya geçiyoruz
                await _youtube.Videos.Streams.DownloadAsync(audioStreamInfo, destinationPath, progress);
            }
        }
        public async Task DownloadVideoWithFFmpegAsync(string videoId, string savePath, VideoResolution targetRes, IProgress<double> progress = null)
        {
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = AppDomain.CurrentDomain.BaseDirectory });
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);

            IVideoStreamInfo videoStreamInfo = streamManifest
                .GetVideoOnlyStreams()
                .Where(s => s.VideoQuality.MaxHeight <= (int)targetRes)
                .OrderByDescending(s => s.VideoQuality.MaxHeight)
                .FirstOrDefault() ?? streamManifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();

            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            string tempVideo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"temp_v_{videoId}.mp4");
            string tempAudio = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"temp_a_{videoId}.m4a");

            try
            {
                // Video ve ses ayrı indiği için progress'i ikiye bölebiliriz 
                // veya basitlik adına sadece video indirmeyi takip edebiliriz.
                // Burada video indirme aşamasını progress ile takip ediyoruz:
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