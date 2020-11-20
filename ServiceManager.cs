using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MarSman.MySql.Service.OnDemand
{
    public class ServiceManager : IHostedService
    {
        public ServiceManager(
            IOptions<ServiceOptions> options,
            ILogger<ServiceManager> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_process != null)
            {
                return;
            }

            _logger.LogInformation("Starting mysqld on demand.");
            _process = new Process();
            _process.StartInfo.FileName = _options.Value.MysqldPath;
            _process.StartInfo.Arguments = $"--defaults-file={_options.Value.MysqlConfigPath}";
            _process.StartInfo.UseShellExecute = false;
            await Task.Run(_process.Start);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_process == null)
            {
                _logger.LogWarning("mysqld process not running.");
                return;
            }

            Process admin = new Process();
            admin.StartInfo.FileName = _options.Value.MysqladminPath;
            admin.StartInfo.Arguments = $"-P {_options.Value.PortNumber} -u {_options.Value.User} -p{_options.Value.Password} shutdown";
            admin.StartInfo.UseShellExecute = false;
            await Task.Run(admin.Start);

            if (admin.WaitForExit(1000 * 30))
            {
                if (_process.WaitForExit(1000 * 30))
                {
                    _process.Close();
                    _process = null;
                    _logger.LogInformation("mysqld stopped gracefully.");
                    return;
                }

                _logger.LogError("Failed to stop mysqld gracefully.");
                return;
            }

            _logger.LogError("mysqladmin problem.");
        }

        private Process _process;
        private readonly IOptions<ServiceOptions> _options;
        private readonly ILogger<ServiceManager> _logger;
    }
}