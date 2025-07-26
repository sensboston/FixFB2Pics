using System;
using System.IO;
using System.Xml;
using System.Text;
using ImageMagick;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FixFB2Pics
{
    class Program
    {
        private static MagickColor backgroundColor = MagickColors.White;

        static void Main(string[] args)
        {
            if (args.Length == 0 || !File.Exists(args[0]))
            {
                Console.WriteLine("Usage: FixFB2Pics.exe <input.fb2> [--background=transparent|#RRGGBB]");
                return;
            }

            LoadNativeDll();
            ParseArguments(args);

            string fb2Path = args[0];
            string fb2Text = File.ReadAllText(fb2Path, Encoding.UTF8);

            var doc = new XmlDocument();
            doc.LoadXml(fb2Text);

            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("fb", doc.DocumentElement.NamespaceURI);

            var binaryNodes = doc.SelectNodes("//fb:binary", namespaceManager);

            int total = binaryNodes.Count;
            int modified = 0;

            foreach (XmlNode binNode in binaryNodes)
            {
                string id = binNode.Attributes["id"]?.Value;
                string contentType = binNode.Attributes["content-type"]?.Value ?? "";
                string base64 = binNode.InnerText.Trim();

                byte[] imageBytes;
                try
                {
                    imageBytes = Convert.FromBase64String(base64);
                }
                catch
                {
                    Console.WriteLine($"[WARN] Invalid base64 for id={id}, skipping.");
                    continue;
                }

                MagickFormat format;
                try
                {
                    using (var img = new MagickImage(imageBytes))
                    {
                        format = img.Format;

                        if (format == MagickFormat.Jpeg || format == MagickFormat.Png)
                            continue;

                        byte[] jpegBytes;

                        if (img.HasAlpha && backgroundColor != null)
                        {
                            using (var flattened = img.Clone())
                            {
                                flattened.BackgroundColor = backgroundColor;
                                flattened.Alpha(AlphaOption.Remove);
                                flattened.Format = MagickFormat.Jpeg;
                                flattened.Quality = 80;

                                using (var ms = new MemoryStream())
                                {
                                    flattened.Write(ms);
                                    jpegBytes = ms.ToArray();
                                }
                            }
                        }
                        else
                        {
                            img.Format = MagickFormat.Jpeg;
                            img.Quality = 80;

                            using (var ms = new MemoryStream())
                            {
                                img.Write(ms);
                                jpegBytes = ms.ToArray();
                            }
                        }

                        string newBase64 = Convert.ToBase64String(jpegBytes);
                        binNode.InnerText = newBase64;
                        binNode.Attributes["content-type"].Value = "image/jpeg";

                        modified++;
                        Console.WriteLine($"[INFO] Converted id={id} from {format} to JPEG.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Could not process id={id}: {ex.Message}");
                }
            }

            if (modified > 0)
            {
                string outPath = Path.Combine(
                    Path.GetDirectoryName(fb2Path),
                    Path.GetFileNameWithoutExtension(fb2Path) + "_fixed.fb2");

                using (var writer = new XmlTextWriter(outPath, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    doc.Save(writer);
                }

                Console.WriteLine($"\nDone. Modified: {modified} of {total} binaries.");
                Console.WriteLine($"Saved as: {outPath}");
            }
            else
            {
                Console.WriteLine("\nNo changes made.");
            }
        }

        // Parses optional arguments, e.g. --background=transparent or --background=#RRGGBB
        static void ParseArguments(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("--background=", StringComparison.OrdinalIgnoreCase))
                {
                    string value = arg.Substring("--background=".Length).Trim();

                    if (string.Equals(value, "transparent", StringComparison.OrdinalIgnoreCase))
                        backgroundColor = null;
                    else
                        backgroundColor = new MagickColor(value);
                }
            }
        }

        #region Native DLL Loading

        private const string DllName = "Magick.Native-Q16-x86.dll";
        private static string ExtractedPath;

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        // Extracts and loads the embedded native DLL at runtime
        public static void LoadNativeDll()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = GetResourceName(assembly, DllName);

            if (resourceName == null)
                throw new InvalidOperationException("Native DLL not embedded as resource.");

            string tempDir = Path.Combine(Path.GetTempPath(), "FixFB2Pics.Native");
            Directory.CreateDirectory(tempDir);
            ExtractedPath = Path.Combine(tempDir, DllName);

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (FileStream fs = new FileStream(ExtractedPath, FileMode.Create, FileAccess.Write))
                stream.CopyTo(fs);

            IntPtr handle = LoadLibrary(ExtractedPath);
            if (handle == IntPtr.Zero)
            {
                int err = Marshal.GetLastWin32Error();
                throw new Exception($"Failed to load native DLL. Error code: {err}");
            }
        }

        // Finds the fully qualified name of the embedded DLL resource
        private static string GetResourceName(Assembly asm, string dllName)
        {
            foreach (string name in asm.GetManifestResourceNames())
            {
                if (name.EndsWith(dllName, StringComparison.OrdinalIgnoreCase))
                    return name;
            }
            return null;
        }

        #endregion
    }
}
