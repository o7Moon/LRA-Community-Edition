using Gwen.Controls;
using Gwen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using IronPython;
using System;
using System.IO;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using linerider;
using linerider.Game;
using linerider.Utils;
using linerider.Scripting;
using linerider.Tools;

namespace linerider.UI.Dialogs
{
    public class ScriptsWindow : DialogBase
    {
        public ScriptsWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            Title = "IronPython Scripting (EXPERIMENTAL)";
            Setup();
            MakeModal(true);
            //DisableResizing();
            SetSize(350, 400);
        }
        private void Setup()
        {
            Gwen.Controls.Label deprecatedLabel = new Gwen.Controls.Label(this);
            deprecatedLabel.Text = "Note: This currently uses Python 2.7.10 which is\n" +
                "deprecated. I'd use Python 3.x but IronPython3 is not\n" +
                "ready yet for use currently.\n";
            deprecatedLabel.Dock = Dock.Top;

            Gwen.Controls.Label waringLabel = new Gwen.Controls.Label(this);
            waringLabel.Text = "WARNING: THIS IS UN-SANDBOXED. \nONLY RUN SCRIPTS FROM TRUSTED SOURCES.";
            waringLabel.TextColor = Color.Red;
            waringLabel.Dock = Dock.Top;
            
            Gwen.Controls.TextBox pythonInput = new Gwen.Controls.TextBox(this);
            pythonInput.Dock = Dock.Fill;
            pythonInput.Text = "The Python script will show here";
            pythonInput.Alignment = Pos.Top;
            pythonInput.IsDisabled = true;
            pythonInput.Size = pythonInput.GetSizeToFitContents();
            pythonInput.TextChanged += (o, e) =>
            {
                pythonInput.Size = pythonInput.GetSizeToFitContents();
            };
            Gwen.Controls.Button runButton = new Gwen.Controls.Button(this);
            runButton.Text = "Run Script";
            runButton.Dock = Dock.Bottom;
            runButton.Clicked += (o, e) =>
            {
                runPythonScript(pythonInput.Text);
            };
            Gwen.Controls.Button loadFileButton = new Gwen.Controls.Button(this);
            loadFileButton.Text = "Load Script From File";
            loadFileButton.Dock = Dock.Bottom;
            loadFileButton.Clicked += (o, e) =>
            {
                using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
                {
                    openFileDialog.Filter = "Python Scripts|*.py;*.pyw|All Files|*";

                    if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            string filePath = openFileDialog.FileName;
                            string text = System.IO.File.ReadAllText(filePath);
                            pythonInput.Text = text;
                        }
                        catch { }
                    }
                }
            };
        }

        private void runPythonScript(string text)
        {
            try
            {
                PythonHelper pythonHelper = new PythonHelper(_editor, _canvas);
                IronPythonScriptRunner.RunPythonScript(text, pythonHelper);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
