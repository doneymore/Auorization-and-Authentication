using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationLinkdien.Dtos
{
    public class TokenRefresh
    {
        public string Token { get; set; }
        public string RefreshTok { get; set; }
    }
}
