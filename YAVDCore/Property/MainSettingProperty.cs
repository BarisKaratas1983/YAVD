using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVDCore.Methods;
using YAVDCore.Models;

namespace YAVDCore.Property
{
    public class MainSettingProperty
    {
        private readonly MainSettingsModel item;
        public MainSettingsModel Item { get { return item; } }
        public MainSettingProperty()
        {
            item = DatabaseMethods.GetMainSettings();
        }
    }
}