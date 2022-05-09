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
using System.Text.RegularExpressions;

namespace NebulaNetwork.Ngrok
{
    public class NgrokManager
    {
        private readonly string _ngrokPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/ngrok-v3-stable-windows-amd64/ngrok.exe";
        private readonly int _port;
        private readonly string _authtoken;
        private readonly string _region;
        private readonly Task<bool> _ngrokAddressObtained = new Task<bool>(() => true);

        private Process _ngrokProcess;
        private string _ngrokAPIAddress;

        public string NgrokAddress;
        public string NgrokLastErrorCode;
        public bool NgrokEnabled = Config.Options.EnableNgrok;

        public NgrokManager(int port, string authtoken = null, string region = null)
        {
            _port = port;
            _authtoken = authtoken ?? Config.Options.NgrokAuthtoken;
            _region = region ?? Config.Options.NgrokRegion;

            if (!NgrokEnabled)
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

                    hasDownloadAndInstallBeenConfirmed = (await Task.WhenAny(downloadAndInstallConfirmation, downloadAndInstallRejection)).Result;
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
                    NebulaModel.Logger.Log.Warn($"Failed to start Ngrok tunnel! LastErrorCode: {NgrokLastErrorCode}");
                    return;
                }

                if (!IsNgrokActive())
                {
                    NebulaModel.Logger.Log.Warn($"Ngrok tunnel has exited prematurely! LastErrorCode: {NgrokLastErrorCode}");
                    return;
                }

            });

        }

        private async Task DownloadAndInstallNgrok()
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

        private bool IsNgrokInstalled()
        {
            return File.Exists(_ngrokPath);
        }

        private bool StartNgrok()
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

            _ngrokProcess.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceivedEventHandler);
            _ngrokProcess.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataReceivedEventHandler);

            var started = _ngrokProcess.Start();
            if (IsNgrokActive())
            {
                // This links the process as a child process by attaching a null debugger thus ensuring that the process is killed when its parent dies.
                new ChildProcessLinker(_ngrokProcess, (exception) =>
                {
                    NebulaModel.Logger.Log.Warn("Failed to link Ngrok process to DSP process as a child! (This might result in a left over ngrok process if the DSP process uncleanly killed)");
                });
            }

            _ngrokProcess.BeginOutputReadLine();
            _ngrokProcess.BeginErrorReadLine();

            return started;
        }

        private void OutputDataReceivedEventHandler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                NebulaModel.Logger.Log.Debug($"Ngrok Stdout: {e.Data}");

                var json = MiniJson.Deserialize(e.Data) as Dictionary<string, object>;
                if (json != null)
                {
                    var lvl = json["lvl"] as string;
                    if (lvl == "info")
                    {
                        var msg = json["msg"] as string;
                        if (msg == "starting web service")
                        {
                            _ngrokAPIAddress = json["addr"] as string;
                        } else if (msg == "started tunnel")
                        {
                            var addr = json["addr"] as string;
                            var url = json["url"] as string;
                            if (
                                (addr == $"//localhost:{_port}" || addr == $"//127.0.0.1:{_port}" || addr == $"//0.0.0.0:{_port}") &&
                                url.StartsWith("tcp://")
                                )
                            {
                                NgrokAddress = url.Replace("tcp://", "");
                                _ngrokAddressObtained.Start();
                            }
                        }
                    }
                }
            }
        }

        private void ErrorDataReceivedEventHandler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                NebulaModel.Logger.Log.Warn($"Ngrok Stderr: {e.Data}");

                var errorCodeMatches = Regex.Matches(e.Data, @"ERR_NGROK_\d+");
                if (errorCodeMatches.Count > 0)
                {
                    NgrokLastErrorCode = errorCodeMatches[errorCodeMatches.Count - 1].Value;
                }
            }
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

        public Task<string> GetNgrokAddressAsync()
        {
            return Task.Run(() =>
            {
                if (!IsNgrokActive())
                {
                    throw new Exception($"Not able to get Ngrok tunnel address because Ngrok is not started (or exited prematurely)! LastErrorCode: {NgrokLastErrorCode}");
                }

                if (!_ngrokAddressObtained.Wait(TimeSpan.FromSeconds(15)))
                {
                    throw new TimeoutException($"Not able to get Ngrok tunnel address because 15s timeout was exceeded! LastErrorCode: {NgrokLastErrorCode}");
                }

                return NgrokAddress;
            });
        }

        public async Task<string> GetTunnelAddressFromAPI()
        {
            if (!IsNgrokActive())
            {
                throw new Exception($"Not able to get Ngrok tunnel address from API because Ngrok is not started (or exited prematurely)! LastErrorCode: {NgrokLastErrorCode}");
            }

            if (_ngrokAPIAddress == null)
            {
                throw new Exception($"Not able to get Ngrok tunnel address because Ngrok API address is not (yet) known!");
            }

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"http://{_ngrokAPIAddress}/api/tunnels");
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
