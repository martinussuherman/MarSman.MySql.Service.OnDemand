using System;

namespace MarSman.MySql.Service.OnDemand
{
    public class ServiceOptions
    {
        public const string OptionsName = "MysqlService";

        public string MysqldPath { get; set; }
        public string MysqladminPath { get; set; }
        public string MysqlConfigPath { get; set; }
        public int PortNumber { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
