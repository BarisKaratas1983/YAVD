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
        public async Task DownloadAudioAsync(string videoId, string destinationPath)
        {            
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);
         
            var audioStreamInfo = streamManifest
                .GetAudioOnlyStreams()
                .GetWithHighestBitrate();

            if (audioStreamInfo != null)
            {                
                await _youtube.Videos.Streams.DownloadAsync(audioStreamInfo, destinationPath);
            }
        }
    }
}