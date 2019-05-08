using System;
namespace Safester.Models
{
    public class SettingsInfo : BaseResult
    {
        public string name { get; set; }
        public int product { get; set; }
        public string cryptographyInfo { get; set; }
        public long mailboxSize { get; set; }
        public bool notificationOn { get; set; }
        public string notificationEmail { get; set; }
    }
}
