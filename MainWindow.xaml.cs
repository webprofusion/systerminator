using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Systerminator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SystemManager sysManager = new SystemManager();
        
        public GroupItem SelectedGroup {
            get {
                return sysManager.SelectedGroup;
            }
        }
    
        public MainWindow()
        {
            InitializeComponent();

            //load host groups from HostGroup.xml
            sysManager.LoadHosts();

            //populate group list
            foreach (var g in sysManager.Groups)
            {
                var i = new ComboBoxItem();
                i.Content = g.Title;
                if (lstGroups.Items.Count==0) i.IsSelected = true;
                this.lstGroups.Items.Add(i);
            }
           
        }

        private void button_PingAll(object sender, RoutedEventArgs e)
        {
            sysManager.PerformBatchPing(sysManager.SelectedGroup);
        }

        private void button_ShutdownAll(object sender, RoutedEventArgs e)
        {
            sysManager.PerformBatchShutdown(sysManager.SelectedGroup, txtCommand.Text);
        }

        private void lstGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sysManager.SelectedGroup = GetSelectedGroup();
            this.dataGrid1.ItemsSource = sysManager.SelectedGroup.Hosts;
        }

        private GroupItem GetSelectedGroup()
        {
            string groupName = ((ComboBoxItem)lstGroups.SelectedItem).Content.ToString();
            var g = sysManager.Groups.FirstOrDefault(gr => gr.Title == groupName);
            return g;
        }
    }
}
