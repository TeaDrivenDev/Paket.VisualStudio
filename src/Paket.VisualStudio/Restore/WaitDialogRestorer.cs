using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Shell.Interop;

namespace Paket.VisualStudio.Restore
{
    public class WaitDialogRestorer : IPackageRestorer
    {
        private readonly IPackageRestorer restorer;
        private readonly IVsThreadedWaitDialogFactory waitDialogFactory;

        public WaitDialogRestorer(IPackageRestorer restorer, IVsThreadedWaitDialogFactory waitDialogFactory)
        {
            if (restorer == null)
                throw new ArgumentNullException("restorer");
            if (waitDialogFactory == null)
                throw new ArgumentNullException("waitDialogFactory");

            this.restorer = restorer;
            this.waitDialogFactory = waitDialogFactory;
        }

        public void Restore(Dependencies dependencies, IEnumerable<RestoringProject> projects)
        {
            var projectsList = projects.ToList();
            IVsThreadedWaitDialog2 waitDialog;
            waitDialogFactory.CreateInstance(out waitDialog);
            waitDialog.StartWaitDialog("Paket", "Restoring packages", null, null, null, 0, false, true);

            int i = 0;
            try
            {
                foreach (var project in projectsList)
                {
                    bool canceled;
                    waitDialog.UpdateProgress(string.Format("Restoring packages for {0}", project.ProjectName), null, null, i++, projectsList.Count, false, out canceled);

                    restorer.Restore(dependencies, new[] { project });
                }
            }
            finally
            {
                waitDialog.EndWaitDialog(out i);
            }
        }
    }
}