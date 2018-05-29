using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class MailConfig
    {
        public String ServerSMTP { get; set; }
        public Int32 portSMTP { get; set; }
        public String Account { get; set; }
        public String AccountPass { get; set; }
        public Boolean useSSL { get; set; }
    }
}
