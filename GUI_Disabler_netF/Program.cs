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
using System.Threading;
using static System.Environment;
using IWshRuntimeLibrary;

namespace GUI_Disabler_netF
{
    static class Globals
    {
        public static bool HAL_was_disabled = false;
        public static bool GPUs_were_disabled = false;
        public static bool sound_fix_enabled = false;

        public static string application_dir = AppDomain.CurrentDomain.BaseDirectory.ToString();
    }

    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Macbook Pro GPU Disabler\nVersion 1.2 - Written by Annabel Sandford");
            Console.WriteLine("=======================================");
            checkInstaller();
            Console.ReadKey();
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
                DialogResult dialogResult = MessageBox.Show("All GPU's had been disabled\nWe need to reboot to apply the changes.\n\nDo you want to reboot? (recommended)", "REBOOT - GPU Disabler", MessageBoxButtons.YesNo);
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

        public static void SoundFix()
        {
            // Sound Fix work in progress
        }

        public static void checkInstaller()
        {
            bool can_continue = true;
            bool error_occured = false;
            bool exe_found = false;
            string exe_found_str = "";
            string home_path = "C:\\GPU_Disabler";
            string exception_filled = "";
            string status_filled = "";

            while (can_continue == true)
            {
                if (!Directory.Exists(home_path))
                {
                    // Create installation directory if it doesnt exist
                    Console.WriteLine(home_path + " does not exist. Creating...");
                    try
                    {
                        Directory.CreateDirectory(home_path);
                        Console.WriteLine("Directory created.");
                    }
                    catch (Exception hp)
                    {
                        error_occured = true;
                        exception_filled = hp.ToString();
                        break;
                    }

                    // Move files to installation directory
                    try
                    {
                        string[] fileEntries = Directory.GetFiles(Globals.application_dir);
                        foreach (string fileName in fileEntries)
                        {
                            string just_name = Path.GetFileName(fileName);
                            string file_extension = Path.GetExtension(fileName);
                            Console.WriteLine("Copying: " + just_name + " (" + file_extension + ")");
                            System.IO.File.Move(fileName, home_path + "\\" + just_name);
                            Console.WriteLine("Copied.");

                            if (file_extension == ".exe" && exe_found == false)
                            {
                                exe_found = true;
                                exe_found_str = just_name;
                                Console.WriteLine("Executable found. ^");
                            }
                        }
                    }
                    catch (Exception hr)
                    {
                        error_occured = true;
                        exception_filled = hr.ToString();
                        break;
                    }

                    // Delete shortcut @ startup if it exists
                    try
                    {
                        if (System.IO.File.Exists(Environment.GetFolderPath(SpecialFolder.CommonStartup) + "\\" + exe_found_str + ".lnk"))
                        {
                            Console.WriteLine("Old shortcut detected @ " + Environment.GetFolderPath(SpecialFolder.CommonStartup) + "\\" + exe_found_str + ".lnk");
                            System.IO.File.Delete(Environment.GetFolderPath(SpecialFolder.CommonStartup) + "\\" + exe_found_str + ".lnk");
                            Console.WriteLine("Deleted.");
                        }
                    }
                    catch (Exception dings)
                    {
                        Console.WriteLine(dings.ToString());
                    }

                    // Create shortcut to auto startup
                    try
                    {
                        string system_auto = Environment.GetFolderPath(SpecialFolder.CommonStartup) + "\\";

                        string link = system_auto + exe_found_str + ".lnk";
                        var shell = new WshShell();
                        var shortcut = shell.CreateShortcut(link) as IWshShortcut;
                        shortcut.TargetPath = home_path + "\\" + exe_found_str;
                        shortcut.WorkingDirectory = Application.StartupPath;
                        shortcut.Save();

                        status_filled = "GPU_Disabler installed + added to startup";
                    }
                    catch (Exception aut)
                    {
                        error_occured = true;
                        exception_filled = aut.ToString();
                        break;
                    }
                }
                else
                {
                    try
                    {
                        string installed_exe_name = "";
                        string[] fileEntriesC = Directory.GetFiles(home_path);
                        foreach (string fileNamesC in fileEntriesC)
                        {
                            string just_name_c = Path.GetFileName(fileNamesC);
                            string file_extension_c = Path.GetExtension(fileNamesC);
                            bool exe_found_c = false;

                            if (file_extension_c == ".exe" && exe_found_c == false)
                            {
                                exe_found_c = true;
                                installed_exe_name = just_name_c;
                                Console.WriteLine("Installed executable found @ " + fileNamesC);
                            }
                        }
                        if (System.IO.File.Exists(Environment.GetFolderPath(SpecialFolder.CommonStartup) + "\\" + installed_exe_name + ".lnk"))
                        {
                            status_filled = "GPU_Disabler already installed + startup exists";
                        }
                        else
                        {
                            status_filled = "GPU_Disabler already installed + startup broken";
                        }
                    }
                    catch (Exception no)
                    {
                        error_occured = true;
                        exception_filled = no.ToString();
                        break;
                    }
                }
                can_continue = false;
                break;
            }
            if (error_occured == true)
            {
                Console.WriteLine("Error occured: " + exception_filled);
            }
            else
            {
                Console.WriteLine(status_filled);
            }
            Console.WriteLine("=======================================\n");
            checkMacHAL();
        }

