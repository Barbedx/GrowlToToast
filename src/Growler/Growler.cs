﻿using Growl.DisplayStyle;
using GrowlToToast.Bread;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace GrowlToToast.Growler
{
    public class Growler : Display
    {
        public Growler()
        {
            this.SettingsPanel = new GrowlerSettingsPanel();
        }

        protected override void HandleNotification(Notification notification, string displayName)
        {
            Message bread = new Message
            {
                Action = ActionType.Show,
                Title = notification.Title,
                Body = notification.Description,
                Silent = this.GetSettingOrDefault<bool>(GrowlerSetting.Silent, false),
                Image = notification.Image,
                PersistNotifications = this.GetSettingOrDefault<bool>(GrowlerSetting.PersistNotifications, false)
            };

            if (this.GetSettingOrDefault<bool>(GrowlerSetting.ShowAppName, false))
            {
                bread.AppName = notification.ApplicationName;
            }

            LaunchToaster(bread);
        }

        public override void CloseAllOpenNotifications()
        {
            if (!this.GetSettingOrDefault<bool>(GrowlerSetting.IgnoreClose, false))
            {
                LaunchToaster(new Message { Action = ActionType.CloseAll });
            }
        }

        public override void CloseLastNotification()
        {
            if (!this.GetSettingOrDefault<bool>(GrowlerSetting.IgnoreClose, false))
            {
                LaunchToaster(new Message { Action = ActionType.CloseLast });
            }
        }

        public override string Author
        {
            get { return "BobVul"; }
        }

        public override string Description
        {
            get { return "Sends Growl notifications to Windows 10 toasts"; }
        }

        public override string Name
        {
            get { return "GrowlToToast"; }
        }

        public override string Version
        {
            get {
                return (Attribute
                            .GetCustomAttribute(
                                Assembly.GetExecutingAssembly(),
                                typeof(AssemblyInformationalVersionAttribute))
                            as AssemblyInformationalVersionAttribute
                            )?.InformationalVersion ?? "Unknown";
            }
        }

        public override string Website
        {
            get { return "https://github.com/BobVul/GrowlToToast"; }
        }

        private T GetSettingOrDefault<T>(GrowlerSetting setting, T def)
        {
            string key = GrowlerSettingKeymap.GetKey(setting);
            if (!this.SettingsCollection.ContainsKey(key))
                return def;
            return (T)this.SettingsCollection[key];
        }

        private void LaunchToaster(Message bread)
        {
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                RedirectStandardInput = true,
                UseShellExecute = false,
                FileName = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"toasterpath")),
            };

            if (this.GetSettingOrDefault<bool>(GrowlerSetting.DebugLogging, false))
            {
                psi.Arguments += " --loglevel-debug";
            }

            Process p = new Process()
            {
                StartInfo = psi
            };

            p.Start();
            p.StandardInput.WriteLine(JsonConvert.SerializeObject(bread));
            p.StandardInput.Flush();
        }

        private string Base64Encode(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }
    }
}
