using Gwen.Controls;
using linerider.Game;
using linerider.Tools;
using linerider.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace linerider.Utils
{
    public class PythonHelper
    {
        private Editor _editor;
        private GameCanvas _canvas;

        public bool GameIsRecording
        {
            get
            {
                return Settings.Local.RecordingMode;
            }
        }
        public int FrameOffset
        {
            get
            {
                return _editor.Offset;
            }
        }
        public int IterationsOffset
        {
            get
            {
                return _editor.IterationsOffset;
            }
        }
        public Rider RiderState
        {
            get
            {
                return _editor.RenderRider;
            }
        }

        public PythonHelper(Editor editor, GameCanvas parent)
        {
            _editor = editor;
            _canvas = parent;
        }
        public Panel MakePanelOnCanvas()
        {
            Panel panel = new Panel(_canvas);
            panel.Name = "PythonPanel";
            panel.SetSize(100, 100);
            return panel;
        }
        public Button MakeButtonOnCanvas()
        {
            Button button = new Button(_canvas);
            button.Name = "PythonButton";
            button.SetSize(100, 100);
            return button;
        }
        public WindowControl CreateWindowOnCanvas()
        {
            //Stop the tool so everything doesn't break
            if (_editor.Playing) 
            {
                _editor.TogglePause();
            }
            CurrentTools.SelectedTool.Stop();

            return new WindowControl(_canvas);
        }
        public TrackWriter CreateTrackWriter()
        {
            return _editor.CreateTrackWriter();
        }
        public TrackReader CreateTrackReader()
        {
            return _editor.CreateTrackReader();
        }
        public void BeginTrackChange()
        {
            _editor.UndoManager.BeginAction();
        }
        public void EndTrackChange()
        {
            _editor.UndoManager.EndAction();
        }
        public void CancelTrackChange()
        {
            _editor.UndoManager.CancelAction();
        }
        public void NotifyTrackChanged()
        {
            _editor.NotifyTrackChanged();
        }
        public void SetFrame(int frame)
        {
            _editor.SetFrame(frame);
        }
    }
}
