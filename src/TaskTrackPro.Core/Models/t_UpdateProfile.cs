using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TaskTrackPro.Core.Models
{
    public class t_UpdateProfile
    {
        public int u_id { get; set; }

        public string c_profilepicture { get; set; }

        // public string c_newProfilepicture { get; set; } 

        public IFormFile? c_profile { get; set; }
    }
}