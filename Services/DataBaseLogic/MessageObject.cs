using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySqlConnector;
using OpenAI.Chat;
using RightHelp___Aida.Services.Constants;

namespace RightHelp___Aida.Services.DataBaseLogic
{
    internal class MessageObject
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; } // "user" ou "assistant"
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
