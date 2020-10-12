using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CodeFirstContext;
using CodeFirstModels;
using System.IO;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace Library.Controllers
{
    public class BooksController : Controller
    {
        DataContext context = new DataContext();

        //[Authorize]
        public ActionResult AllBook()
        {
            TempData.Keep("userName");

            var books = context.Books.Include(b => b.Department).Include(b => b.Publisher).Include(b => b.User);
            return View(books.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = context.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            TempData.Keep("userName");

            return View(book);
        }

        public ActionResult AddNewBook()
        {
            ViewData["dept"] = context.Departments.ToList();
            ViewData["pub"] = context.Publishers.ToList();
            ViewData["usr"] = context.Users.ToList();
            TempData.Keep("userName");

            /*
            ViewBag.DeptID = new SelectList(context.Departments, "ID", "Name");
            ViewBag.PublID = new SelectList(context.Publishers, "ID", "Name");
            ViewBag.UserID = new SelectList(context.Users, "ID", "Name");
            */
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewBook(Book book)
        {
            Book BookDB = context.Books.FirstOrDefault(b => b.ID == book.ID);

            if (ModelState.IsValid)
            {

                //check whether name is already exists in the database or not
                bool nameAlreadyExists = context.Books.Any(b => b.Title == book.Title);


                if (nameAlreadyExists)
                {
                    //adding error message to ModelState
                    ModelState.AddModelError("Title", "Book Name Already Exists.");
                    ViewData["dept"] = context.Departments.ToList();
                    ViewData["usr"] = context.Users.ToList();
                    ViewData["pub"] = context.Publishers.ToList();
                    return View(book);
                }
                if (book.Image != null && book.Image.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(book.Image.FileName);
                    string imgPath = Path.Combine(Server.MapPath("~/Image/"), fileName);
                    book.ImagePath = "~/Image/" + fileName;
                    book.Image.SaveAs(imgPath);

                }
                Author author = new Author();
                author.Name = book.AuthorName;
                //author.BookID = book.ID;
                //author.Book = book;
                //book.Authors.Add(author);
                context.Authors.Add(author);
                context.Books.Add(book);
                context.SaveChanges();
                return RedirectToAction("AllBook");
            }
            ViewData["dept"] = context.Departments.ToList();
            ViewData["usr"] = context.Users.ToList();
            ViewData["pub"] = context.Publishers.ToList();
            TempData.Keep("userName");

            //ViewBag.DeptID = new SelectList(context.Departments, "ID", "Name", book.Department.ID);
            //ViewBag.PublID = new SelectList(context.Publishers, "ID", "Name", book.Publisher.ID);
            return View(book);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = context.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewData["dept"] = context.Departments.ToList();
            ViewData["pub"] = context.Publishers.ToList();
            TempData.Keep("userName");

            //ViewBag.UserID = new SelectList(context.Users, "ID", "Name", book.UserID);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Book book)
        {
            Book BookDB = context.Books.FirstOrDefault(b => b.ID == id);
            if (!ModelState.IsValid)
            {
                ViewData["dept"] = context.Departments.ToList();
                ViewData["pub"] = context.Publishers.ToList();
                return View(book);

            }
            else
            { //checking model state

                //check whether name is already exists in the database or not
                bool nameAlreadyExists = context.Books.Any(b => b.Title == book.Title);


                if (nameAlreadyExists)
                {
                    //adding error message to ModelState
                    ModelState.AddModelError("Title", "Book Name Already Exists.");
                    ViewData["dept"] = context.Departments.ToList();
                    ViewData["pub"] = context.Publishers.ToList();
                    return View(book);
                }
            }

            if (ModelState.IsValid)
            {
                if (book.Image != null && book.Image.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(book.Image.FileName);
                    string imgPath = Path.Combine(Server.MapPath("~/Image/"), fileName);
                    book.ImagePath = "~/Image/" + fileName;
                    book.Image.SaveAs(imgPath);
                }

                //context.Authors.Add(author);

                BookDB.Title = book.Title;
                BookDB.DepartmentID = book.DepartmentID;
                BookDB.PublisherID = book.PublisherID;
                BookDB.Image = book.Image;
                BookDB.ImagePath = book.ImagePath;
                BookDB.Yeare = book.Yeare;
                BookDB.ISBN = book.ISBN;
                BookDB.AuthorName = book.AuthorName;
                BookDB.Authors = book.Authors;

                Author author = context.Authors.FirstOrDefault(a => a.BookID == book.ID);
                author.Name = BookDB.AuthorName;
                author.BookID = BookDB.ID;

                //context.Books.Add(book);
                context.SaveChanges();
                TempData.Keep("userName");

                return RedirectToAction("AllBook");
            }
            ViewData["dept"] = context.Departments.ToList();
            ViewData["pub"] = context.Publishers.ToList();
            // ViewBag.UserID = new SelectList(context.Users, "ID", "Name", book.UserID);
            return View(book);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = context.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            TempData.Keep("userName");

            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TempData.Keep("userName");

            Book book = context.Books.Find(id);
            context.Books.Remove(book);
            context.SaveChanges();

            return RedirectToAction("AllBook");
        }
        public ActionResult Borrow(int? id)
        {
            TempData.Keep("user");
            TempData.Keep("userName");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = context.Books.Find(id);
            //User user = context.Users.Find(userID);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        [HttpPost, ActionName("Borrow")]
        [ValidateAntiForgeryToken]
        public ActionResult BorrowConfirmed(int id)
        {
            TempData.Keep("user");
            TempData.Keep("userName");

            if (TempData.ContainsKey("user"))
            {
                User userTD = TempData["user"] as User;
                Message mes = new Message();
                User user = context.Users.FirstOrDefault(u => u.ID == userTD.ID);
                mes.UserID = user.ID;
                mes.User = new User()
                {
                    Name = user.Name,
                    ID = user.ID,
                    Email = user.Email,
                    Password = user.Password,
                    Books = user.Books
                };
                mes.User = user;
                //mes.time = DateTime.Today.Date;
                mes.Text = "YouAreBorrowTheBook";
                user.Messages.Add(mes);
                context.Entry(mes).State = EntityState.Added;
                context.SaveChanges();

                return RedirectToAction("Page", "Books", new { messageid = mes.ID, bookid = id });
            }
            return View();
        }

        //public ActionResult Page2(User user)
        //{

        //    //using (var context2 = new DataContext())
        //    //{
        //    //    Book bookbd = context2.Books.FirstOrDefault(b => b.ID == book.ID);
        //    //    User user = context2.Users.FirstOrDefault(u => u.ID == 1);
        //    //    book.User = user;
        //    //    book.UserID = user.ID;
        //    //    //context2.Entry(book).State = EntityState.Modified;
        //    //    user.Books.Add(book);
        //    //    bookbd = book;
        //    //    //context2.Entry(bookbd).State = EntityState.Modified;
        //    //    //context2.SaveChanges();
        //    //}

        //    return View(user);
        //}
        public ActionResult Page(int messageid,int bookid)
        {
            TempData.Keep("user");
            TempData.Keep("userName");

            Message message = context.Messages.FirstOrDefault(m => m.ID == messageid);
            TempData["UserMessage"] = message;
            User user = context.Users.FirstOrDefault(u => u.ID == message.UserID);
            //message.User = user;
            Book book = context.Books.FirstOrDefault(b => b.ID == bookid);
            book.UserID = user.ID;
            book.User = user;
            user.Books.Add(book);

            context.SaveChanges();

            return View(message);
        }

        public ActionResult Remove(int id)
        {
            TempData.Keep("UserMessage");
            TempData.Keep("user");
            TempData.Keep("userName");

            User user = TempData["user"] as User;
            Book book = context.Books.FirstOrDefault(b => b.ID == id);
            Message message = TempData["UserMessage"] as Message;
            book.UserID = null;
            book.User = null;
            user.Books.Remove(book);
            user.Messages.Remove(message);
            //context.Messages.Remove(message);

            context.SaveChanges();
            ViewData["Mess"] = "You Are Remove The Book";
            TempData.Keep("user");

            return RedirectToAction("BorrowsBook",new { id = user.ID });
        }

        public ActionResult BorrowsBook(int id)
        {
            TempData.Keep("user");
            TempData.Keep("UserMessage");
            TempData.Keep("userName");

            //Message message = context.Messages.FirstOrDefault(m => m.ID == id);
            var books = context.Books.Where(b => b.UserID == id);
            //Book book = context.Books.FirstOrDefault(b => b.ID == bookid);
            //Message message = context.Messages.FirstOrDefault(m => m.ID == messageid);
            //book.UserID = null;
            //book.User = null;
            //user.Books.Remove(book);
            //context.Messages.Remove(message);

            //context.SaveChanges();
            //ViewData["Mess"] = "You Are Remove The Book";
            return View(books);
        }
    }
}