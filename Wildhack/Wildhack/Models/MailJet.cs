using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wildhack.Models
{
    public class MailJet
    {
        public class ShareModel
        {
            [Required]
            [Display(Name = "Email Address")]
            [DataType(DataType.EmailAddress)]
            public string ToEmail { get; set; }
            [Required]
            [Display(Name = "Body")]
            public string TextBody { get; set; }
        }
    }
}