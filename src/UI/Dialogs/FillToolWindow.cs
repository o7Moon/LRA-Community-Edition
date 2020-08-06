using Gwen.Controls;
using Gwen;
using linerider.Tools;
using linerider.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using linerider.Game;
using OpenTK;
using System.ComponentModel.Design;

namespace linerider.UI.Dialogs
{
    class FillToolWindow : DialogBase
    {
        private bool _linechangemade = false;
        private bool closing = false;
        private const string DefaultTitle = "Fill Tool";

        private List<LineSelection> _lineSelection;
        private List<Line> _lines;
        private PropertyTree _proptree;

        private CheckProperty _blueLines;
        private CheckProperty _redLines;
        private CheckProperty _greenLines;

        private NumberProperty _angle;
        private NumberProperty _offset;
        private NumberProperty _spacing;
        private NumberProperty _scenerywidth;

        public FillToolWindow(GameCanvas parent, Editor editor, List<LineSelection> lineSelection) : base(parent, editor)
        {
            _editor = editor;
            _lineSelection = lineSelection;
            _canvas = parent;

            _lines = new List<Line>();
            foreach (var selection in _lineSelection)
            {
                _lines.Add(selection.line);
            }

            Title = DefaultTitle;
            AutoSizeToContents = true;
            MakeModal(true);

            _proptree = new PropertyTree(this)
            {
                Width = 220,
                Height = 200,
                Dock = Dock.Top
            };

            Setup();
            _proptree.ExpandAll();

        }
        private void Setup()
        {
            SetupOptions(_proptree);

            Panel bottom = new Panel(this)
            {
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
                ShouldDrawBackground = false,
            };
            Button cancel = new Button(bottom)
            {
                Text = "Cancel",
                Dock = Dock.Right,
            };
            cancel.Clicked += (o, e) =>
            {
                CancelChange();
                closing = true;
                Close();
            };
            Button ok = new Button(bottom)
            {
                Text = "Okay",
                Dock = Dock.Right,
                Margin = new Margin(0, 0, 5, 0)
            };
            ok.Clicked += (o, e) =>
            {
                FinishChange();
                closing = true;
                Close();
            };
        }

        private void SetupOptions(PropertyTree tree)
        {
            var lineColor = tree.Add("Line Color (Select one)", 120);
            var settingsTable = tree.Add("Settings", 120);
            _blueLines = GwenHelper.AddPropertyCheckbox(lineColor, "Blue Lines", false);
            _blueLines.ValueChanged += (o, e) =>
            {
                UpdateFill(_angle.NumberValue, _spacing.NumberValue, _offset.NumberValue);
            };

            _redLines = GwenHelper.AddPropertyCheckbox(lineColor, "Red Lines", false);
            _redLines.ValueChanged += (o, e) =>
            {
                UpdateFill(_angle.NumberValue, _spacing.NumberValue, _offset.NumberValue);
            };

            _greenLines = GwenHelper.AddPropertyCheckbox(lineColor, "Green Lines", true);
            _greenLines.ValueChanged += (o, e) =>
            {
                UpdateFill(_angle.NumberValue, _spacing.NumberValue, _offset.NumberValue);
            };

            _angle = new NumberProperty(settingsTable)
            {
                Min = -1000,
                Max = 1000,
                NumberValue = 0,
            };
            _angle.ValueChanged += (o, e) =>
            {
                UpdateFill(_angle.NumberValue, _spacing.NumberValue, _offset.NumberValue);
            };
            settingsTable.Add("Angle", _angle);

            _spacing = new NumberProperty(settingsTable)
            {
                Min = -0.2,
                Max = int.MaxValue,
                NumberValue = -0.2,
            };
            _spacing.ValueChanged += (o, e) =>
            {
                UpdateFill(_angle.NumberValue, _spacing.NumberValue, _offset.NumberValue);
            };
            settingsTable.Add("Spacing", _spacing);

            _offset = new NumberProperty(settingsTable)
            {
                Min = 0,
                Max = 1,
                NumberValue = 0,

            };
            _offset.ValueChanged += (o, e) =>
            {
                UpdateFill(_angle.NumberValue, _spacing.NumberValue, _offset.NumberValue);
            };
            settingsTable.Add("Offset", _offset);

            _scenerywidth = new NumberProperty(settingsTable)
            {
                Min = 0.1,
                Max = 25,
                NumberValue = 1,

            };
            _scenerywidth.ValueChanged += (o, e) =>
            {
                UpdateFill(_angle.NumberValue, _spacing.NumberValue, _offset.NumberValue);
            };
            settingsTable.Add("Scenery Width", _scenerywidth);
            UpdateFill(_angle.NumberValue, _spacing.NumberValue, _offset.NumberValue);
        }
        /// <summary>
        /// Implemntation of conundrumer's shader mod for linerider.com
        /// 
        /// Source .js file: https://github.com/EmergentStudios/linerider-userscript-mods/blob/6362165f90b743941fba7dc3f1f2f12a1d38b50e/selection-shader-mod.user.js
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="spacing"></param>
        /// <param name="offset"></param>
        private void UpdateFill(double angle, double spacing, double offset)
        {
            //Lines will be put in here
            List<GameLine> fillLines = new List<GameLine>();

            spacing = 2 * (1 + spacing); //Actual spacing
            offset = spacing * offset; //Actual offset


            // sort by x
            List<PointWithLine> points = new List<PointWithLine>();
            foreach (GameLine gameline in _lines)
            {
                GameLine edited = gameline.Clone();

                edited.Position = Utility.Rotate(edited.Position, Vector2d.Zero, Angle.FromDegrees(angle));
                edited.Position2 = Utility.Rotate(edited.Position2, Vector2d.Zero, Angle.FromDegrees(angle));

                // sort endpoints
                if (edited.Position.X < edited.Position2.X)
                {
                    //Do nothing
                }
                else
                {
                    var temp = edited.Position;
                    edited.Position = edited.Position2;
                    edited.Position2 = temp;
                }

                points.Add(new PointWithLine(edited.Position, edited));
                points.Add(new PointWithLine(edited.Position2, edited));

                //CreateSingleLine(new RedLine(edited.Position, edited.Position2));
            }

            List<PointWithLine> SortedPointsByX = points.OrderBy(o => o.point.X).ToList();



            List<GameLine> currentLines = new List<GameLine>();

            // keep track of sorted y positions (for inner loop)
            List<Double> ys = new List<Double>();

            var currentX = SortedPointsByX[0].point.X + offset; //Cursor X axis
            foreach (var point in SortedPointsByX)
            {
                //Sweeping through X axis
                for (; currentX < point.point.X; currentX += spacing)
                {
                    foreach (GameLine gameline in currentLines)
                    {
                        // get relative x position of cursor on currentLine
                        double t = (currentX - gameline.Position.X) / (gameline.Position2.X - gameline.Position.X);

                        // get y position of intersection btwn cursor and currentLine
                        double y = t * (gameline.Position2.Y - gameline.Position.Y) + gameline.Position.Y;

                        // insert sorted
                        ys.Add(y);
                        ys.Sort(); //Sort from least to greatest
                    }
                    // keep track of inside/outside fill
                    double currentY = 0;
                    // vertically sweep through lines
                    foreach (double y in ys)
                    {
                        if (currentY == 0)
                        {
                            // enter fill
                            currentY = y;
                        }
                        else if (currentY == y)
                        {
                            // do not include the edge case of exactly overlapping lines
                        }
                        else
                        {
                            // yield the reverse transformed segment between currentY and y
                            Vector2d positon = new Vector2d(currentX, currentY);
                            Vector2d positon2 = new Vector2d(currentX, y);

                            Vector2d p1 = Utility.Rotate(positon, Vector2d.Zero, Angle.FromDegrees(-angle));
                            Vector2d p2 = Utility.Rotate(positon2, Vector2d.Zero, Angle.FromDegrees(-angle));

                            if (_greenLines.IsChecked)
                            {
                                fillLines.Add(new SceneryLine(p1, p2));
                                fillLines[fillLines.Count() - 1].Width = (float)_scenerywidth.NumberValue;
                            }
                            else if (_blueLines.IsChecked)
                            {
                                fillLines.Add(new StandardLine(p1, p2));
                            }
                            else if (_redLines.IsChecked)
                            {
                                fillLines.Add(new RedLine(p1, p2));
                            }
                            else 
                            {
                                //Bruh just select one
                            }

                            //exit fill
                            currentY = 0;
                        }
                    }

                    // clear ys for next iteration
                    ys.Clear();
                }

                // enter/exit line segments
                if (currentLines.Contains(point.line))
                {
                    currentLines.Remove(point.line);
                }
                else
                {
                    currentLines.Add(point.line);
                }
            }
            CreateLinesFromList(fillLines);
        }

