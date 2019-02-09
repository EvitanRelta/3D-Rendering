using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace _3DRenderingSystem
{
    public partial class MainForm : Form
    {
        KeyControlsWrapper keyControl = new KeyControlsWrapper();

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            keyControl.CurrentKeyEventArgs = e;

            if (e.Control)
                keyControl.CtrlIsDown = true;
            else
                keyControl.CheckKeysAndSetTo(true);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            keyControl.CurrentKeyEventArgs = e;

            if (e.KeyCode == Keys.T)
                setChatBoxToFocus();

            if (e.KeyCode == Keys.Escape)
                keyControl.TogglePause();
            
            if (!e.Control && !keyControl.Pause)
            {
                Cursor.Position = new Point(500, 500);
                keyControl.CtrlIsDown = false;
                keyControl.CheckKeysAndSetTo(false);
            }
        }

        private void setChatBoxToFocus()
        {
            keyControl.Pause = true;
            textBox1.Enabled = true;
            textBox1.Focus();
        }
    }

    class KeyControlsWrapper
    {
        public KeyEventArgs CurrentKeyEventArgs { get; set; }
        public bool W { get; set; } = false;
        public bool A { get; set; } = false;
        public bool S { get; set; } = false;
        public bool D { get; set; } = false;
        public bool Space { get; set; } = false;
        public bool Shift { get; set; } = false;

        private bool pause = false;
        public bool Pause
        {
            get => pause;
            set
            {
                pause = value;
                CheckAndSetCursorVisibility();
            }
        }

        private bool ctrlIsDown = false;
        public bool CtrlIsDown
        {
            get => ctrlIsDown;
            set
            {
                ctrlIsDown = value;
                CheckAndSetCursorVisibility();
            }
        }

        public bool NotPausedOrHoldingControl => !Pause && !CtrlIsDown;
        public bool MoveForward => W && !S;
        public bool MoveBackward => S && !W;
        public bool MoveLeft => A && !D;
        public bool MoveRight => D && !A;
        public bool FlyUp => Space && !Shift;
        public bool FlyDown => Shift && !Space;



        public KeyControlsWrapper() { }

        public void CheckKeysAndSetTo(bool setValue)
        {
            KeyEventArgs e = CurrentKeyEventArgs;
            if (e.KeyCode == Keys.W)
                W = setValue;
            if (e.KeyCode == Keys.A)
                A = setValue;
            if (e.KeyCode == Keys.S)
                S = setValue;
            if (e.KeyCode == Keys.D)
                D = setValue;
            if (e.KeyCode == Keys.Space)
                Space = setValue;
            if (e.KeyCode == Keys.ShiftKey)
                Shift = setValue;
        }
        
        public void TogglePause()
        {
            if (Pause)
                Pause = false;
            else Pause = true;
        }

        private void CheckAndSetCursorVisibility()      //not sure why doesnt work
        {
            if (Pause && CtrlIsDown)
                Cursor.Show();
            else
                Cursor.Hide();
        }
    }
}
