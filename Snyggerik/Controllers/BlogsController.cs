﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Snyggerik.Models;

namespace Snyggerik.Controllers
{
    public class BlogsController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Blogs
        //public ActionResult Index()
        //{
        //    return View(db.Blogs.ToList());
        //}

        public ActionResult MyBlogs()
        {
            var blogs = db.Blogs.Where(b => b.User.UserName == User.Identity.Name).ToList();
            return View(blogs.ToList());
        }
        // GET: Blogs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blog blog = db.Blogs.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }

        // GET: Blogs/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Blogs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BlogId,BlogTitle,BlogBody,BlogCreated")] Blog blog)
        {
            var userName = User.Identity.Name;
            var user = db.Users.Where(u => u.UserName == userName).FirstOrDefault();
            blog.User = user ;
           // blog.BlogCreated = DateTime.Now.Date;
            if (ModelState.IsValid)
            {
                db.Blogs.Add(blog);
                db.SaveChanges();
                return RedirectToAction("MyBlogs");
            }

            return View(blog);
        }

        // GET: Blogs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blog blog = db.Blogs.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }

        // POST: Blogs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BlogId,BlogTitle,BlogBody,BlogCreated")] Blog blog)
        {
            if (ModelState.IsValid)
            {
                db.Entry(blog).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("MyBlogs");
            }
            return View(blog);
        }

        // GET: Blogs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blog blog = db.Blogs.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }

        // POST: Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Blog blog = db.Blogs.Find(id);
            //Find posts
            var posts = db.Posts.Where(x => x.Blog.BlogId == id).ToList();
            
            //Remove comments an posts
            foreach(var p in posts)
            {
                var posttags = db.PostTags.Where(x => x.Post.IdPost == p.IdPost).ToList();
                var comments = db.Comments.Where(x => x.Post.IdPost == p.IdPost).ToList();
                foreach(var c in comments)
                {
                    db.Comments.Remove(c);
                }
                foreach (var pt in posttags)
                {
                    db.PostTags.Remove(pt);
                }
                db.Posts.Remove(p);
            }
            //Remove blog
            db.Blogs.Remove(blog);
            db.SaveChanges();
            return RedirectToAction("MyBlogs");
        }

        public ActionResult Show(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blog blog = db.Blogs.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }
        
        public ActionResult Index(int? sort)
        {
            List<SuperBlog> SB = new List<SuperBlog>();
            for(int i = 0; i < db.Blogs.ToList().Count; i++)
            {
            }

            foreach (var b in db.Blogs.ToList())
            {
                int views = 0;
                List<Tag> tmpTags = new List<Tag>();
                SuperBlog sb = new SuperBlog();
                sb.Blog = b;
                //get views
                List<Post> p = db.Posts.Where(x => x.Blog.BlogId == b.BlogId).ToList();
                foreach(var _p in p)
                {
                    views += _p.Views;
                    foreach(var t in _p.PostTags)
                    {
                        tmpTags.Add(t.Tag);
                    }
                   
                }
               
                sb.Tags = tmpTags.Distinct().ToList(); 
                sb.totalViews = views;
                SB.Add(sb);
                
                
            }
            if (sort != null)
            {
                switch (sort)
                {
                    case 1:
                        SB = SB.OrderBy(x => x.Blog.User.UserName).ToList();
                        ViewBag.Sort = " (by username)";
                        break;
                    case 2:
                        SB = SB.OrderBy(x => x.Blog.BlogCreated).ToList();
                        ViewBag.Sort = " (by creation)";
                        break;
                    case 3:
                        SB = SB.OrderByDescending(x => x.totalViews).ToList();
                        ViewBag.Sort = " (by views)";
                        break;
                    default:
                        break;

                }
            }
            return View(SB);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
