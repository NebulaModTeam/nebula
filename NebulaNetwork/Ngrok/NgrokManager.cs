using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using NebulaModel;
using NebulaModel.Utils;
using NebulaWorld;

namespace NebulaNetwork.Ngrok
{
    public class NgrokManager
    {
        private readonly string _ngrokPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/ngrok-v3-stable-windows-amd64/ngrok.exe";
        private readonly int _port;
        private readonly string _authtoken;

        private Process _ngrokProcess;

        public string NgrokAddress;

        public NgrokManager(int port, string authtoken = null)
        {
            _port = port;
            _authtoken = authtoken ?? Config.Options.NgrokAuthtoken;

            if (!Config.Options.EnableNgrok)
            {
                return;
            }

            if (string.IsNullOrEmpty(_authtoken))
            {
                NebulaModel.Logger.Log.Warn("Ngrok support was enabled, however no Authtoken was provided");
                return;
            }

            // Start this stuff in it's own thread, as we require async and we dont want to freeze up the GUI when freeze up when Downloading and installing ngrok
            Task.Run(async () =>
            {

                if (!IsNgrokInstalled())
                {
                    var hasDownloadAndInstallBeenConfirmed = false;
                    var downloadAndInstallConfirmation = new Task<bool>(() => true);
                    var downloadAndInstallRejection = new Task<bool>(() => false);

                    UnityDispatchQueue.RunOnMainThread(() =>
                    {
                        InGamePopup.ShowWarning(
                            "Ngrok download and installation confirmation",
                            "Ngrok is support is enabled, however it has not been downloaded and installed yet, do you want to automattically download and install Ngrok?",
                            "Accept",
                            "Reject",
                            () => downloadAndInstallConfirmation.Start(),
                            () => downloadAndInstallRejection.Start()
                        );
                    });

                    hasDownloadAndInstallBeenConfirmed = await await Task.WhenAny(downloadAndInstallConfirmation, downloadAndInstallRejection);
                    if (!hasDownloadAndInstallBeenConfirmed)
                    {
                        NebulaModel.Logger.Log.Warn("Failed to download or install Ngrok, because user rejected Ngrok download and install confirmation!");
                        return;
                    }             
                    
                    try
                    {
                        await DownloadAndInstallNgrok();
                    }
                    catch
                    {
                        NebulaModel.Logger.Log.Warn("Failed to download or install Ngrok!");
                        throw;
                    }

                }

                if (!StartNgrok())
                {
                    NebulaModel.Logger.Log.Warn("Failed to start Ngrok tunnel!");
                    return;
                }

                if (!IsNgrokStarted())
                {
                    NebulaModel.Logger.Log.Warn("Ngrok tunnel has exitted prematurely! Invalid authtoken perhaps?");
                    return;
                }

                try
                {
                    NgrokAddress = await GetTunnelAdress();
                }
                catch
                {
                    NebulaModel.Logger.Log.Warn("Failed to obtain Ngrok address!");
                    throw;
                }

            });

        }

        public async Task DownloadAndInstallNgrok()
        {
            using (var client = new HttpClient())
            {
                using (var s = client.GetStreamAsync("https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-windows-amd64.zip"))
                {
                    using (var zip = new ZipArchive(await s, ZipArchiveMode.Read))
                    {
                        zip.ExtractToDirectory(Path.GetDirectoryName(_ngrokPath));
                    }
                }
            }
        }

        public bool IsNgrokInstalled()
        {
            return File.Exists(_ngrokPath);
        }

        public bool StartNgrok()
        {
            StopNgrok();

            _ngrokProcess = new Process();
            _ngrokProcess.StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = _ngrokPath,
                Arguments = $"tcp {_port} --authtoken {_authtoken}"
            };
            return _ngrokProcess.Start();
        }

        public void StopNgrok()
        {
            if (_ngrokProcess != null)
            {
                _ngrokProcess.Refresh();
                if (!_ngrokProcess.HasExited)
                {
                    _ngrokProcess.Kill();
                    _ngrokProcess.Close();
                }
                _ngrokProcess = null;
            }
        }

        public bool IsNgrokStarted()
        {
            if (_ngrokProcess == null)
            {
                return false;
            }

            _ngrokProcess.Refresh();
            return !_ngrokProcess.HasExited;
        }

        public async Task<string> GetTunnelAdress()
        {
            if (!IsNgrokStarted())
            {
                throw new Exception("Not able to get Ngrok tunnel address because Ngrok is not started (or exitted prematurely)");
            }

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("http://localhost:4040/api/tunnels");
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();

                    var json = MiniJson.Deserialize(body) as Dictionary<string, object>;

                    var tunnels = json["tunnels"] as List<object>;

                    string publicUrl = null;
                    foreach (Dictionary<string, object> tunnel in tunnels)
                    {
                        if (tunnel["proto"] as string == "tcp" && (tunnel["config"] as Dictionary<string, object>)["addr"] as string == $"localhost:{_port}")
                        {
                            publicUrl = tunnel["public_url"] as string;
                            break;
                        }
                    }

                    if (publicUrl == null)
                    {
                        throw new Exception("Not able to get Ngrok tunnel address because no matching tunnel was found in API response");
                    }

                    return publicUrl.Replace("tcp://", "");

                } else
                {
                    throw new Exception("Could not access the ngrok API");
                }
            }
        }
    }
}
