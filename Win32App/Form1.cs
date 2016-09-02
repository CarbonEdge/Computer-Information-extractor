using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Data.Sql;
using System.Data.SqlClient;
using Microsoft.Win32;

namespace Win32App
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();// Create save the CSV
                sfd.Filter = "Text File|*.txt";// filters for text files only
                sfd.FileName = "CPU usage.txt";
                sfd.Title = "Save Text File";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string path = sfd.FileName;
                    using (StreamWriter bw = new StreamWriter(File.Create(path)))
                    {
                        bw.WriteLine("Machine Name; {0} ", Environment.MachineName);
                        foreach (var item in new ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
                        {
                            bw.WriteLine("Current user name; {0}", item["UserName"].ToString().ToLower());
                            bw.WriteLine("Number Of Physical Processors; {0} ", item["NumberOfProcessors"]);
                        }

                        //Console.WriteLine("The number of processors " + "on this computer is {0}.", Environment.ProcessorCount);
                        int coreCount = 0;

                        foreach (var item in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
                        {
                            coreCount += int.Parse(item["NumberOfCores"].ToString());
                        }

                        bw.WriteLine("Number Of Cores; {0}", coreCount);
                        //The below environment class sometime gives different information. 
                        //Console.WriteLine("Number Of Logical Processors: {0}", Environment.ProcessorCount);

                        foreach (var item in new ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
                        {
                            bw.WriteLine("Number Of Logical Processors; {0}", item["NumberOfLogicalProcessors"]);
                        }

                        var wmi = new ManagementObjectSearcher("select * from Win32_OperatingSystem").Get().Cast<ManagementObject>().First();
                        string OpSystem = ((string)wmi["Caption"]).Trim();

                        bw.WriteLine("operating system; "+OpSystem);

                        var cpu = new ManagementObjectSearcher("select * from Win32_Processor").Get().Cast<ManagementObject>().First();

                        string Chip = (string)cpu["Name"];
                        bw.WriteLine("Processor manufacturer; "+Chip);

                        bw.Flush();
                        bw.Close();
                    }
                    MessageBox.Show("Data Saved");
                }
                else
                {
                    MessageBox.Show("No option selected");
                }
            }//end try
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)//SQL
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();// Create save the CSV
                sfd.Filter = "Text File|*.txt";// filters for text files only
                sfd.FileName = "SQL information.txt";
                sfd.Title = "Save Text File";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string path = sfd.FileName;
                    using (StreamWriter bw = new StreamWriter(File.Create(path)))
                    {
                        SqlDataSourceEnumerator sqldatasourceenumerator1 = SqlDataSourceEnumerator.Instance;
                        using (DataTable datatable1 = sqldatasourceenumerator1.GetDataSources())
                        {
                            foreach (DataRow row in datatable1.Rows)
                            {
                                bw.WriteLine("****************************************");
                                bw.WriteLine("Server Name:" + row["ServerName"]);
                                bw.WriteLine("Instance Name:" + row["InstanceName"]);
                                bw.WriteLine("Is Clustered:" + row["IsClustered"]);
                                bw.WriteLine("Version:" + row["Version"]);
                                bw.WriteLine("****************************************");
                            }
                        }                            
                        //using win32
                        var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                        var msSQLServer = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server");
                        var instances = (string[])msSQLServer.GetValue("InstalledInstances");
                        bw.WriteLine("This Machine Name; {0} ", Environment.MachineName);
                        if (instances!=null)
                        {
                            foreach (var instance in instances)
                            {
                                var insNames = localMachine.OpenSubKey(@"Software\Microsoft\Microsoft SQL Server\Instance Names\SQL");
                                var realNameInstanse = (string)insNames.GetValue(instance);
                                var sqlEditionRegistry = localMachine.OpenSubKey(string.Format(@"Software\Microsoft\Microsoft SQL Server\{0}\Setup", realNameInstanse));
                                var edition = (string)sqlEditionRegistry.GetValue("Edition");
                                bw.WriteLine("Instance {0}, RealName {2}, - Edition: {1}", instance, edition, realNameInstanse);
                            }
                        }
                        else
                        {
                            bw.WriteLine("No instances found on this PC");
                        }
                        
                        bw.Flush();
                        bw.Close();
                    }
                    MessageBox.Show("Data Saved");
                }
                else
                {
                    MessageBox.Show("No option selected");
                }
            }//end try
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
