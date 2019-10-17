using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Models
{
    public class ApiAuthResponse
    {
        public string BToken { set; get; }

        public string RToken { set; get; }

        public bool IsValid { set; get; }

        public int UserId { set; get; }

        public string DisplayName { set; get; }
    }
}
