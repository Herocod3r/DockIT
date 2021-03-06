﻿using System;
using AspNetCore.Identity.MongoDbCore.Models;
using DocIT.Core.Data.Models;

namespace DocIT.Service.Models
{
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        public ApplicationUser() : base()
        {
        }

        public ApplicationUser(string userName, string email) : base(userName, email)
        {
        }

        public Guid UserID { get; set; }
    }
}
