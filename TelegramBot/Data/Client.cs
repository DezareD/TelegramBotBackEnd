using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Data
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }                // Название пользователя
        public long UserId { get; set; }                // Айди пользователя
        public string? UniqRoleName { get; set; }       // Роль пользователя

        public long ChatId { get; set; }                // Чат с этим пользователем для связи
        public bool isBanned { get; set; }              // Забанен ли пользователь?
    }
}
