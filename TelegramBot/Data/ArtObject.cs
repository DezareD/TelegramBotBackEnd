using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Data
{
    public class ArtObject
    {
        [Key]
        public int Id { get; set; }

        public string Date { get; set; }
        public string AuthorName { get; set; }
    }
}
