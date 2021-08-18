using System;
using System.Collections.Generic;
using System.Text;

namespace SampleShared
{
    public class EmailModel
    {
        public string from { get; set; }
        public string To { get; set; }
        public string mail_subject { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string mail_message { get; set; }
        public string attachement { get; set; }
        public bool isBodyHtml { get; set; }
    }
}
