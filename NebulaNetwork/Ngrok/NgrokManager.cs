using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NebulaNetwork.Ngrok
{
    public class NgrokManager
    {
        private readonly string _ngrokPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/ngrok-v3-stable-windows-amd64/ngrok.exe";

        private Process _ngrokProcess;
        private int _port;

        public NgrokManager()
        {
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

        public bool StartNgrok(int port, string authToken)
        {
            _port = port;

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
                Arguments = $"tcp {_port} --authtoken {authToken}"
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

                    var json = JObject.Parse(body);

                    var matchingTunnels =
                        from tunnel in json["tunnels"]
                        where tunnel["proto"].ToString() == "tcp" && tunnel["config"]["addr"].ToString() == $"localhost:{_port}"
                        select tunnel;

                    var publicUrl = matchingTunnels.ToList()[0]["public_url"].ToString();

                    return publicUrl.Replace("tcp://", "");

                } else
                {
                    throw new Exception("Could not access the ngrok API");
                }
            }
        }
    }
}
