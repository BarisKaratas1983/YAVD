﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.Core.Models
{
    public class YouTubeChannelModel
    {
        private string Id_;
        public string Title_;
        public string Description_;
        
        public string Id
        {
            get { return Id_; }
            set { Id_ = value; }
        }
        public string Title
        {
            get { return Title_; }
            set { Title_ = value; }
        }
        public string Description
        {
            get { return Description_; }
            set { Description_ = value; }
        }        
    }
}
