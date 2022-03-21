using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Data
{
    public class Applicant
    {
        [Key]
        public int Id { get; set; }

        public string Text { get; set; }
        public string FileId { get; set; }

        public string Status { get; set; }
        // status_waiting
        // status_banned
        // status_ok

        public long AuthorId { get; set; }
    }
}
