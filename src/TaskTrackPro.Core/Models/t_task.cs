using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TaskTrackPro.Core.Models
{
    public class t_task
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int c_tid { get; set; }

        public int c_uid { get; set; }

        public string c_task_title { get; set; }

        public string c_description { get; set; }

        public DateTime c_start_date { get; set; }

        public DateTime c_end_date { get; set; }

        public string c_task_status { get; set; }
    }
}