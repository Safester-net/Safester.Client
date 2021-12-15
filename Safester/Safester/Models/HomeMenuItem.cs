using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace Safester.Models
{
    public enum MenuItemType
    {
        Compose = 0,
        Inbox,
        Sent,
        Starred,
        Search,
        Drafts,
        Trash,
        Contacts,
        Settings,
        TwoFactorSettings,
        ChangePass,
        About,
        Users,
        Logout
    }

    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Image { get; set; }

        public string Title { get; set; }
    }
}
