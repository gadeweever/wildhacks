using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wildhack.Models;

namespace Wildhack.DAL
{

        public class WildhackInit : System.Data.Entity.DropCreateDatabaseAlways<ApplicationDbContext>
        {

            protected override void Seed(ApplicationDbContext context)
            {
                Person newperson = new Person();
                newperson.ID = 1;
                newperson.Known = new List<Word>();
                newperson.Unknown = new List<Word>();
                context.Person.Add(newperson);
                context.SaveChanges();
            }
        
    }
}