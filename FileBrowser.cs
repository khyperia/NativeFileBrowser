using UnityEngine;

namespace NativeFileBrowser
{
    public static class FileBrowser
    {
        public static string PickFile()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor or RuntimePlatform.WindowsPlayer:
                    return WindowsCOMFileBrowser.PickFile();
                case RuntimePlatform.OSXEditor or RuntimePlatform.OSXPlayer:
                    return MacOpenPanel.PickFile();
                case RuntimePlatform.LinuxEditor or RuntimePlatform.LinuxPlayer:
                    return Gtk3FileDialog.PickFile();
                default:
                    return "";
            }
        }

        public static string PickFolder()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor or RuntimePlatform.WindowsPlayer:
                    return WindowsCOMFileBrowser.PickFolder();
                case RuntimePlatform.OSXEditor or RuntimePlatform.OSXPlayer:
                    return MacOpenPanel.PickFolder();
                case RuntimePlatform.LinuxEditor or RuntimePlatform.LinuxPlayer:
                    return Gtk3FileDialog.PickFolder();
                default:
                    return "";
            }
        }
    }
}