        private void CreateLinesFromList(List<GameLine> lines)
        {
            using (var trk = _editor.CreateTrackWriter())
            {
                CancelChange();
                MakingChange();
                foreach (var line in lines)
                {
                    trk.AddLine(line);
                }
            }
        }
        private void CreateSingleLine(GameLine line)
        {
            _editor.UndoManager.BeginAction();
            using (var trk = _editor.CreateTrackWriter())
            {
                trk.AddLine(line);
            }
            _editor.UndoManager.EndAction();
            _editor.NotifyTrackChanged();
        }
        private void MakingChange()
        {
            if (!_linechangemade)
            {
                _editor.UndoManager.BeginAction();
                _linechangemade = true;
                Title = DefaultTitle + " *";
            }
        }
        private void CancelChange()
        {
            if (_linechangemade)
            {
                _editor.UndoManager.CancelAction();
                _linechangemade = false;
            }
        }
        private void FinishChange()
        {
            if (_linechangemade)
            {
                _editor.UndoManager.EndAction();
                _linechangemade = false;
            }
        }
        protected override void CloseButtonPressed(ControlBase control, EventArgs args)
        {
            if (closing || !_linechangemade)
            {
                closing = true;
                base.CloseButtonPressed(control, args);
            }
            else
            {
                WarnClose();
            }
        }
        public override bool Close()
        {
            if (closing || !_linechangemade)
            {
                closing = true;
                return base.Close();
            }
            else
            {
                WarnClose();
                return false;
            }
        }
        private void WarnClose()
        {
            var mbox = MessageBox.Show(
                _canvas,
                "The line has been modified. Do you want to save your changes?",
                "Save Changes?",
                MessageBox.ButtonType.YesNoCancel);
            mbox.RenameButtonsYN("Save", "Discard", "Cancel");
            mbox.MakeModal(false);
            mbox.Dismissed += (o, e) =>
            {
                switch (e)
                {
                    case DialogResult.Yes:
                        FinishChange();
                        closing = true;
                        base.Close();
                        break;
                    case DialogResult.No:
                        CancelChange();
                        closing = true;
                        base.Close();
                        break;
                }
            };
        }
    }

    internal class PointWithLine
    {
        public GameLine line;
        public Vector2d point;
        public PointWithLine(Vector2d point, GameLine line)
        {
            this.point = point;
            this.line = line;
        }
    }
}