        public static void checkMacHAL()
        {
            string HAL_path = "C:\\Windows\\System32\\drivers\\MacHALDriver.sys";
            string replace_HAL_path = "C:\\Windows\\System32\\drivers\\disabled_MacHALDriver.sys";
            if (System.IO.File.Exists(HAL_path))
            {
                Console.WriteLine("MacHALDriver.sys detected at " + HAL_path);
                try
                {
                    if (System.IO.File.Exists(replace_HAL_path))
                    {
                        System.IO.File.Delete(replace_HAL_path);
                    }
                    System.IO.File.Move(HAL_path, replace_HAL_path);
                    Console.WriteLine("MacHALDriver.sys moved to " + replace_HAL_path);
                    Globals.HAL_was_disabled = true;
                }
                catch (Exception b)
                {
                    Console.WriteLine("Could not move MacHALDriver.sys :( \n" + b.ToString());
                }
            }
            else if (System.IO.File.Exists(replace_HAL_path))
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
            int turned_off_gpus = 0;

            Console.WriteLine("\nGPU CHECK: ============================");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (ManagementObject mo in searcher.Get())
            {
                PropertyData currentBitsPerPixel = mo.Properties["CurrentBitsPerPixel"];
                PropertyData description = mo.Properties["PNPDeviceID"];
                PropertyData gpu_name = mo.Properties["Description"];
                PropertyData status = mo.Properties["Status"];
                if (currentBitsPerPixel != null && description != null)
                {
                    if (currentBitsPerPixel.Value != null)
                    {
                        Console.WriteLine(gpu_name.Value + " @ " + description.Value + " (" + status.Value + ")");
                        gpus.Add(description.Value.ToString());
                        gpu_count++;
                        if (status.Value.ToString() == "Error")
                        {
                            turned_off_gpus++;
                        }
                    }
                }
            }
            if (turned_off_gpus == gpu_count)
            {
                Globals.GPUs_were_disabled = true;
            }
            Console.WriteLine("Total GPUs detected: " + gpu_count.ToString());
            Console.WriteLine("=======================================\n");
            Console.WriteLine("DISABLING GPU(s): =====================");

            if (gpu_count > 0 && Globals.GPUs_were_disabled == false)
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
                        System.IO.File.WriteAllText(scriptPath, param1);
                        //Process.Start("notepad.exe", scriptPath);
                        ps.AddScript(System.IO.File.ReadAllText(scriptPath));
                        //ps.AddArgument(param1);
                        ps.Invoke();
                        ps1s.Add(scriptPath);
                    }

                    foreach (var ps1 in ps1s)
                    {
                        Console.WriteLine("Clearing " + ps1.ToString() + "....");
                        try
                        { 
                            System.IO.File.Delete(ps1);
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
            else if (Globals.GPUs_were_disabled == true)
            {
                Console.WriteLine("All GPU's already disabled.");
                Thread.Sleep(2000);
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Could not detect any GPU's. Quitting...");
                Thread.Sleep(2000);
                Environment.Exit(0);
            }

            Console.WriteLine("=======================================\n");
        }
    }
}
