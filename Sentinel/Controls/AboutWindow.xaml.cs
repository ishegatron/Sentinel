using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Sentinel.Controls
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow(Window parent)
        {
            InitializeComponent();

            Owner = parent;            

            var assembly = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);

            AssemblyNameLabel.Text = assembly.ProductName;
            VersionNumberLabel.Text = assembly.ProductVersion;
            DescriptionLabel.Text = assembly.Comments;
            DeveloperInfoLabel.Text = assembly.CompanyName;
            CopyrightInfoLabel.Text = assembly.LegalCopyright;
        }
    }
}
