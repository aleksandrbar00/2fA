using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using _2FA;

namespace _2FA
{
    class AuthData
    {
        public string login { get; set; }
        public string pwd { get; set; }
        public string sn { get; set; }

        public AuthData(string login, string pwd, string sn)
        {
            this.login = login;
            this.pwd = pwd;
            this.sn = sn;
        }
    }
}
