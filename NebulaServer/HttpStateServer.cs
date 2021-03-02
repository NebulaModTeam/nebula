using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace NebulaServer
{
    public class HttpStateServer
    {
        private IWebHost webHost;

        public HttpStateServer(string bindAddressAndPort, string saveFilePath)
        {
            webHost = WebHost.CreateDefaultBuilder()
                .Configure(config => config.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.GetDirectoryName(saveFilePath)),
                    RequestPath = new Microsoft.AspNetCore.Http.PathString("/initialstate"),
                    ServeUnknownFileTypes = true
                }))
                .UseUrls(bindAddressAndPort)
                .Build();
        }

        public Task Start()
        {
            return Task.Factory.StartNew(() =>
            {
                webHost.Run();
            });
        }

        public void Stop()
        {
            webHost.StopAsync().ContinueWith((ctx) =>
            {
                webHost.Dispose();
                webHost = null;
            });
        }
    }
}
