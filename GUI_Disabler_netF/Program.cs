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
using System.Net;
using System.IO.Compression;

namespace GUI_Disabler_netF
{
    static class Globals
    {
        public static bool HAL_was_disabled = false;
        public static bool GPUs_were_disabled = false;
        public static bool sound_fix_enabled = false;

        public static bool sound_fix_stage_1 = false;
        public static bool sound_fix_stage_2 = false;
        public static bool sound_fix_stage_3 = false;
        public static bool sound_fix_stage_4 = false;

        public static string application_dir = AppDomain.CurrentDomain.BaseDirectory.ToString();
    }

    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Macbook Pro GPU Disabler\nVersion 1.3 - Written by Annabel Sandford");
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

        public static void SoundFixCheck()
        {
            // Kontrollera om soundfix ska köras eller inte

            string home_path = "C:\\GPU_Disabler";
            string sound_config = home_path + "\\sound.config";
            string error_occured_string = "";

            bool ask_for_soundfix = false;
            bool do_soundfix = false;
            bool error_occured = false;

            if (System.IO.File.Exists(sound_config))
            {
                string sound_config_content = System.IO.File.ReadAllText(sound_config);

                if (sound_config_content == "true")
                {
                    ask_for_soundfix = false;
                    do_soundfix = true;
                }
                else if (sound_config_content == "false")
                {
                    ask_for_soundfix = false;
                }
                else if (sound_config_content == "ask")
                {
                    ask_for_soundfix = true;
                }
                else if (sound_config_content == "magic")
                {
                    string[] bops = { "https://www.youtube.com/watch?v=AYBTfIALz20", "https://www.youtube.com/watch?v=ldV0F5rfMog", "https://www.youtube.com/watch?v=Jv1isVWZKjI" };
                    Random random = new Random();
                    Process.Start(bops[random.Next(bops.Length)]);
                    Environment.Exit(0);
                }
                else
                {
                    // Soundfix-filen finns men innehåller skadad information
                    try
                    {
                        System.IO.File.Delete(sound_config);
                        ask_for_soundfix = true;
                    }
                    catch (Exception sff)
                    {
                        error_occured = true;
                        error_occured_string = sff.ToString();
                    }
                }
            }
            else
            {
                // Ingen konfigurationsfil finns, fråga om användaren vill distribuera korrigeringen eller inte.
                try
                {
                    using (StreamWriter sw = System.IO.File.CreateText(sound_config))
                    {
                        sw.Write("ask");
                    }
                    ask_for_soundfix = true;
                }
                catch (Exception sffa)
                {
                    error_occured = true;
                    error_occured_string = sffa.ToString();
                    ask_for_soundfix = false;
                    do_soundfix = false;
                }
            }   

            if (error_occured == true)
            {
                Console.WriteLine(error_occured_string);
            }

            if (ask_for_soundfix == true)
            {
                DialogResult dialogResult = MessageBox.Show("Do you want to fix the audio for this computer? We will not ask again.\n\n(In case you change your mind you can delete the sound.config file under C:\\GPU_Disabler)", "Sound Fix - GPU Disabler", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    SoundFix_StageInterpreter();
                }
                else if (dialogResult == DialogResult.No)
                {
                    try
                    {
                        System.IO.File.Delete(sound_config);
                        using (StreamWriter sw = System.IO.File.CreateText(sound_config))
                        {
                            sw.Write("false");
                        }
                    }
                    catch (Exception fa_no)
                    {
                        Console.WriteLine(fa_no.ToString());
                    }
                    checkMacHAL();
                }
            }
            else if (do_soundfix == true)
            {
                SoundFix_StageInterpreter();
            }
            else
            {
                checkMacHAL();
            }
        }

