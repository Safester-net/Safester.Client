using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Safester.CryptoLibrary.Api;
using Safester.Models;
using Safester.Network;
using Safester.Utils;
using Xamarin.Forms;

namespace Safester.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public Command LoadSettingsCommand { get; set; }
        public Command SaveSettingsCommand { get; set; }

        public Action<bool> ResponseAction { get; set; }

        public SettingsViewModel()
        {
            LoadSettingsCommand = new Command(async () => await ExecuteLoadSettingsCommand());
            SaveSettingsCommand = new Command(async () => await ExecuteSaveSettingsCommand());
        }

        async Task ExecuteLoadSettingsCommand()
        {
            try
            {
                var settings = await ApiManager.SharedInstance().GetUserSettings(App.CurrentUser.UserEmail, App.CurrentUser.Token);
                if (settings != null)
                {
                    App.UserSettings = settings;
                    Utils.Utils.SaveDataToFile(App.UserSettings, Utils.Utils.KEY_FILE_USERSETTINGS);
                    ResponseAction?.Invoke(true);
                }
                else
                {
                    ResponseAction?.Invoke(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        async Task ExecuteSaveSettingsCommand()
        {
            try
            {
                var result = await ApiManager.SharedInstance().SetUserSettings(App.CurrentUser.UserEmail, App.CurrentUser.Token, App.UserSettings);
                if (string.IsNullOrEmpty(result) == false)
                {
                    if (result.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    {
                        Utils.Utils.SaveDataToFile(App.UserSettings, Utils.Utils.KEY_FILE_USERSETTINGS);
                        ResponseAction?.Invoke(true);
                    }
                    else
                    {
                        ResponseAction?.Invoke(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
