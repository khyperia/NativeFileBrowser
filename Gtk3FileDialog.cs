using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace NativeFileBrowser
{
    public static class Gtk3FileDialog
    {
        // Steam's runtime does not provide gtk4 (which is nicer), so, use gtk3, which *is* provided by steam
        // TODO: Somehow check if this is actually loading Steam's version and not the system one?
        private const string So = "libgtk-3.so.0";

        [DllImport(So)]
        private static extern void gtk_init(IntPtr argc, IntPtr argv);

        [DllImport(So)]
        private static extern int gtk_main_iteration_do(int blocking);

        // gtk_file_chooser_dialog_new is a varargs function, hardcode one set of buttons
        [DllImport(So)]
        private static extern IntPtr gtk_file_chooser_dialog_new(IntPtr title, IntPtr parent, int action, IntPtr buttonName, int responseType, IntPtr zero);

        [DllImport(So)]
        private static extern IntPtr gtk_file_chooser_get_filename(IntPtr dialog);

        [DllImport(So)]
        private static extern void g_free(IntPtr mem);

        [DllImport(So)]
        private static extern int gtk_dialog_run(IntPtr dialog);

        [DllImport(So)]
        private static extern int gtk_widget_destroy(IntPtr widget);

        static Gtk3FileDialog()
        {
            Debug.Log("calling gtk_init");
            gtk_init(IntPtr.Zero, IntPtr.Zero);
        }

        private static string Go(int action)
        {
            var select = Marshal.StringToCoTaskMemUTF8("Select");
            const int accept = -3;
            var dialog = gtk_file_chooser_dialog_new(IntPtr.Zero, IntPtr.Zero, action, select, accept, IntPtr.Zero);
            var res = gtk_dialog_run(dialog);
            string result;
            if (res == accept)
            {
                var filename = gtk_file_chooser_get_filename(dialog);
                result = Marshal.PtrToStringUTF8(filename);
                g_free(filename);
            }
            else
            {
                result = "";
            }

            gtk_widget_destroy(dialog);
            Marshal.FreeCoTaskMem(select);

            // calling main_iteration_do is required to close the window once accepted
            UpdateObject.EnsureOpen();

            return result;
        }

        public static string PickFile() => Go(0);

        public static string PickFolder() => Go(2);

        private class UpdateObject : MonoBehaviour
        {
            private static UpdateObject _instance;

            private void Update() => gtk_main_iteration_do(0);

            public static void EnsureOpen()
            {
                if (!_instance) _instance = new GameObject(nameof(Gtk3FileDialog)).AddComponent<UpdateObject>();
            }
        }
    }
}
