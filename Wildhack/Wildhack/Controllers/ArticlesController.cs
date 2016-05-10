using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Wildhack.Models;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Wildhack.DAL;
using RestSharp;
using System.Collections.Specialized;
using System.Text;
using System.IO;



namespace Wildhack.Controllers
{
    public class ArticlesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Articles
        public ActionResult Index()
        {
            return View(db.Article.ToList());
        }

        public ActionResult TestArticle()
        {
            Article newArticle = GetNewArticle(Difficulty.Beginner, 2);
            ViewBag.Trans = blueMix(new Word {ID=10, Content="Hello"});
            return View(newArticle);
        }


        // GET: Articles/Details/5
        /*
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Article article = db.Article.Find(id);
            if (article == null)
            {
                return HttpNotFound();
            }
            return View(article);
        }
        */
        // GET: Articles/Create
        public ActionResult Create()
        {
            return View();
        }
        /*
        public void mailJet(Wildhack.Models.MailJet.ShareModel share)
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values["from"] = "penpal@uchicago.edu";
                values["to"] = share.ToEmail;
                values["subject"] = "New words from PenPal!";
                values["html"] = share.TextBody;
                NetworkCredential myCreds = new NetworkCredential("*", "*");
                client.Credentials = myCreds;
        */

        public void mailJet(Wildhack.Models.MailJet.ShareModel share)
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values["from"] = "aubakirova@uchicago.edu";
                values["to"] = share.ToEmail;
                values["subject"] = "New words from PenPal!";
                values["html"] = share.TextBody;
                NetworkCredential myCreds = new NetworkCredential("*", "*");
                client.Credentials = myCreds;

                var response = client.UploadValues("http://api.mailjet.com/v3/send/message", values);

