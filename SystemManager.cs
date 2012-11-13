using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using System.ComponentModel;

namespace Systerminator
{
    public class GroupItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Title {get;set;}
        public List<HostDetails> Hosts {get;set;}
        public bool IsSelected { get; set; }
    }

    public class HostDetails : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string HostName { get; set; }
        public string Status { get; set; }
        public DateTime? LastPinged { get;set;}
        public bool AllowShutdown { get; set; }
    }

    public class SystemManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<GroupItem> Groups { get; set; }

        public GroupItem SelectedGroup
        {
            get {
                return Groups.FirstOrDefault(g => g.IsSelected == true);
            }
            set
            {
                foreach (GroupItem g in Groups)
                {
                    if (g == value) g.IsSelected = true;
                    else g.IsSelected = false;
                }
            }
        }

        public void LoadHosts()
        {
       
            Groups = new List<GroupItem>();

            XDocument xdoc = XDocument.Load("HostGroup.xml");
            var groups = xdoc.Elements("groups");
            foreach(XElement g in groups.Elements("group"))
            {
                GroupItem grp = new GroupItem();
                grp.Title = g.Attribute("title").Value;
                grp.Hosts = new List<HostDetails>();

                var items = g.Elements("item");

                foreach (var val in items)
                {
                    HostDetails host = new HostDetails();
                    host.HostName = val.Value;
                    host.AllowShutdown = val.Attribute("allowshutdown") != null ? bool.Parse(val.Attribute("allowshutdown").Value) : true;
                    grp.Hosts.Add(host);
                }
                Groups.Add(grp);
            }   
        }

        public void PerformBatchPing(GroupItem group)
        {
            foreach (var host in group.Hosts)
            {
                Ping p = new Ping();
                p.PingCompleted += new PingCompletedEventHandler(PerformPingCompleted);
                p.SendAsync(host.HostName, host);                
            }
        }

        
        void PerformPingCompleted(object sender, PingCompletedEventArgs e)
        {
            var reply = e.Reply;
            HostDetails host = (HostDetails)e.UserState;
            string statusText = "No Response";
            if (reply != null)
            {
                statusText = reply.Status.ToString();
            }
            
            foreach (var group in Groups)
            {
                if (group.Hosts.Contains(host))
                {
                    foreach (var h in group.Hosts.Where(ht => ht == host))
                    {
                        h.Status = statusText;
                        h.LastPinged = DateTime.Now;
                    }
                }
            }
        }
        
        public void PerformSystemCommand(string command)
        {
            System.Diagnostics.Debug.WriteLine(command);
            ExecuteCommandSync(command);
        }

        public void ExecuteCommandSync(object command)
        {

            try
            {

                // create the ProcessStartInfo using "cmd" as the program to be run, and "/c " as the parameters.

                // Incidentally, /c tells cmd that we want it to execute the command that follows, and then exit.

                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

                // The following commands are needed to redirect the standard output.

                //This means that it will be redirected to the Process.StandardOutput StreamReader.

                procStartInfo.RedirectStandardOutput = true;

                procStartInfo.UseShellExecute = false;

                // Do not create the black window.

                procStartInfo.CreateNoWindow = true;

                // Now we create a process, assign its ProcessStartInfo and start it

                System.Diagnostics.Process proc = new System.Diagnostics.Process();

                proc.StartInfo = procStartInfo;

                proc.Start();



                // Get the output into a string

                string result = proc.StandardOutput.ReadToEnd();



                // Display the command output.

                Console.WriteLine(result);

            }

            catch (Exception)
            {

                // Log the exception

            }

        }


        public void PerformBatchShutdown(GroupItem g, string CommandText)
        {
            foreach (var host in g.Hosts)
            {
                if (host.AllowShutdown)
                {
                    string command = CommandText.Replace("{MachineName}", host.HostName);
                    PerformSystemCommand(command);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Shutdown not permitted for host:"+host.HostName);
                }
            }
        }
    }
}
