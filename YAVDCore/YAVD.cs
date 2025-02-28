using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVDCore.Methods;
using YAVDCore.Models;
using YAVDCore.Property;

namespace YAVDCore
{
    public class YAVD
    {        
        private readonly ChannelProperty channels;
        private readonly MainSettingProperty mainSettings;
        private readonly ApiKeyProperty apiKeys;
        public ChannelProperty Channels { get { return channels; } }
        public MainSettingProperty MainSettings { get { return mainSettings; } }
        public ApiKeyProperty ApiKey { get { return apiKeys; } }
        public YAVD()
        {
            apiKeys = new ApiKeyProperty();
            mainSettings = new MainSettingProperty();
            channels = new ChannelProperty();
        }
    }
}