                var responseString = Encoding.Default.GetString(response);
                System.Diagnostics.Debug.WriteLine(responseString);
            }

        }

        public void sendEmail(string email)
        {
            Wildhack.Models.MailJet.ShareModel share = new MailJet.ShareModel();
            share.ToEmail = email;
            Person person = db.Person.Find(1);
            if (person == null)
            {
                System.Diagnostics.Debug.WriteLine("No person");
                return;
            }
            IEnumerable<Word> list = person.Unknown;
            System.Diagnostics.Debug.Write("Count: ");
            System.Diagnostics.Debug.WriteLine(list.Count().ToString());

            foreach (Word word in list)
            {
                System.Diagnostics.Debug.WriteLine(word.Content);
            }
            // Partial Email
            var sw = new StringWriter();
            PartialViewResult result = EmailPartialView(list);

            result.View = ViewEngines.Engines.FindPartialView(ControllerContext, "EmailPartialView").View;
            ViewContext vc = new ViewContext(ControllerContext, result.View, result.ViewData, result.TempData, sw);
            result.View.Render(vc, sw);
            string emailText = sw.GetStringBuilder().ToString();

            share.TextBody = emailText;

            mailJet(share);
        }

        public PartialViewResult EmailPartialView(IEnumerable<Word> list)
        {
            return PartialView(list);
        }

        // POST: Articles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Rank,Language,RawText")] Article article)
        {
            if (ModelState.IsValid)
            {
                string mystring = article.RawText;
                db.Article.Add(article);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(article);
        }

        // GET: Articles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Article article = db.Article.Find(id);
            if (article == null)
            {
                return HttpNotFound();
            }
            return View(article);
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Rank,Language")] Article article)
        {
            if (ModelState.IsValid)
            {
                db.Entry(article).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(article);
        }

        // GET: Articles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Article article = db.Article.Find(id);
            if (article == null)
            {
                return HttpNotFound();
            }
            return View(article);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Article article = db.Article.Find(id);
            db.Article.Remove(article);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private Article GetNewArticle(Difficulty difficulty, int number)
        {
            Runspace articleRunspace = RunspaceFactory.CreateRunspace();
            articleRunspace.Open();

            RunspaceInvoke scriptInvoker = new RunspaceInvoke(articleRunspace);
            scriptInvoker.Invoke("Set-ExecutionPolicy -Scope Process -ExecutionPolicy RemoteSigned");

            Pipeline articlePipeline = articleRunspace.CreatePipeline();

            string parserPath = Server.MapPath(@"../../DAL/adilsparser.ps1");
            Command getArticleCommand = new Command("& '" + parserPath + "' -Diff " + difficulty.ToString().ToLower() + " -Num " + number, true);
            articlePipeline.Commands.Add(getArticleCommand);
            //articlePipeline.Commands.Add(getProccess);
            Collection<PSObject> articleResults = articlePipeline.Invoke();

            Article newArticle = new Article();
            newArticle.Title = articleResults[0].ToString();
            ParseArticleText(newArticle, articleResults[1].ToString());
            return newArticle;
        }

        private string GetTitleArticleByStringLevel(string level, int id)
        {
            System.Diagnostics.Debug.WriteLine(level + " " + id);
            Article a = GetNewArticle((Difficulty)Enum.Parse(typeof(Difficulty), level), id);
            System.Diagnostics.Debug.WriteLine(a.Title);
            return a.Title;
        }

        private string GetTextArticleByStringLevel(string level, int id)
        {
            Article a = GetNewArticle((Difficulty)Enum.Parse(typeof(Difficulty), level), id);
            return a.RawText;
        }

        public ActionResult Details(int? id, string level)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Article article = GetNewArticle((Difficulty)Enum.Parse(typeof(Difficulty), level), (int)id);
            if (article == null)
            {
                return HttpNotFound();
            }
            return View(article);
        }

        private void ParseArticleText(Article article, String text)
        {
            article.Words = new List<Word>();
            List<Word> wordsToAdd = new List<Word>();
            Person user = db.Person.First();

            //List<String> parseText = text.Split(new Char[] {' '}).ToList();

            int len = text.Length;
            article.RawText = String.Empty;
            int i = 0;
            while (i < len)
            {
                int spaceindex = text.IndexOf(" ");
                if (spaceindex < 0)
                    spaceindex = text.Length;
                string word = text.Substring(0, spaceindex);
                List<char> nopuncList = word.ToCharArray().Where(c => IsPunctuation(c)).ToList();

                string nopunc = String.Empty;
                foreach (char c in nopuncList)
                    nopunc += c;
                Word newWord = new Word();
                newWord.ID = db.Word.Count() + 1 + wordsToAdd.Count;
                newWord.Content = nopunc.ToLower();

                wordsToAdd.Add(newWord);
                if (user.Unknown.Count > 0 && user.Unknown.Contains(newWord))
                    article.RawText += "<span data-toggle='tooltip' data-placement='top' class=\"highlight\" id=\"" + newWord.ID + "\" title='"+getTranslation(word)+"'>" + word + "</span> ";
                else
                    article.RawText += "<span data-toggle='tooltip' data-placement='top' title='" + getTranslation(word) + "'>" + word + "</span> ";
                text = text.Substring(Math.Min(text.Length, spaceindex + 1));
                len = text.Length;
            }

            wordsToAdd = wordsToAdd.Distinct().ToList();
            wordsToAdd.ForEach(word => db.Word.Add(word));
            db.SaveChanges();

            wordsToAdd.ForEach(word =>
                article.Words.Add(db.Word.First(w => w.Content.Equals(word.Content))));

            //article.RawText = text;
            db.Article.Add(article);
            db.SaveChanges();


        }

        public bool IsPunctuation(char c)
        {
            if (c == '"' || c == '!' || c == '.' || c == ',' || c == '¡' || c == '¿' ||
                c == '\n' || c == '\t')
                return false;

            return true;
        }

        public void SaveVocab(int? id, Array uWords, Array kWords)
        {
            if (id == null)
            {
                id = 1;
            }
            Article article = db.Article.Find(id);

            if (article == null)
            {
                article = new Article();
                article.RawText = "This all started with a couple of women griping about getting cold feet. It was September and Tribune outdoors columnist Barbara Brotman and I were already anxious and unhappy. Some people would make the case that I'm always anxious and unhappy. But Barbara? She's notoriously cheery. What was worrying us both was the upcoming cold weather and our annual failure to find a solution to our big winter woe. That is, keeping our feet protected from Chicago's usual trifecta of misery: freezing temps, snow and flowing gutters of icy slush.";

                db.Article.Add(article);
                db.SaveChanges();
            }
            if (article.Unknown == null) {
                article.Unknown = new List<Word>();
            }
            Person user = db.Person.Find(1);
            System.Diagnostics.Debug.Write("User: ");
            System.Diagnostics.Debug.WriteLine(user.ID);
            if (user.Unknown == null) {
                user.Unknown = new List<Word>();
            }

            foreach (string word in uWords)
            {
                Word new_word = null;
                if(db.Word.Count() > 0)
                {
                    new_word = db.Word.SingleOrDefault(w => w.Content == word.ToLower());
                }
                if (new_word == null)
                {
                    new_word = new Word();
                    new_word.Content = (string)word;
                    new_word.Translation = blueMix(new_word);
                    System.Diagnostics.Debug.WriteLine(new_word.Content + " "+ new_word.Translation);
                    db.Word.Add(new_word);
                }
                article.Unknown.Add(new_word);
                user.Unknown.Add(new_word);
            }
            /*
            foreach (string word in kWords)
            {
                Word new_word = db.Word.SingleOrDefault(w => w.Content == word.ToLower());
                if (new_word == null)
                {
                    new_word = new Word();
                    new_word.Content = (string)word;
                    new_word.Translation = blueMix(new_word);
                    db.Word.Add(new_word);
                }

                user.Known.Add(new_word);
            }
             */
            db.SaveChanges();

            return;
        }
        /*
        public Word createOrFindWord(string text)
        {
            Word new_word = db.Word.SingleOrDefault(w => w.Content == text.ToLower());
            if (new_word == null)
            {
                new_word = new Word();
                new_word.Content = (string)text.ToLower();
                new_word.Translation = blueMix(new_word);
                db.Word.Add(new_word);
            }

            db.SaveChanges();

            return new_word;
        }
        */
        public ActionResult Vocabulary()
        {
            Person person = db.Person.Find(1);
            List<Article> list = person.ArticlesRead.ToList();
            if (list==null)
            {
                list = new List<Article>();
            }
            else if (list.Count < 1)
            {
                Article article = new Article();
                article.RawText = "This all started with a couple of women griping about getting cold feet. It was September and Tribune outdoors columnist Barbara Brotman and I were already anxious and unhappy. Some people would make the case that I'm always anxious and unhappy. But Barbara? She's notoriously cheery. What was worrying us both was the upcoming cold weather and our annual failure to find a solution to our big winter woe. That is, keeping our feet protected from Chicago's usual trifecta of misery: freezing temps, snow and flowing gutters of icy slush.";

                db.Article.Add(article);
                db.SaveChanges();
                list.Add(article);
            }

            foreach(Article a in list) {

                if (a.Unknown == null)
                {
                    a.Unknown = new List<Word>();

                    Word w1 = new Word();
                    w1.Content = "chicanery";
                    w1.Translation = "kruchkotvorstvo";

                    Word w2 = new Word();
                    w2.Content = "emae";
                    w2.Translation = "kruchkotvorstvo";


                    Word w3 = new Word();
                    w3.Content = "loliol";
                    w3.Translation = "kruchkotvorstvo";

                    a.Unknown.Add(w1);
                    a.Unknown.Add(w2);
                    a.Unknown.Add(w3);
                    db.Entry(a).State = EntityState.Modified;
                }
            }
            db.SaveChanges();
            return View(list);
        }

        public String blueMix(Word word)
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values["sid"] = "mt-eses-enus";
                values["txt"] = word.Content;
                values["rt"] = "text";
                NetworkCredential myCreds = new NetworkCredential("*", "*");
                client.Credentials = myCreds;

                var response = client.UploadValues("https://gateway.watsonplatform.net/laser/service/api/v1/smt/719e359f-a83a-4501-b917-84e915e14b38", values);

                var responseString = Encoding.Default.GetString(response);
                if(responseString != null)
                    return responseString;
            }
            return "WE COULDN'T FIND ANYTHING";
        }


        public string getTranslation(string name)
        {
            Word word = new Word();
            word.Content = name;
            string translation = blueMix(word);
            return translation;
        }
    }
}
