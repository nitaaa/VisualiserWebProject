using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.EnterpriseServices;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using VisualiserWebProject.Models;

namespace VisualiserWebProject.Controllers
{
    public class UsersController : Controller
    {
        private QuizVisualiserDatabaseEntities db = new QuizVisualiserDatabaseEntities();

        // GET: Users
        [Authorize]
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        // GET: Users/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Users/Register
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "userFirstName,userLastName,title,emailAddress,password")] User user)
        {
            //TODO: CREATE SALT AND HASH PASSWORD
            user.salt = createSalt();
            byte[] saltByteArray = Encoding.Default.GetBytes(user.salt);

            //https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing?view=aspnetcore-7.0
            //https://andrewlock.net/exploring-the-asp-net-core-identity-passwordhasher/
            user.password = Convert.ToBase64String(KeyDerivation.Pbkdf2(user.password, saltByteArray, KeyDerivationPrf.HMACSHA256, 100000, 256 / 8));
            

            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
            
        }

        private string createSalt()
        {
            //string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            //UTF16 Characters: https://asecuritysite.com/coding/asc2?val=0%2C255
            //0-9: 48-57 || A-Z: 65-90 || a-z: 97-122
            Random random = new Random();
            string salt = string.Empty;
            while (salt.Length < 5)
            {
                String alphanum = random.Next(48, 57).ToString() + Convert.ToChar(random.Next(65, 90)) + Convert.ToChar(random.Next(97, 122));
                salt += alphanum[random.Next(3)];
            }
            return salt;
        }

        // GET: Users/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID,userFirstName,userLastName,title,emailAddress,password,salt")] User user)
        {
            byte[] saltByteArray = Encoding.Default.GetBytes(user.salt);
            user.password = Convert.ToBase64String(KeyDerivation.Pbkdf2(user.password, saltByteArray, KeyDerivationPrf.HMACSHA256, 100000, 256 / 8));

            if (ModelState.IsValid)
            {
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
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

        /*Log In View
         */
        public ActionResult Login()
        {
            return View();
        }

        /*Log In Validation and redirects
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "emailAddress,password")] User user)
        {
            try
            {
                if (ValidateUser(user.emailAddress, user.password))
                {
                    FormsAuthentication.SetAuthCookie(user.emailAddress, true);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "User Not Found");
                    return RedirectToAction("Login");
                }
            }
            catch
            {
                return View();
            }
        }

        /*User validation - Checking that the users email and password match stored values
         */
        private bool ValidateUser(string email, string password)
        {
            bool flag = false;
            //connectiong to the database (web.config file)
            string ConnString = ConfigurationManager.ConnectionStrings["VisualiserModels"].ConnectionString;
            SqlConnection dbConn = new SqlConnection(ConnString);
            //SQL command to get users with a matching email address
            string cmd = "SELECT * FROM [User] WHERE [emailAddress] = '" + email + "'";
            SqlCommand command = new SqlCommand(cmd, dbConn);
            
            //Opening the database connection and reading the results of the SQL command
            dbConn.Open();
            SqlDataReader Reader = command.ExecuteReader();
            //While there are still remaining records, check the record password agaisnt the password given (after the salt and hash have been applied)
            //if the hashed passwords match, then set the result to TRUE and the user is valid/can be logged in
            while (Reader.Read())
            {
                IDataRecord record = (IDataRecord)Reader;
                User curUser = new User();
                curUser.password = record[5].ToString();
                curUser.salt = record[6].ToString();
                byte[] saltByteArray = Encoding.Default.GetBytes(curUser.salt);

                string _password = Convert.ToBase64String(KeyDerivation.Pbkdf2(password, saltByteArray, KeyDerivationPrf.HMACSHA256, 100000, 256 / 8));
                if (curUser.password == _password)
                {
                    flag = true;
                    break;
                }
            }
            //close DB connection
            dbConn.Close();

            return flag;
        }

        /*Logging out of the web application
         */
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }
    }
}
