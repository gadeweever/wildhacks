using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wildhack.Models
{
    public class Article
    {

        public int ID { get; set; }
        public int Rank { get; set; }
        public string Title { get; set; }
        public string RawText { get; set; }
        public virtual ICollection<Word> Words { get; set; }
        public virtual ICollection<Word> Known { get; set; }
        public virtual ICollection<Word> Unknown { get; set; }
        public string Language { get; set; }
    }
}