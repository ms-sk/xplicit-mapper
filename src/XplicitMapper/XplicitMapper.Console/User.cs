using System;
using System.Collections.Generic;
using System.Text;

namespace XplicitMapper.Console
{
    public sealed class User
    {
        public string PwHash { get; set; } = string.Empty;

        public string Login { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
