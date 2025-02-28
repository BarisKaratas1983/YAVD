using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVDCore.Methods;
using YAVDCore.Models;

namespace YAVDCore.Property
{
    public class ApiKeyProperty
    {
        private readonly List<ApiKeyModel> items;
        public List<ApiKeyModel> Items { get { return items; } }
        public ApiKeyProperty()
        {
            items = DatabaseMethods.GetApiKeys();
        }
    }
}
