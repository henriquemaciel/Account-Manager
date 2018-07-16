using DraconiusGoGUI.AccountScheduler;
using DraconiusGoGUI.DracoManager;
using DraconiusGoGUI.ProxyManager;
using System.Collections.Generic;

namespace DraconiusGoGUI.Models
{
    public class ProgramExportModel
    {
        public List<Manager> Managers { get; set; }
        public ProxyHandler ProxyHandler { get; set; }
        public List<Scheduler> Schedulers { get; set; }
        public bool SPF { get; set; }
        public bool ShowWelcomeMessage { get; set; }
        public bool AutoUpdate { get; set; }
    }
}
