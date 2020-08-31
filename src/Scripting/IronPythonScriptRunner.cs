using IronPython.Hosting;
using linerider.Utils;
using Microsoft.Scripting;
using System;
using Gwen;
using OpenTK;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Permissions;
using System.IO;
using System.Windows.Forms;
using System.Globalization;

namespace linerider.Scripting
{
    public class IronPythonScriptRunner
    {
        /// <summary>
        /// Used by the sandboxer to execute python scripts
        /// </summary>
        /// <param name="script"></param>
        /// <param name="_pythonHelper"></param>
        public static void RunPythonScript(string script, PythonHelper _pythonHelper)
        {
            try
            {
                if (ScanScript(script) == false) { return; }

                /* Attempted sandbox setup, it doesn't work and always errors out 
                 * I decided to scan the file for file read/write/execution methods 
                 *   so there's at least something there to protect users if someone
                 *   makes a malicious script.
                 *
                AppDomainSetup ads = new AppDomainSetup();
                ads.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                ads.DisallowBindingRedirects = false;
                ads.DisallowCodeDownload = true;
                ads.ShadowCopyFiles = "false";
                ads.ShadowCopyDirectories = "false";
                ads.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                var perms = new PermissionSet(PermissionState.None);
                perms.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                AppDomain sandbox = AppDomain.CreateDomain("Sandbox", null, ads, perms);
                var engine = Python.CreateEngine(sandbox);
                 *
                 */
                var engine = Python.CreateEngine();
                var scope = engine.CreateScope();
                scope.SetVariable("_pythonHelper", _pythonHelper);
                var source = engine.CreateScriptSourceFromString(script, SourceCodeKind.Statements);
                var compiled = source.Compile();
                var result = compiled.Execute(scope);
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(
                "Unhandled Python Exception: " +
                exception.Message +
                Environment.NewLine +
                Environment.NewLine +
                exception.StackTrace,
                "Python Error!",
                System.Windows.Forms.MessageBoxButtons.OK);
            }
        }
        /// <summary>
        /// Returns true if script should be executed.  
        /// Returns false if user selects no on the dialog box.  
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        private static bool ScanScript(string script)
        {
            string[] lines = script.Split(
                new[] { "\r\n", "\r", "\n" }, //Check different line ending (windows and unix endings)
             StringSplitOptions.None
            );

            string windowText = "";
            string listOfWaringsWithLines = "";

            //Just check for some sus stuff lol, not 100% the best but  w h a t e v e r
            string[] readWriteList = { "open(", "StreamReader", "ReadLine", 
                ".close", "WriteLine", "Write",
                ".txt", ".conf", ".json", ".xml", "C:", "delete" };
            string[] executeList = { ".exe", ".dll", ".bat", ".py", ".pyw", "Process", "execfile", "os.system", 
                 "http", };
            string[] importsList = { "clr.AddReference", "import" };

            int readWriteWarn = 0;
            int executesFileWarn = 0;
            int importWarn = 0;

            for (int i = 0; i<lines.Length; i++)
            {
                //Write check
                foreach (string str in readWriteList)
                {
                    if (ContainsIgnoreCase(lines[i], str))
                    {
                        readWriteWarn++;
                        listOfWaringsWithLines += "Write to or read a file on line [" + (i + 1) + "]: \t\"" + lines[i] + "\"\n" + "";
                    }
                }
                
                //execution check
                foreach (string str in executeList)
                {
                    if (ContainsIgnoreCase(lines[i], str))
                    {
                        executesFileWarn++;
                        listOfWaringsWithLines += "Executing a file on line [" + (i + 1) + "]: \t\"" + lines[i] + "\"\n" + "";
                    }
                }

                //something check
                foreach (string str in importsList)
                {
                    if (ContainsIgnoreCase(lines[i], str))
                    {
                        importWarn++;
                        listOfWaringsWithLines += "Is trying to import on line [" + (i + 1) + "]: \t\"" + lines[i] + "\"\n" + "";
                    }
                }
            }

            if (readWriteWarn > 0 && executesFileWarn == 0 && importWarn == 0) //If reading and writing (Normal for saving/loading settings from a file)
            {
                windowText = "It looks like this script is reading and writing to one or more files.\n" +
                             "This is normal if it's trying to save / load settings to a file.\n\n";
                windowText += "Read/Write file(s) " + readWriteWarn + " time(s)\n";
                windowText += importWarn > 0 ? "Import file(s) " + importWarn + " time(s)\n" : "";

                windowText += "\n" + listOfWaringsWithLines;
            }
            else if (readWriteWarn > 0 || executesFileWarn > 0 || importWarn > 0)
            {
                windowText = "It looks like this file is trying to...\n\n";
                windowText += readWriteWarn > 0 ? "Read/Write file(s) "+ readWriteWarn + " time(s)\n" : "";
                windowText += executesFileWarn > 0 ? "Execute file(s) " + executesFileWarn + " time(s)\n" : "";
                windowText += importWarn > 0 ? "Import file(s) " + importWarn + " time(s)\n" : "";
                windowText += "\n" + listOfWaringsWithLines;
            }
            else //If all warnings false 
            {
                windowText += "Are you sure you want to run this script?";
                return ShowWindow(windowText);
            }

            windowText += "\nAre you sure you want to run this script?";
            return ShowWindow(windowText);
        }
        internal static bool ShowWindow(string text)
        {
            return (System.Windows.Forms.MessageBox.Show(
                text,
                "Script warning!",
                System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes);
        }
        internal static bool ContainsIgnoreCase(string str, string value) //Just so it looks nicer :p
        {
            return str.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
