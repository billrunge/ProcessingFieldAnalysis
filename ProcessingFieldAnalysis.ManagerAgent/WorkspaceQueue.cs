using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class WorkspaceQueue
    {
        IHelper Helper { get; set; }
        IAPILog Logger { get; set; }

        public WorkspaceQueue(IHelper helper, IAPILog logger)
        {
            Helper = helper;
            Logger = logger;
        }





    }
}
