using System;
using System.IO;

namespace Safester.Services
{
    public interface SettingsService
    {
        void SaveSettings(String key, String value);
        String LoadSettings(String key);

		String GetAppVersionName();

        void AskContactsPermission(Action ContactsGrantedAction);
    }
}
