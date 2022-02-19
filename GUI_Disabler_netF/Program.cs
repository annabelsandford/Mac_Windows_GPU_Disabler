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
    static class Globals
    {
        public static bool HAL_was_disabled = false;
    }

    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Macbook Pro GPU Disabler\nVersion 1.1 - Written by Annabel Sandford");
            Console.WriteLine("=======================================");
            checkMacHAL();
        }

        static void rebootMe()
        {
            if (Globals.HAL_was_disabled == true)
            {
                DialogResult dialogResult = MessageBox.Show("We have detected and disabled drivers (MacHALDriver.sys) which can cause this computer to crash and/or be unstable.\nWe need to reboot to apply the changes.\n\nDo you want to reboot? (Highly recommended)", "REBOOT - GPU Disabler", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
                }
                else if (dialogResult == DialogResult.No)
                {
                    Environment.Exit(0);
                }
            }
            else
            {
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
        }

        public static void checkMacHAL()
        {
            string HAL_path = "C:\\Windows\\System32\\drivers\\MacHALDriver.sys";
            string replace_HAL_path = "C:\\Windows\\System32\\drivers\\disabled_MacHALDriver.sys";
            if (File.Exists(HAL_path))
            {
                Console.WriteLine("MacHALDriver.sys detected at " + HAL_path);
                try
                {
                    if (File.Exists(replace_HAL_path))
                    {
                        File.Delete(replace_HAL_path);
                    }
                    File.Move(HAL_path, replace_HAL_path);
                    Console.WriteLine("MacHALDriver.sys moved to " + replace_HAL_path);
                    Globals.HAL_was_disabled = true;
                }
                catch (Exception b)
                {
                    Console.WriteLine("Could not move MacHALDriver.sys :( \n" + b.ToString());
                }
            }
            else if (File.Exists(replace_HAL_path))
            {
                Console.WriteLine("Disabled MacHALDriver.sys detected at " + replace_HAL_path);
            }
            else
            {
                Console.WriteLine("MacHALDriver.sys not found.");
            }
            getGPU();
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