        public static void SoundFix_StageInterpreter()
        {
            string home_path = "C:\\GPU_Disabler";
            string intr_path = home_path + "\\sound_stage.intr";

            if (!System.IO.File.Exists(intr_path))
            {
                // Interpreter inte exists
                try
                {
                    using (StreamWriter sw = System.IO.File.CreateText(intr_path))
                    {
                        sw.Write("1");
                    }

                    SoundFix(1);
                }
                catch (Exception intr1)
                {
                    Console.WriteLine("Couldn't write stage interpreter -> " + intr1.ToString());
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
            else
            {
                // Interpreter exists
                try
                {
                    string interpreter_content = System.IO.File.ReadAllText(intr_path);

                    if (interpreter_content == "1")
                    {
                        SoundFix(1);
                    }
                    else if (interpreter_content == "2")
                    {
                        SoundFix(2);
                    }
                    else if (interpreter_content == "3")
                    {
                        SoundFix(3);
                    }
                    else if (interpreter_content == "4")
                    {
                        SoundFix(4);
                    }
                    else if (interpreter_content == "finished")
                    {
                        SoundFix(5);
                    }
                    else
                    {
                        Console.WriteLine("Interpreter corrupted.");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                }
                catch (Exception idk)
                {
                    Console.WriteLine("Something fucked the interpreter -> " + idk.ToString());
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
        }

        public static void SoundFix(int stage)
        {
            /* SoundFix Stages:
             * stage = 1: 1
             * stage = 2: 2
             * stage = 3: 3
             * stage = 4: 4
             * stage = 5: no stage, soundfix already applied (finished)
             */

            string sound_config = "C:\\GPU_Disabler\\sound.config";
            string something_wrong = "";
            Console.WriteLine("Entering SoundFix Stage....");
            if (stage == 1)
            {
                // STAGE 1 HERE
                Console.WriteLine("(SFS 1) SoundFix Stage....");

                System.IO.File.Delete(sound_config);
                using (StreamWriter sw = System.IO.File.CreateText(sound_config))
                {
                    sw.Write("true");
                }

                try
                {
                    using (var client = new WebClient())
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        // yes internet
                        Console.WriteLine("(SFS 1) Checking for Windows Driver Kit Executable...");
                        if (System.IO.File.Exists(@"C:\GPU_Disabler\wdksetup.exe"))
                        {
                            Console.WriteLine("(SFS 1) Windows Driver Kit Executable found. Deleting.");
                            System.IO.File.Delete(@"C:\GPU_Disabler\wdksetup.exe");
                            Console.WriteLine("(SFS 1) Windows Driver Kit Executable removed.");
                        }
                        Console.WriteLine("(SFS 1) Downloading Windows Driver Kit Executable...");
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile("https://download.microsoft.com/download/8/1/6/816FE939-15C7-4185-9767-42ED05524A95/wdk/wdksetup.exe", @"C:\GPU_Disabler\wdksetup.exe");
                        Console.WriteLine("(SFS 1) Windows Driver Kit Executable downloaded to: C:\\GPU_Disabler\\wdksetup.exe");
                        Console.WriteLine("\nCAUTION: Starting Windows Driver Kit Setup now.\nPlease follow all instructions during setup.\nOnce installed completely, press any key to continue here.\n\n(If you have previously installed the WDK, just press any key to skip)\n");
                        Process.Start(@"C:\GPU_Disabler\wdksetup.exe");
                        Console.ReadKey();

                        Console.WriteLine("(SFS 1) Checking for C:\\dsdt...");
                        // Okay now create dsdt directory; delete entire folder if it already exists
                        if (System.IO.Directory.Exists(@"C:\dsdt"))
                        {
                            Console.WriteLine("(SFS 1) Removed existing C:\\dsdt directory.");
                            System.IO.Directory.Delete(@"C:\dsdt", true);
                        }
                        System.IO.Directory.CreateDirectory(@"C:\dsdt");
                        Console.WriteLine("(SFS 1) Created new C:\\dsdt directory. Continuing with command prompt...");
                        Console.WriteLine("(SFS 1) Here goes nothing...");

                        Process cmd = new Process();
                        cmd.StartInfo.FileName = "cmd.exe";
                        cmd.StartInfo.Arguments = "/C cd \\ & cd C:\\ & dir/w";
                        cmd.StartInfo.UseShellExecute = false;
                        cmd.StartInfo.RedirectStandardOutput = true;
                        cmd.StartInfo.RedirectStandardError = true;
                        cmd.Start();

                        while (!cmd.StandardOutput.EndOfStream)
                        {
                            string line = cmd.StandardOutput.ReadLine();
                            Console.WriteLine(line);
                        }


                        // Windows Binary Tools shit below, sorry got too ahead of myself now have to deal with wrong order of setup


                        Console.WriteLine("(SFS 1) Checking for Windows Binary Tools...");
                        if (System.IO.File.Exists(@"C:\GPU_Disabler\iasl-win.zip"))
                        {
                            Console.WriteLine("(SFS 1) Windows Binary Tools found. Deleting.");
                            System.IO.File.Delete(@"C:\GPU_Disabler\iasl-win.zip");
                            Console.WriteLine("(SFS 1) Windows Binary Tools removed.");
                        }
                        Console.WriteLine("(SFS 1) Downloading Windows Binary Tools...");
                        webClient.DownloadFile("https://acpica.org/sites/acpica/files/iasl-win-20161222.zip", @"C:\GPU_Disabler\iasl-win.zip");
                        Console.WriteLine("(SFS 1) Windows Binary Tools downloaded to: C:\\GPU_Disabler\\iasl-win.zip\n(SFS 1) Extracting iasl-win.zip");

                        if (System.IO.Directory.Exists(@"C:\dsdt"))
                        {
                            Console.WriteLine("(SFS 1) Removed existing C:\\dsdt directory.");
                            System.IO.Directory.Delete(@"C:\dsdt", true);
                        }
                        ZipFile.ExtractToDirectory(@"C:\GPU_Disabler\iasl-win.zip", @"C:\dsdt");
                        Console.WriteLine("(SFS 1) Successfully extracted files. Continuing with CMD...");
                        Console.WriteLine("(SFS 1) Here it comes...");

                        // batch creation here (DSDT1.BAT)
                        using (StreamWriter sw = System.IO.File.CreateText(@"C:\dsdt\dsdt1.bat"))
                        {
                            sw.WriteLine("cd /");
                            sw.WriteLine("c: & cd \\dsdt");
                            sw.WriteLine("copy /y \"C:\\Program Files (x86)\\Windows Kits\\10\\Tools\\x64\\ACPIVerify\\asl.exe\" c:\\dsdt");
                            sw.WriteLine("if not exist c:\\dsdt\\asl.exe echo ERROR: Failed to copy asl.exe to c:\\dsdt");
                        }
                        // run batch from above here (DSDT1.BAT)
                        var process = new Process();
                        var startinfo = new ProcessStartInfo("cmd.exe", @"/C C:\dsdt\dsdt1.bat");
                        startinfo.RedirectStandardOutput = true;
                        startinfo.UseShellExecute = false;
                        process.StartInfo = startinfo;
                        process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data); // do whatever processing you need to do in this handler
                        process.Start();
                        process.BeginOutputReadLine();
                        process.WaitForExit();


                        Console.WriteLine("(SFS 1) Dump ACPI Tables...");

                        // batch creation here (DSDT2.BAT)
                        using (StreamWriter sw = System.IO.File.CreateText(@"C:\dsdt\dsdt2.bat"))
                        {
                            sw.WriteLine("cd /");
                            sw.WriteLine("c: & cd \\dsdt");
                            sw.WriteLine("acpidump -b -z");
                            sw.WriteLine("asl /u dsdt.dat");
                            sw.WriteLine("copy dsdt.asl dsdt-modified.asl");
                        }
                        // run batch from above here (DSDT2.BAT)
                        var process2 = new Process();
                        var startinfo2 = new ProcessStartInfo("cmd.exe", @"/C C:\dsdt\dsdt2.bat");
                        startinfo2.RedirectStandardOutput = true;
                        startinfo2.UseShellExecute = false;
                        process2.StartInfo = startinfo2;
                        process2.OutputDataReceived += (sender2, args2) => Console.WriteLine(args2.Data); // do whatever processing you need to do in this handler
                        process2.Start();
                        process2.BeginOutputReadLine();
                        process2.WaitForExit();

                        Console.WriteLine("(SFS 1) Downloading refs.txt...");
                        webClient.DownloadFile("https://raw.githubusercontent.com/annabelsandford/Mac_Windows_GPU_Disabler/main/refs.txt", @"C:\dsdt\refs.txt");
                        Console.WriteLine("(SFS 1) Copied refs.txt to C:\\dsdt");
                        Console.WriteLine("(SFS 1) Back to CMD a third time. Praying it works...");

                        // batch creation here (REFS.BAT)
                        using (StreamWriter sw = System.IO.File.CreateText(@"C:\dsdt\refs.bat"))
                        {
                            sw.WriteLine("cd /");
                            sw.WriteLine("c: & cd \\dsdt");
                            sw.WriteLine("iasl -da -dl -fe refs.txt dsdt.dat");
                            sw.WriteLine("copy dsdt.dsl dsdt-modified.dsl");
                        }
                        // run batch from above here (REFS.BAT)
                        var process3 = new Process();
                        var startinfo3 = new ProcessStartInfo("cmd.exe", @"/C C:\dsdt\refs.bat");
                        startinfo3.RedirectStandardOutput = true;
                        startinfo3.UseShellExecute = false;
                        process3.StartInfo = startinfo3;
                        process3.OutputDataReceived += (sender3, args3) => Console.WriteLine(args3.Data); // do whatever processing you need to do in this handler
                        process3.Start();
                        process3.BeginOutputReadLine();
                        process3.WaitForExit();

                        // Okay let's find the needle in that damn haystack.
                        // Needle: Last DWordMemory - Haystack: dsdt-modified.dsl

                        Console.WriteLine("(SFS 1) Looking for dsdt-modified.dsl ...");
                        if (System.IO.File.Exists(@"C:\dsdt\dsdt-modified.dsl"))
                        {
                            Console.WriteLine("(SFS 1) dsdt-modified.dsl found. Thinking about proceeding... okay yeah let's do it.");

                            int general_line_count = 0;
                            int latest_dwordmemory_line = 0;
                            int latest_crs_line = 0;
                            bool crs_found = false;

                            Console.WriteLine("(SFS 1) Processing DSL (Can take a while be patient ok)");
                            var lines = System.IO.File.ReadLines(@"C:\dsdt\dsdt-modified.dsl");
                            foreach (var line in lines)
                            {
                                //Console.WriteLine("DEBUG: > " + line);
                                general_line_count++;
                                if (line.ToString().Contains("DWordMemory"))
                                {
                                    latest_dwordmemory_line = general_line_count;
                                }
                                if (line.ToString().Contains("Method (_CRS, 0, Serialized)"))
                                {
                                    latest_crs_line = general_line_count;
                                    crs_found = true;
                                    break;
                                }
                            }
                            if (crs_found == false)
                            {
                                Console.WriteLine("CRITICAL ERROR: dsdt-modified.dsl error (CRS_FOUND = FALSE)");
                                Console.ReadKey();
                                Environment.Exit(0);
                            }

                            Console.WriteLine("(DEBUG: Latest DWordMemory Address: " + latest_dwordmemory_line + ")\n(DEBUG: Cut Method CRS: " + latest_crs_line + ")");
                            
                            // Okay now that we found the needle(s), let's replace DWordMemory with QWordMemory

                            try
                            {
                                Console.WriteLine("(SFS 1) Writing DSL Part I ...");
                                using (StreamWriter sw = System.IO.File.CreateText(@"C:\GPU_Disabler\dsdt-part-1.dsl"))
                                {
                                    var lines2 = System.IO.File.ReadLines(@"C:\dsdt\dsdt-modified.dsl");
                                    int line2_counter = 0;
                                    int dwordmemory_line = latest_dwordmemory_line - 1;
                                    foreach (var line2 in lines2)
                                    {
                                        if (line2_counter < dwordmemory_line)
                                        {
                                            line2_counter++;
                                            sw.WriteLine(line2);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                                Console.WriteLine("(SFS 1) Writing DSL Part II ...");
                                using (StreamWriter sw = System.IO.File.CreateText(@"C:\GPU_Disabler\dsdt-part-2.dsl"))
                                {
                                    var lines3 = System.IO.File.ReadLines(@"C:\dsdt\dsdt-modified.dsl");
                                    int line3_counter = 0;
                                    foreach (var line3 in lines3)
                                    {
                                        if (line3_counter < latest_crs_line - 1)
                                        {
                                            line3_counter++;
                                        }
                                        else
                                        {
                                            sw.WriteLine(line3);
                                        }
                                    }
                                }

                                Console.WriteLine("(SFS 1) Merge DSL Part I and II ...");
                                using (StreamWriter sw = System.IO.File.CreateText(@"C:\GPU_Disabler\dsdt-part-final.dsl"))
                                {
                                    var lines_part1 = System.IO.File.ReadLines(@"C:\GPU_Disabler\dsdt-part-1.dsl");
                                    var lines_part2 = System.IO.File.ReadLines(@"C:\GPU_Disabler\dsdt-part-2.dsl");
                                    foreach (var line_part1 in lines_part1)
                                    {
                                        sw.WriteLine(line_part1);
                                    }
                                    // squeeze in QWordMemory here
                                    sw.WriteLine("QWordMemory (ResourceProducer, PosDecode, MinFixed, MaxFixed, Cacheable, ReadWrite,");
                                    sw.WriteLine("   0x0000000000000000, // Granularity");
                                    sw.WriteLine("   0x0000000C20000000, // Range Minimum,  set it to 48.5GB");
                                    sw.WriteLine("   0x0000000E0FFFFFFF, // Range Maximum,  set it to 56.25GB");
                                    sw.WriteLine("   0x0000000000000000, // Translation Offset");
                                    sw.WriteLine("   0x00000001F0000000, // Length calculated by Range Max - Range Min.");
                                    sw.WriteLine("   ,, , AddressRangeMemory, TypeStatic)");
                                    sw.WriteLine("})");
                                    // stop squeezing QWordMemory here, continue with part II
                                    foreach (var line_part2 in lines_part2)
                                    {
                                        sw.WriteLine(line_part2);
                                    }
                                }
                            }
                            catch (Exception why)
                            {
                                Console.WriteLine("CRITICAL ERROR: Could not process damn text files. Not administrator?\n" + why.ToString());
                                Console.ReadKey();
                                Environment.Exit(0);
                            }
                        }
                        else
                        {
                            Console.WriteLine("CRITICAL ERROR: dsdt-modified.dsl in C:\\dsdt not found.");
                            Console.ReadKey();
                            Environment.Exit(0);
                        }

                        Console.WriteLine("(SFS 1) QWordMemory squeezed in. Checking...");
                        if (!System.IO.File.Exists(@"C:\GPU_Disabler\dsdt-part-final.dsl"))
                        {
                            Console.WriteLine("CRITICAL ERROR: dsdt-part-final.dsl not found under C:\\GPU_Disabler");
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                        if (!System.IO.File.Exists(@"C:\dsdt\dsdt-modified.dsl"))
                        {
                            Console.WriteLine("CRITICAL ERROR: dsdt-modified.dsl not found under C:\\dsdt");
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                        Console.WriteLine("(SFS 1) All checks passed! Copying now...");

                        // Delete original dsdt-modified.dsl and replace with final-part in GPU_Disabler directory
                        try
                        {
                            System.IO.File.Delete(@"C:\dsdt\dsdt-modified.dsl");
                            System.IO.File.Move(@"C:\GPU_Disabler\dsdt-part-final.dsl", @"C:\dsdt\dsdt-modified.dsl");
                        }
                        catch (Exception fuck)
                        {
                            Console.WriteLine("CRITICAL ERROR: Could not copy dsdt-part-final.dsl to dsdt dir\n" + fuck.ToString());
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                        Console.WriteLine("(SFS 1) Okay all moved :) Going back to CMD. Wish me luck.");

                        // batch creation here (MODIFIED.BAT)
                        using (StreamWriter sw = System.IO.File.CreateText(@"C:\dsdt\modified.bat"))
                        {
                            sw.WriteLine("cd /");
                            sw.WriteLine("c: & cd \\dsdt");
                            sw.WriteLine("iasl -ve dsdt-modified.dsl");
                        }
                        // run batch from above here (MODIFIED.BAT)
                        var process4 = new Process();
                        var startinfo4 = new ProcessStartInfo("cmd.exe", @"/C C:\dsdt\modified.bat");
                        startinfo4.RedirectStandardOutput = true;
                        startinfo4.UseShellExecute = false;
                        process4.StartInfo = startinfo4;
                        process4.OutputDataReceived += (sender4, args4) => Console.WriteLine(args4.Data); // do whatever processing you need to do in this handler
                        process4.Start();
                        process4.BeginOutputReadLine();
                        process4.WaitForExit();


                        // -------------------------------------------------------------------------------------------------------------------------------------------------
                        // Okay all the big shit is over now. Go into testsining and do stuff
                        Console.WriteLine("(SFS 1) Preparing Windows Registry DSDT override...");

                        // batch creation here (OVERRIDE.BAT)
                        using (StreamWriter sw = System.IO.File.CreateText(@"C:\dsdt\override.bat"))
                        {
                            sw.WriteLine("cd /");
                            sw.WriteLine("c: & cd \\dsdt");
                            sw.WriteLine("asl /loadtable dsdt-modified.aml");
                            sw.WriteLine("bcdedit -set TESTSIGNING ON");
                        }
                        // run batch from above here (OVERRIDE.BAT)
                        var process5 = new Process();
                        var startinfo5 = new ProcessStartInfo("cmd.exe", @"/C C:\dsdt\override.bat");
                        startinfo5.RedirectStandardOutput = true;
                        startinfo5.UseShellExecute = false;
                        process5.StartInfo = startinfo5;
                        process5.OutputDataReceived += (sender5, args5) => Console.WriteLine(args5.Data); // do whatever processing you need to do in this handler
                        process5.Start();
                        process5.BeginOutputReadLine();
                        process5.WaitForExit();

                        System.IO.File.Delete(@"C:\GPU_Disabler\sound_stage.intr");
                        using (StreamWriter sw = System.IO.File.CreateText(@"C:\GPU_Disabler\sound_stage.intr"))
                        {
                            sw.Write("4");
                        }
                        Console.WriteLine("SoundFix Stage 1 (SFS1) Completed.\nRebooting in 10 Seconds...");
                        Thread.Sleep(10000);
                        System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
                    }
                }
                catch (Exception sumtingwong1)
                {
                    Console.WriteLine("Something went super wrong:\n" + sumtingwong1.ToString());
                    checkMacHAL();
                }
            }
            else if (stage == 2)
            {
                // STAGE 2 HERE
                // Update 02/21/22: There is no stage 2 lol. There's also no stage 3.
                // Didn't think it'd be this easy lol (just kidding it was horriffic)
            }
            else if (stage == 3)
            {
                // STAGE 3 HERE
            }
            else if (stage == 4)
            {
                DialogResult dialogResult = MessageBox.Show("Does sound work? (Please test thoroughly)", "Sound Fix (Stage 4) - GPU Disabler", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    System.IO.File.Delete(@"C:\GPU_Disabler\sound_stage.intr");
                    System.IO.File.Delete(sound_config);
                    using (StreamWriter sw = System.IO.File.CreateText(@"C:\GPU_Disabler\sound_stage.intr"))
                    {
                        sw.Write("finished");
                    }
                    using (StreamWriter sw = System.IO.File.CreateText(sound_config))
                    {
                        sw.Write("false");
                    }
                    MessageBox.Show("SoundFix completed.\nPress any key to continue");
                    checkMacHAL();
                }
                else
                {
                    DialogResult dialogResult2 = MessageBox.Show("Have you installed all Bootcamp drivers?", "Sound Fix (Stage 4) - GPU Disabler", MessageBoxButtons.YesNo);
                    if (dialogResult2 == DialogResult.Yes)
                    {
                        Console.WriteLine("(SFS 4) Run modified.bat again");
                        var process4 = new Process();
                        var startinfo4 = new ProcessStartInfo("cmd.exe", @"/C C:\dsdt\modified.bat");
                        startinfo4.RedirectStandardOutput = true;
                        startinfo4.UseShellExecute = false;
                        process4.StartInfo = startinfo4;
                        process4.OutputDataReceived += (sender4, args4) => Console.WriteLine(args4.Data); // do whatever processing you need to do in this handler
                        process4.Start();
                        process4.BeginOutputReadLine();
                        process4.WaitForExit();

                        Console.WriteLine("(SFS 4) Run override.bat again");
                        var process5 = new Process();
                        var startinfo5 = new ProcessStartInfo("cmd.exe", @"/C C:\dsdt\override.bat");
                        startinfo5.RedirectStandardOutput = true;
                        startinfo5.UseShellExecute = false;
                        process5.StartInfo = startinfo5;
                        process5.OutputDataReceived += (sender5, args5) => Console.WriteLine(args5.Data); // do whatever processing you need to do in this handler
                        process5.Start();
                        process5.BeginOutputReadLine();
                        process5.WaitForExit();

                        System.IO.File.Delete(@"C:\GPU_Disabler\sound_stage.intr");
                        using (StreamWriter sw = System.IO.File.CreateText(@"C:\GPU_Disabler\sound_stage.intr"))
                        {
                            sw.Write("finished");
                        }

                        Console.WriteLine("SoundFix Stage 4 (SFS4) Completed.\nRebooting in 10 Seconds...");
                        Thread.Sleep(10000);
                        System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
                    }
                    else
                    {
                        MessageBox.Show("Please install all Bootcamp drivers for this device (Including REALTEK Audio) - Then try the fix again.");
                        System.IO.File.Delete(@"C:\GPU_Disabler\sound_stage.intr");
                        using (StreamWriter sw = System.IO.File.CreateText(@"C:\GPU_Disabler\sound_stage.intr"))
                        {
                            sw.Write("1");
                        }
                        Environment.Exit(0);
                    }
                }
            }
            else if (stage == 5)
            {
                System.IO.File.Delete(@"C:\GPU_Disabler\sound_stage.intr");
                System.IO.File.Delete(sound_config);
                using (StreamWriter sw = System.IO.File.CreateText(@"C:\GPU_Disabler\sound_stage.intr"))
                {
                    sw.Write("finished");
                }
                using (StreamWriter sw = System.IO.File.CreateText(sound_config))
                {
                    sw.Write("false");
                }
                Console.WriteLine("SoundFix Completed.\nPress any key to continue");
                Console.ReadKey();
                checkMacHAL();
            }
            else
            {
                // STAGE CORRUPTED SOMEHOW HERE
            }
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

            // dirty, i know, but just try to delete .exe files that dont belong into the dir
            if (System.IO.File.Exists(@"C:\GPU_Disabler\wdksetup.exe"))
            {
                System.IO.File.Delete(@"C:\GPU_Disabler\wdksetup.exe");
            }
            // okay end of dirty trick. Please pretend like this has never happened ok thank you


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
            SoundFixCheck();
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
