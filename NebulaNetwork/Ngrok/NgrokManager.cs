#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Utils;
using NebulaWorld;

#endregion

namespace NebulaNetwork.Ngrok;

public class NgrokManager
{
    private readonly string _authToken;
    private readonly TaskCompletionSource<bool> _ngrokAddressObtainedSource = new();
    private readonly string _ngrokConfigPath;

    private readonly string _ngrokPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("_ngrokPath null"),
        "ngrok-v3-stable-windows-amd64", "ngrok.exe");

    private readonly int _port;
    private readonly string _region;
    public readonly bool NgrokEnabled = Config.Options.EnableNgrok;
    private string _ngrokAPIAddress;

    private Process _ngrokProcess;

    public string NgrokAddress;
    public string NgrokLastErrorCode;
    public string NgrokLastErrorCodeDesc;
    private static readonly string[] contents = { "version: 2" };

    public NgrokManager(int port, string authToken = null, string region = null)
    {
        _ngrokConfigPath = Path.Combine(Path.GetDirectoryName(_ngrokPath) ?? throw new InvalidOperationException("_ngrokConfigPath null"), "ngrok.yml");
        _port = port;
        _authToken = authToken ?? Config.Options.NgrokAuthtoken;
        _region = region ?? Config.Options.NgrokRegion;

        if (!NgrokEnabled)
        {
            return;
        }

        if (string.IsNullOrEmpty(_authToken))
        {
            Log.WarnInform("Ngrok support was enabled, however no Authtoken was provided".Translate());
            return;
        }

        // Validate the Ngrok region
        string[] availableRegions = { "us", "eu", "au", "ap", "sa", "jp", "in" };
        if (!string.IsNullOrEmpty(_region) && !availableRegions.Any(_region.Contains))
        {
            Log.WarnInform("Unsupported Ngrok region was provided, defaulting to autodetection".Translate());
            _region = null;
        }

        // Start this stuff in it's own thread, as we require async and we dont want to freeze up the GUI when freeze up when Downloading and installing ngrok
        Task.Run(async () =>
        {
            if (!IsNgrokInstalled())
            {
                var downloadAndInstallConfirmationSource = new TaskCompletionSource<bool>();

                UnityDispatchQueue.RunOnMainThread(() =>
                {
                    InGamePopup.ShowWarning(
                        "Ngrok download and installation confirmation".Translate(),
                        "Ngrok support is enabled, however it has not been downloaded and installed yet, do you want to automatically download and install Ngrok?"
                            .Translate(),
                        "Accept".Translate(),
                        "Reject".Translate(),
                        () => downloadAndInstallConfirmationSource.TrySetResult(true),
                        () => downloadAndInstallConfirmationSource.TrySetResult(false)
                    );
                });

                var hasDownloadAndInstallBeenConfirmed = await downloadAndInstallConfirmationSource.Task;
                if (!hasDownloadAndInstallBeenConfirmed)
                {
                    Log.Warn(
                        "Failed to download or install Ngrok, because user rejected Ngrok download and install confirmation!"
                            .Translate());
                    return;
                }

                try
                {
                    await DownloadAndInstallNgrok();
                    if (!IsNgrokInstalled())
                    {
                        throw new FileNotFoundException();
                    }
                }
                catch
                {
                    Log.WarnInform("Failed to download or install Ngrok!".Translate());
                    throw;
                }
            }

            if (!StartNgrok())
            {
                Log.WarnInform(
                    string.Format("Failed to start Ngrok tunnel! LastErrorCode: {0} {1}".Translate(), NgrokLastErrorCode, NgrokLastErrorCodeDesc));
                return;
            }

            if (!IsNgrokActive())
            {
                Log.WarnInform(string.Format("Ngrok tunnel has exited prematurely! LastErrorCode: {0} {1}".Translate(),
                    NgrokLastErrorCode, NgrokLastErrorCodeDesc));
            }
        });
    }

    private async Task DownloadAndInstallNgrok()
    {
        using (var client = new HttpClient())
        {
            using var s = client.GetStreamAsync("https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-windows-amd64.zip");
            using var zip = new ZipArchive(await s, ZipArchiveMode.Read);
            if (File.Exists(_ngrokPath))
            {
                File.Delete(_ngrokPath);
            }
            zip.ExtractToDirectory(Path.GetDirectoryName(_ngrokPath));
        }

        File.WriteAllLines(_ngrokConfigPath, contents);

        if (File.Exists(_ngrokPath))
        {
            Log.WarnInform("Ngrok install completed in the plugin folder".Translate());
        }
        else
        {
            Log.Error("Ngrok installation failed".Translate());
        }
    }

    private bool IsNgrokInstalled()
    {
        return File.Exists(_ngrokPath) && File.Exists(_ngrokConfigPath);
    }

    private bool StartNgrok()
    {
        StopNgrok();

        _ngrokProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = _ngrokPath,
                Arguments =
                $"tcp {_port} --authtoken {_authToken} --log stdout --log-format json --config \"{_ngrokConfigPath}\"" +
                (!string.IsNullOrEmpty(_region) ? $" --region {_region}" : "")
            }
        };

        _ngrokProcess.OutputDataReceived += OutputDataReceivedEventHandler;
        _ngrokProcess.ErrorDataReceived += ErrorDataReceivedEventHandler;
        _ngrokProcess.Exited += (_, _) =>
        {
            StopNgrok();
        };

        var started = _ngrokProcess.Start();
        if (IsNgrokActive())
        {
            // This links the process as a child process by attaching a null debugger thus ensuring that the process is killed when its parent dies.
            _ = new ChildProcessLinker(_ngrokProcess, _ =>
            {
                Log.WarnInform(
                    "Failed to link Ngrok process to DSP process as a child! (This might result in a left over ngrok process if the DSP process uncleanly killed)");
            });
        }
        else
        {
            Log.WarnInform("Failed to start Ngrok process!");
        }

        _ngrokProcess?.BeginOutputReadLine();
        _ngrokProcess?.BeginErrorReadLine();

        return started;
    }

    private void OutputDataReceivedEventHandler(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data))
        {
            return;
        }
        Log.Debug($"Ngrok Stdout: {e.Data}");

        if (MiniJson.Deserialize(e.Data) is not Dictionary<string, object> json)
        {
            return;
        }
        var lvl = json["lvl"] as string;
        if (lvl != "info")
        {
            return;
        }
        var msg = json["msg"] as string;
        switch (msg)
        {
            case "starting web service":
                _ngrokAPIAddress = json["addr"] as string;
                break;
            case "started tunnel":
                {
                    var addr = json["addr"] as string;
                    if (
                        json["url"] is string url &&
                        (addr == $"//localhost:{_port}" || addr == $"//127.0.0.1:{_port}" ||
                         addr == $"//0.0.0.0:{_port}") &&
                        url.StartsWith("tcp://")
                    )
                    {
                        NgrokAddress = url.Replace("tcp://", "");
                        _ngrokAddressObtainedSource.TrySetResult(true);
                    }
                    break;
                }
        }
    }

    private void ErrorDataReceivedEventHandler(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data))
        {
            return;
        }
        Log.Warn($"Ngrok Stderr: {e.Data}");

        var errorCodeMatches = Regex.Matches(e.Data, @"ERR_NGROK_\d+");
        if (errorCodeMatches.Count <= 0)
        {
            return;
        }
        NgrokLastErrorCode = errorCodeMatches[errorCodeMatches.Count - 1].Value;
        NgrokLastErrorCodeDesc = NgrokLastErrorCode switch
        {
            "ERR_NGROK_105" => "Authtoken is empty or expired".Translate(),
            "ERR_NGROK_108" => "Session limit reached".Translate(),
            "ERR_NGROK_123" => "Account email not verified".Translate(),
            "ERR_NGROK_8013" => "Account requires a debit/credit card".Translate(),
            _ => string.Empty
        };
        NgrokLastErrorCodeDesc = !string.IsNullOrWhiteSpace(NgrokLastErrorCodeDesc) ? $"({NgrokLastErrorCodeDesc})" : string.Empty;
        Log.WarnInform(string.Format("Ngrok Error! Code: {0} {1}".Translate(), NgrokLastErrorCode, NgrokLastErrorCodeDesc));
    }

    public void StopNgrok()
    {
        if (_ngrokProcess == null)
        {
            return;
        }
        _ngrokProcess.Refresh();
        try
        {
            if (!_ngrokProcess.HasExited)
            {
                _ngrokProcess.Kill();
            }
        }
        catch (Exception e)
        {
            Log.WarnInform(e.ToString());
        }
        _ngrokProcess.Close();
        _ngrokProcess = null;
    }

    public bool IsNgrokActive()
    {
        try
        {
            _ngrokProcess?.Refresh();
            return !_ngrokProcess?.HasExited ?? false;
        }
        catch (Exception e)
        {
            Log.WarnInform(e.ToString());
            return false;
        }
    }

    public Task<string> GetNgrokAddressAsync()
    {
        return Task.Run(() =>
        {
            if (!_ngrokAddressObtainedSource.Task.Wait(TimeSpan.FromSeconds(15)))
            {
                throw new TimeoutException(
                    $"Not able to get Ngrok tunnel address because 15s timeout was exceeded! LastErrorCode: {NgrokLastErrorCode} {NgrokLastErrorCodeDesc}");
            }

            return NgrokAddress;
        });
    }

    public async Task<string> GetTunnelAddressFromAPI()
    {
        if (!IsNgrokActive())
        {
            throw new Exception(
                $"Not able to get Ngrok tunnel address from API because Ngrok is not started (or exited prematurely)! LastErrorCode: {NgrokLastErrorCode} {NgrokLastErrorCodeDesc}");
        }

        if (_ngrokAPIAddress == null)
        {
            throw new Exception("Not able to get Ngrok tunnel address because Ngrok API address is not (yet) known!");
        }

        using var client = new HttpClient();
        var response = await client.GetAsync($"http://{_ngrokAPIAddress}/api/tunnels");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Could not access the ngrok API");
        }
        var body = await response.Content.ReadAsStringAsync();

        if (MiniJson.Deserialize(body) is not Dictionary<string, object> json)
        {
            throw new Exception(
                "Not able to get Ngrok tunnel address because response contained invalid json");
        }
        var tunnels = json["tunnels"] as List<object>;

        var publicUrl =
            (from Dictionary<string, object> tunnel in tunnels
             where tunnel["proto"] as string == "tcp" &&
                      (tunnel["config"] as Dictionary<string, object>)?["addr"] as string == $"localhost:{_port}"
             select tunnel["public_url"] as string).FirstOrDefault();

        return publicUrl == null
            ? throw new Exception(
                "Not able to get Ngrok tunnel address because no matching tunnel was found in API response")
            : publicUrl.Replace("tcp://", "");
    }
}
