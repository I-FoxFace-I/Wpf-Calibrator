using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Calibrator.WpfControl.Demo
{
    public partial class App : Application
    {
        public App()
        {
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level =
                System.Diagnostics.SourceLevels.Warning | System.Diagnostics.SourceLevels.Error;
        }
    }

}
