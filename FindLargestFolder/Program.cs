using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows.Forms;
using ByteSizeLib;
using High_DPI_Handler;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace FindLargestFolder
{
    class Program
    {

        static List<string> fileNames = new List<string>();
        static List<long> fileSizes = new List<long>();

        [STAThread]
        static void Main(string[] args)
        {
            HighDPIHandler.EnableHighDPISupport();

            Console.Title = "FindLargestFolder v1.0";

            Console.WriteLine("FindLargestFolder v1.0");

            OpenFileDialog folderBrowser = new OpenFileDialog();
            // Set validate names and check file exists to false otherwise windows will
            // not let you select "Folder Selection."
            folderBrowser.ValidateNames = false;
            folderBrowser.CheckFileExists = false;
            folderBrowser.CheckPathExists = true;
            // Always default to Folder Selection.
            folderBrowser.FileName = "Folder Selection.";
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                string folderPath = Path.GetDirectoryName(folderBrowser.FileName);
                string[] subDirectoryEntries = Directory.GetDirectories(folderPath);
                int fileIndex = 1;
                if (subDirectoryEntries.Length == 0)
                {
                    MessageBox.Show("Cannot find any folders in this directory");
                    Application.Exit();
                    Environment.Exit(0);
                }
                Console.WriteLine("Checking your folders...");
                foreach (string subDirectory in subDirectoryEntries)
                {
                    fileNames.Add(subDirectory);
                    fileSizes.Add(DirSize(new DirectoryInfo(subDirectory)));
                    drawTextProgressBar(fileIndex, subDirectoryEntries.Length);
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                    TaskbarManager.Instance.SetProgressValue(fileIndex, subDirectoryEntries.Length);
                    fileIndex++;
                }
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                Console.WriteLine("\n");
                if (fileNames.Count == fileSizes.Count)
                {
                    for (int i = 0; i < fileNames.Count; i++)
                    {
                        var fileSize = ByteSize.FromBytes(fileSizes[i]);
                        Console.WriteLine(fileNames[i] + " " + fileSize.MegaBytes.ToString() + "MB");
                    }
                }
                Console.WriteLine("\n");
                int index = 0;
                Console.WriteLine("Scanning your folders...");
                for (int i = 0; i < fileSizes.Count; i++)
                {
                    if (fileSizes.Max() == fileSizes[i])
                    {
                        index = i;
                    }
                    drawTextProgressBar(i + 1, subDirectoryEntries.Length);
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                    TaskbarManager.Instance.SetProgressValue(i + 1, subDirectoryEntries.Length);
                }
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                Console.WriteLine("\n");
                Console.WriteLine("The biggest folder in the directory is:");
                Console.WriteLine(fileNames[index] + " " + ByteSize.FromBytes(fileSizes[index]).MegaBytes + "MB");

                Console.WriteLine("restart - restart the application");
                Console.WriteLine("navigate - open the folder in explorer.exe");
                Console.Write("Insert Command: ");

                string command = Console.ReadLine();

                switch (command)
                {
                    case "restart" :
                        Application.Restart();
                        break;

                    case "navigate":
                        // opens the folder in explorer
                        Process.Start(fileNames[index]);
                        break;

                    default:
                        MessageBox.Show("Unknown Command");
                        Application.Exit();
                        Environment.Exit(0);
                        break;
                }
            } else
            {
                Application.Exit();
                Environment.Exit(0);
            }
        }

        static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = null;
            try
            {
                fis = d.GetFiles();
            }
            catch (Exception)
            {
                MessageBox.Show("Don't have access to the folder");
                if (!Program.IsAdministrator())
                {
                    MessageBox.Show("Trying to restart application in Administrator mode");
                    // Restart and run as admin
                    var exeName = Process.GetCurrentProcess().MainModule.FileName;
                    ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                    startInfo.Verb = "runas";
                    startInfo.Arguments = "restart";
                    Process.Start(startInfo);
                    Application.Exit();
                }
                MessageBox.Show("Please grant permission to yourself");
                Application.Exit();
                Environment.Exit(0);
            }
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        static void drawTextProgressBar(int progress, int total)
        {
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }

        static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

    }
}
