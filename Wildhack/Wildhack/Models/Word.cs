using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wildhack.Models
{
    public class Word
    {
        public int ID { get; set; }
        public String Translation { get; set; }
        public Difficulty Difficulty { get; set; }
        public String Content { get; set; }

    }


    public enum Difficulty
    {
        Absolute,
        Beginner,
        Intermediate,
        Hard,

    }
}