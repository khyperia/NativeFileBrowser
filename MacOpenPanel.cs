using System;
using System.Runtime.InteropServices;

namespace NativeFileBrowser
{
    public class MacOpenPanel
    {
        // libobjc.dylib: Interact with Objective-C through pure C functions via a reflection-like API
        // https://developer.apple.com/documentation/objectivec/objective-c_runtime
        [DllImport("/usr/lib/libobjc.dylib")]
        private static extern IntPtr objc_getClass([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

        [DllImport("/usr/lib/libobjc.dylib")]
        private static extern IntPtr sel_getUid([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

        // objc_msgSend is a weird one: it's a varargs-like C function (but not quite varargs, e.g. floats are not promoted to doubles).
        // Declare multiple overload aliases to the same EntryPoint in C#, and it works out fine. Add overloads for whatever you end up needing.
        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend_IntPtr_void(IntPtr receiver, IntPtr selector);

        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend_IntPtr_uint(IntPtr receiver, IntPtr selector, uint value);

        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        private static extern int objc_msgSend_int_void(IntPtr receiver, IntPtr selector);

        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_void_byte(IntPtr receiver, IntPtr selector, byte value);

        private enum NSModalResponse
        {
            Continue = -1002,
            Abort = -1001,
            Stop = -1000,
            Cancel = 0,
            Ok = 1,
        }

        // NSOpenPanel docs: https://developer.apple.com/documentation/appkit/nsopenpanel
        private static readonly IntPtr NsOpenPanel = objc_getClass("NSOpenPanel");
        private static readonly IntPtr OpenPanel = sel_getUid("openPanel");
        private static readonly IntPtr SetCanChooseFiles = sel_getUid("setCanChooseFiles:");
        private static readonly IntPtr SetCanChooseDirectories = sel_getUid("setCanChooseDirectories:");
        private static readonly IntPtr URL = sel_getUid("URL");
        private static readonly IntPtr RunModal = sel_getUid("runModal");
        private static readonly IntPtr Path = sel_getUid("path");
        private static readonly IntPtr CStringUsingEncoding = sel_getUid("cStringUsingEncoding:");

        // The documentation states canChooseFiles is a property, but "S" is missing from property_getAttributes
        // (and so property_copyAttributeValue("S") also returns null). Not sure what's up with that. I then
        // manually printed class_copyMethodList and found the name of "setCanChooseFiles:" (with colon).

        private static string Go(bool canChooseFiles, bool canChooseDirectories)
        {
            var panel = objc_msgSend_IntPtr_void(NsOpenPanel, OpenPanel);
            objc_msgSend_void_byte(panel, SetCanChooseFiles, canChooseFiles ? (byte)1 : (byte)0);
            objc_msgSend_void_byte(panel, SetCanChooseDirectories, canChooseDirectories ? (byte)1 : (byte)0);
            var response = (NSModalResponse)objc_msgSend_int_void(panel, RunModal);
            if (response != NSModalResponse.Ok)
                return "";
            var url = objc_msgSend_IntPtr_void(panel, URL);
            var str = objc_msgSend_IntPtr_void(url, Path);
            const uint utf8 = 4;
            var ptr = objc_msgSend_IntPtr_uint(str, CStringUsingEncoding, utf8);
            // No need to [panel release] or similar due to panel, url, and str being auto-released. ptr is an alias to str's data.
            return Marshal.PtrToStringUTF8(ptr);
        }

        public static string PickFile() => Go(true, false);
        public static string PickFolder() => Go(false, true);
    }
}
