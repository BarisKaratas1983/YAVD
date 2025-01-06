using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YAVD.Core.Methods;
using YAVD.Core.Models;

namespace YAVD.Core.Base
{
    public class YAVDBase
    {
        private ApiKeyBase _apiKeys = new ApiKeyBase();
        public ApiKeyBase ApiKeys {  get { return _apiKeys; } }
        public YAVDBase()
        {

        }
    }
}
