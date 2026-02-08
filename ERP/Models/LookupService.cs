using ERP.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Models
{
    public class LookupService
    {
        private readonly ERPContext _context;

        public LookupService(ERPContext context)
        {
            _context = context;
        }

        public List<Users> GetUsers()
        {
            var users = _context.Users.OrderBy(x => x.UserName).ToList();
            return users;
        }
        //public List<NewsVisit> GetNewsVisit()
        //{
        //    var newsvisit = _context.NewsVisit.OrderBy(x => x.VisitID).ToList();
        //    return newsvisit;
        //}

        //public List<News_Selected_Group> GetNewsSelectedGroup()
        //{
        //    var model = _context.News_Selected_Group.OrderBy(x => x.PG_ID).ToList();
        //    return model;
        //}

        //public List<News_Comment> GetNewsComment()
        //{
        //    var model = _context.News_Comment.OrderBy(x => x.CreateDate).ToList();
        //    return model;
        //}
    }
}
