# Native File Browser

Open a native file browser windows on Windows, MacOS, and Linux, in "pure" C# - i.e. no compiled native shim layer to call system APIs, just C#.

The original use case is for Unity projects running in Steam, but is fairly agnostic and can easily be ported to other environments.

The implementation is currently a bit barebones, but the purpose of this project
is to provide a hackable foundation as a code example to copy and modify to your
needs. I find that once a foundation is provided, it's relatively easy to add
some additional properties and method calls, instead of making the whole thing
from scratch. The fact that it's pure C# makes it much easier to edit and
adjust, rather than needing to modify and recompile native plugins (MacOS native
dylibs are especially annoying). Feel free to copy and edit this (and PR back if
you want!)

The implementations consist of:

- Windows: Using COM interop to create an instance of IFileOpenDialog
- Linux: Plain P/Invoke into gtk3. Gtk3 was chosen instead of gtk4 due to gtk3 being provided by Steam's distro-agnostic linux runtime (gtk4 is not at time of writing).
- MacOS: P/Invoke into libobjc.A.dylib (C bindings to objective-C) to create an NSOpenPanel (an objective-C type).
- Unused: WindowsGetFileName.cs uses [GetOpenFileName](https://learn.microsoft.com/en-us/windows/win32/api/commdlg/nf-commdlg-getopenfilenamea). If Windows COM breaks in Unity (perhaps with the coreclr upgrade), try this instead. It only supports file dialogs, not folder dialogs, but it's a simple P/Invoke instead of COM.

---

This project is licensed under CC0, which as far as I can tell is the most permissive and corporate-friendly license I can have. (Also debated MIT-0, but that one seems less popular and corporate-safe). You are not obligated to attribute me (unlike MIT), but it'd be nice if you did! What'd be even more nice is if you pinged me [on social media](https://bsky.app/profile/khyperia.bsky.social) or email (on [my website](https://khyperia.com/)) if you're using this - I would love to hear if my stuff is useful to you!
