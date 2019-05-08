using System;
using System.Collections.Generic;
using System.Text;

namespace Safester.Models
{
    public enum MenuItemType
    {
        Inbox = 1,
        Sent,
        Drafts,
        Trash,
        Contacts,
        Settings,
        ChangePass,
        About,
        Logout
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Image { get; set; }

        public string Title { get; set; }
    }
}
