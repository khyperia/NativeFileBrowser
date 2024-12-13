using System;
using System.Runtime.InteropServices;

// Terrifying!

namespace NativeFileBrowser
{
    public static class WindowsCOMFileBrowser
    {
        private static string Go(int fosOptions)
        {
            var dialog = new FileOpenDialogRCW();
            var idialog = (IFileOpenDialog)dialog;
            idialog.SetOptions(fosOptions);
            var showResult = idialog.Show(IntPtr.Zero);
            string result;
            if (showResult == ERROR_CANCELLED)
                result = "";
            else
            {
                Marshal.ThrowExceptionForHR(showResult);
                var shellItem = idialog.GetResult();
                // wpf uses SIGDN_DESKTOPABSOLUTEPARSING, not SIGDN_FILESYSPATH
                // DesktopAbsoluteParsing still returns a full rooted C:\blahblah\file.txt path
                // https://github.com/dotnet/wpf/blob/fe3d2c72ea8a7acf4371592ba41d7f7b49e141b7/src/Microsoft.DotNet.Wpf/src/PresentationFramework/MS/Internal/AppModel/ShellProvider.cs#L939
                result = shellItem.GetDisplayName(SIGDN_DESKTOPABSOLUTEPARSING);
                Marshal.ReleaseComObject(shellItem);
            }

            Marshal.ReleaseComObject(dialog);
            return result;
        }

        private const int ERROR_CANCELLED = unchecked((int)0x800704C7);
        // https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/ne-shobjidl_core-sigdn
        private const int SIGDN_DESKTOPABSOLUTEPARSING = unchecked((int)0x80028000);
        // https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/ne-shobjidl_core-_fileopendialogoptions
        private const int FOS_NOCHANGEDIR = 0x8;
        private const int FOS_PICKFOLDERS = 0x20;
        private const int FOS_PATHMUSTEXIST = 0x800;
        private const int FOS_FILEMUSTEXIST = 0x1000;

        // Default flags from https://learn.microsoft.com/en-us/windows/win32/shell/common-file-dialog
        public static string PickFile() => Go(FOS_PATHMUSTEXIST | FOS_FILEMUSTEXIST | FOS_NOCHANGEDIR);

        public static string PickFolder() => Go(FOS_PATHMUSTEXIST | FOS_FILEMUSTEXIST | FOS_NOCHANGEDIR | FOS_PICKFOLDERS);

        [ComImport,
         ClassInterface(ClassInterfaceType.None),
         TypeLibType(TypeLibTypeFlags.FCanCreate),
         Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
        private class FileOpenDialogRCW
        {
        }

        // https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/PresentationFramework/MS/Internal/AppModel/ShellProvider.cs
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("D57C7288-D4AD-4768-BE02-9D969532D960")]
        internal interface IFileOpenDialog
        {
            [PreserveSig]
            int Show(IntPtr parent);

            void SetFileTypes(uint cFileTypes, [In] IntPtr rgFilterSpec);
            void SetFileTypeIndex(uint iFileType);
            uint GetFileTypeIndex();
            uint Advise([MarshalAs(UnmanagedType.Interface)] IntPtr pfde);
            void Unadvise(uint dwCookie);
            void SetOptions(int fos);
            int GetOptions();
            void SetDefaultFolder(IShellItem psi);
            void SetFolder(IShellItem psi);
            IShellItem GetFolder();
            IShellItem GetCurrentSelection();
            void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);

            [return: MarshalAs(UnmanagedType.LPWStr)]
            void GetFileName();

            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            IShellItem GetResult();
            void AddPlace(IShellItem psi, int fdcp);
            void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            void Close([MarshalAs(UnmanagedType.Error)] int hr);
            void SetClientGuid([In] ref Guid guid);
            void ClearClientData();
            void SetFilter([MarshalAs(UnmanagedType.Interface)] object pFilter);

            [return: MarshalAs(UnmanagedType.Interface)]
            IntPtr GetResults();

            [return: MarshalAs(UnmanagedType.Interface)]
            IntPtr GetSelectedItems();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        internal interface IShellItem
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            object BindToHandler(IntPtr pbc, [In] ref Guid bhid, [In] ref Guid riid);

            IShellItem GetParent();

            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetDisplayName(int sigdnName);

            uint GetAttributes(int sfgaoMask);
            int Compare(IShellItem psi, int hint);
        }
    }
}
