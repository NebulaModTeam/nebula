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
using System.Linq;

namespace NebulaNetwork.Ngrok
{
    public class NgrokManager
    {
        private readonly string _ngrokPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/ngrok-v3-stable-windows-amd64/ngrok.exe";
        private readonly int _port;
        private readonly string _authtoken;
        private readonly string _region;

        private Process _ngrokProcess;

        public string NgrokAddress;

        public NgrokManager(int port, string authtoken = null, string region = null)
        {
            _port = port;
            _authtoken = authtoken ?? Config.Options.NgrokAuthtoken;
            _region = region ?? Config.Options.NgrokRegion;
            //_ngrokProcess = Process.GetProcessesByName("ngrok").FirstOrDefault();

            if (!Config.Options.EnableNgrok)
            {
                return;
            }

            if (string.IsNullOrEmpty(_authtoken))
            {
                NebulaModel.Logger.Log.Warn("Ngrok support was enabled, however no Authtoken was provided");
                return;
            }

            // Validate the Ngrok region
            string[] availableRegions = { "us", "eu", "au", "ap", "sa", "jp", "in" };
            if (!string.IsNullOrEmpty(_region) && !availableRegions.Any(_region.Contains))
            {
                NebulaModel.Logger.Log.Warn("Unsupported Ngrok region was provided, defaulting to autodetection");
                _region = null;
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

                if (!IsNgrokActive())
                {
                    NebulaModel.Logger.Log.Warn("Ngrok tunnel has exitted prematurely! Invalid authtoken perhaps?");
                    return;
                }

                try
                {
                    NgrokAddress = await GetTunnelAddress();
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
                Arguments = $"tcp {_port} --authtoken {_authtoken} --log stdout --log-format json" + (!string.IsNullOrEmpty(_region) ? $" --region {_region}" : ""),
            };

            _ngrokProcess.OutputDataReceived += new DataReceivedEventHandler((sender, e) => { 
                if (!string.IsNullOrEmpty(e.Data))
                {
                    NebulaModel.Logger.Log.Info($"Ngrok Stdout: {e.Data}");
                }
            });
            _ngrokProcess.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    NebulaModel.Logger.Log.Error($"Ngrok Stderr: {e.Data}");
                }
            });

            var started = _ngrokProcess.Start();
            if (IsNgrokActive())
            {
                // This links the process as a child process by attaching a null debugger thus ensuring that the process is killed when its parent dies.
                new ChildProcessLinker(_ngrokProcess);
            }
            _ngrokProcess.BeginOutputReadLine();
            _ngrokProcess.BeginErrorReadLine();

            return started;
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

        public bool IsNgrokActive()
        {
            if (_ngrokProcess == null)
            {
                return false;
            }

            _ngrokProcess.Refresh();
            return !_ngrokProcess.HasExited;
        }

        public async Task<string> GetTunnelAddress()
        {
            if (!IsNgrokActive())
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
