using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace GUI_Disabler_netF
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Macbook Pro GPU Disabler\nVersion 1.0 (by Annabel Sandford)");
            getGPU();

            //Console.ReadKey();
        }

        static void rebootMe()
        {
            //MessageBox.Show("Test");
            DialogResult dialogResult = MessageBox.Show("Do you want to reboot?", "GPU Disabler", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
            }
            else if (dialogResult == DialogResult.No)
            {
                Environment.Exit(0);
            }
        }

        static void getGPU()
        {
            List<string> gpus = new List<string>();
            List<string> ps1s = new List<string>();
            int gpu_count = 0;

            Console.WriteLine("GPU CHECK: ============================");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (ManagementObject mo in searcher.Get())
            {
                PropertyData currentBitsPerPixel = mo.Properties["CurrentBitsPerPixel"];
                PropertyData description = mo.Properties["PNPDeviceID"];
                PropertyData gpu_name = mo.Properties["Description"];
                if (currentBitsPerPixel != null && description != null)
                {
                    if (currentBitsPerPixel.Value != null)
                        System.Console.WriteLine(gpu_name.Value + " / " + description.Value);
                    gpus.Add(description.Value.ToString());
                    gpu_count++;
                }
            }
            Console.WriteLine("Total GPUs detected: " + gpu_count.ToString());
            Console.WriteLine("=======================================\n");
            Console.WriteLine("DISABLING GPU(s): =====================");

            if (gpu_count > 0)
            {
                foreach (var gpu in gpus)
                {
                    Console.WriteLine("Disabling " + gpu.ToString() + "...");

                    using (PowerShell PowerShellInst = PowerShell.Create())
                    {

                        PowerShell ps = PowerShell.Create();

                        string random_string = Guid.NewGuid().ToString("n").Substring(0, 8);
                        string param1 = "pnputil /disable-device \"" + gpu.ToString() + "\"";
                        string scriptPath = Path.GetTempPath() + random_string + ".ps1";
                        File.WriteAllText(scriptPath, param1);
                        //Process.Start("notepad.exe", scriptPath);
                        ps.AddScript(File.ReadAllText(scriptPath));
                        //ps.AddArgument(param1);
                        ps.Invoke();
                        ps1s.Add(scriptPath);
                    }

                    foreach (var ps1 in ps1s)
                    {
                        Console.WriteLine("Clearing " + ps1.ToString() + "....");
                        try
                        { 
                            File.Delete(ps1);
                            Console.WriteLine("Done!");
                        }
                        catch (Exception A)
                        {
                            Console.WriteLine("Failed.");
                        }
                    }
                    rebootMe();
                }
            }
            else
            {
                Console.WriteLine("Could not detect any GPU's. Quitting...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Console.WriteLine("=======================================\n");
        }
    }
}
