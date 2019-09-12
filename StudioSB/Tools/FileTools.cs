using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StudioSB.Tools
{
    public class FileTools
    {
        public static string TryOpenFolder(string title = "")
        {
            using (var dialog = new FolderSelectDialog())
            {
                if (!string.IsNullOrEmpty(title))
                    dialog.Title = title;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }

            }

            return "";
        }

        public static bool TryOpenFile(out string fileName, string filter = "")
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = filter;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = dialog.FileName;
                    return true;
                }
            }
            fileName = "";
            return false;
        }

        public static bool TryOpenFiles(out string[] fileName, string filter = "")
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Filter = filter;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = dialog.FileNames;
                    return true;
                }
            }
            fileName = new string[0];
            return false;
        }

        public static bool TrySaveFile(out string fileName, string filter = "", string defaultFileName = "")
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = filter;
                dialog.FileName = defaultFileName;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = dialog.FileName;
                    return true;
                }
            }
            fileName = "";
            return false;
        }

        /// <summary>
        /// Write RGBA8 data to PNG file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="imageData"></param>
        public static void WriteBitmapFile(string filename, int width, int height, byte[] imageData)
        {
            using (var stream = new MemoryStream(imageData))
            using (var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0,
                                                                bmp.Width,
                                                                bmp.Height),
                                                  ImageLockMode.WriteOnly,
                                                  bmp.PixelFormat);
                IntPtr pNative = bmpData.Scan0;

                Marshal.Copy(imageData, 0, pNative, imageData.Length);

                bmp.UnlockBits(bmpData);

                bmp.Save(filename);
            }
        }
    }
}