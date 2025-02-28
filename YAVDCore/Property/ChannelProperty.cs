using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVDCore.Methods;
using YAVDCore.Models;

namespace YAVDCore.Property
{
    public class ChannelProperty
    {
        private readonly List<ChannelModel> items;
        public List<ChannelModel> Items { get { return items; } }
        public ChannelProperty()
        {
            items = DatabaseMethods.GetChannels();
        }
    }
}