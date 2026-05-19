using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace SimTools
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            // ── Dependency sub-folder resolution ──────────────────────────
            // Must be registered BEFORE WPF or any NuGet assembly is first
            // touched. When the runtime cannot find a DLL in the app root it
            // fires AssemblyLoadContext.Default.Resolving and we redirect it
            // to the Dependencies sub-folder next to the executable.
            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                string deps = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Dependencies");

                string path = Path.Combine(
                    deps, (name.Name ?? string.Empty) + ".dll");

                return File.Exists(path)
                    ? context.LoadFromAssemblyPath(path)
                    : null;
            };

            // ── Start WPF ─────────────────────────────────────────────────
            var app = new App();
            app.Run();
        }
    }
}