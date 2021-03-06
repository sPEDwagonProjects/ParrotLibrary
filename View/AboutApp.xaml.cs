using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ParrotLibrary.View
{
    /// <summary>
    /// Interaction logic for AboutApp.xaml
    /// </summary>
    public partial class AboutApp : Page
    {
        public AboutApp()
        {
            InitializeComponent();
        }

        private void githubLink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)=>
            System.Diagnostics.Process.Start("https://github.com/OneCellDM/ParrotLibrary");

		private void SpedWagonLink_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)=>
            System.Diagnostics.Process.Start("https://spedwagon.online/");
      
	}
}
