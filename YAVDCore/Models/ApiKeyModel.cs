using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVDCore.Models
{
    public class ApiKeyModel
    {
        public int ApiKeyId { get; set; }
        public string ApiKey { get; set; }
        public int Active { get; set; }
        public DateTime CreateDateTime { get; set; }        
    }
}