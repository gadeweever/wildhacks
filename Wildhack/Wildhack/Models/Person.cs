using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wildhack.Models
{
    public class Person
    {
        public int ID { get; set; }
        public virtual ICollection<Word> Known { get; set; }
        public virtual ICollection<Word> Unknown { get; set; }

        public virtual ICollection<Article> ArticlesRead { get; set; }



    }
}