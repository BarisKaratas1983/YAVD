using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVD.Core.Methods;
using YAVD.Core.Models;

namespace YAVD.Core.Base
{
    public class ApiKeyBase
    {
        private List<ApiKeyModel> _apiKeys;
        public List<ApiKeyModel> ApiKeys { get { return _apiKeys; } }
        public void GetApiKeys()
        {
            _apiKeys = DatabaseMethods.GetApiKeys();
        }
    }
}