using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TaskTrackPro.Core.Models
{
    public class t_notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int c_nid { get; set; }

        public string c_noti_title { get; set; }

        public string c_queue_name { get; set; }

        public DateTime c_created_date { get; set; }

        public int c_uid { get; set; }

        public bool c_status { get; set; }
    }
}