using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3DRenderingSystem
{
    public partial class MainForm : Form
    {
        Player player = new Player(new Vector(0, 2, 0));
        double FOV = 40;
        List<Polygon> polygons = new List<Polygon>();
        Rectangle screen;

        public MainForm()
        {
            InitializeComponent();
            Form1_Load(null, null);
            textBox1.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            screen = new Rectangle(0, 0, this.Width, this.Height);
            
            polygons.Add(Polygons.Cube(1, new Vector(0, 0, 5)));
            polygons.Add(Polygons.Cube(5, new Vector(5, 0, 5)));


            Cursor.Position = new Point(500, 500);
            FrameTick.Start();
            WorldTickHandler = new TickHandler(WorldTick);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {            
            e.Graphics.DrawRectangle(Pens.Black, screen);
            RenderEngine r = new RenderEngine(e, screen, player, FOV);
            
            r.Render(polygons);

            //label1.Text = Tick.ToString() + "   " + r.Player.DirectionVector.VertAngDeg.ToString(1, true) + "   " + player.DirectionVector.ToVector().ToString(1, true);
            string tempStr = "";
            if ((DateTime.Now - ProgramStartTime).TotalMilliseconds % 150 < 50)
                tempStr = "FPS: " + FPS.ToString(1, true) + "\nInterval: " + FrameTick.Interval;

            label1.Text = "Coord: " + player.Coord.ToString(1, true) + "\nDirection: " + player.DirectionVector.ToVector().ToString(1, true) + "\n" + tempStr;
        }


        double FPS = 0;
        DateTime ProgramStartTime = DateTime.Now;
        DateTime TimeOfPreviousFrameTick = DateTime.Now;
        List<double> frameTickDurations = new List<double>();
        int frameTickDurations_NumOfSamples = 5;

        private void FrameTick_Tick(object sender, EventArgs e)
        {
            double TimeElapsed = (DateTime.Now - TimeOfPreviousFrameTick).TotalSeconds;         // Time elapsed since previous frame tick
            TimeOfPreviousFrameTick = DateTime.Now;
            frameTickDurations.Add(TimeElapsed);
            if (frameTickDurations.Count > frameTickDurations_NumOfSamples) frameTickDurations.RemoveAt(0);
            FPS = frameTickDurations.Count / frameTickDurations.Sum();                          // Calculate average FPS

            if (keyControl.NotPausedOrHoldingControl)
            {
                double rotationSensitivity = 8 * 0.01;                  // Const X, where ChangeInAngleInDeg = X * ChangeInScreenCoord
                double movementSpeed = 5;                               // UnitPerSec aka BlocksPerSec

                #region Up-down rotation
                double Y_Offset = 500 - Cursor.Position.Y;
                player.DirectionVector.VertAngDeg -= Y_Offset * rotationSensitivity;
                #endregion

                #region Left-Right rotation
                double X_Offset = 500 - Cursor.Position.X;
                player.DirectionVector.HoriAngDeg -= X_Offset * rotationSensitivity;
                Cursor.Position = new Point(500, 500);
                #endregion

                #region Movement
                double distanceMoved = movementSpeed * TimeElapsed;     // UnitPerSec * TimeInSeconds
                if (keyControl.MoveForward)
                    player.Coord += player.DirectionVector.ToVector() * distanceMoved;
                if (keyControl.MoveBackward)
                    player.Coord -= player.DirectionVector.ToVector() * distanceMoved;
                if (keyControl.MoveLeft)
                {
                    SphericalVec temp = player.DirectionVector.ReturnOffset(0, -(Math.PI / 2), 0, rad: true, evokeSinCos: false);
                    temp.VertAngRad = Math.PI / 2;
                    player.Coord += temp.ToVector() * distanceMoved;
                }
                if (keyControl.MoveRight)
                {
                    SphericalVec temp = player.DirectionVector.ReturnOffset(0, (Math.PI / 2), 0, rad: true, evokeSinCos: false);
                    temp.VertAngRad = Math.PI / 2;
                    player.Coord += temp.ToVector() * distanceMoved;
                }
                if (keyControl.FlyUp)
                    player.Coord.Y += 1.0 * distanceMoved;
                if (keyControl.FlyDown)
                    player.Coord.Y -= 1.0 * distanceMoved;
                #endregion
            }



            Invalidate();

            #region Dynamic FrameTick Interval Adjuster
            if (FPS < 60 && FPS != 0 && FrameTick.Interval > 5)
                FrameTick.Interval--;
            else
                FrameTick.Interval++;
            #endregion
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (sender == null)
            {
                if (textBox1.Text.Length > 3)
                    if (textBox1.Text.Substring(0, 4) == "/tp ")            //teleport
                    {
                        string text = textBox1.Text.Substring(3).Replace(" ", "");
                        string[] array = text.Split(',');
                        if (array.Length == 3)
                        {
                            player.Coord.X = Convert.ToDouble(array[0]);
                            player.Coord.Y = Convert.ToDouble(array[1]);
                            player.Coord.Z = Convert.ToDouble(array[2]);
                        }
                    }
                UnfocusChatBox();
            }
        }

        private void UnfocusChatBox()
        {
            textBox1.Text = "";
            textBox1.Enabled = false;
            keyControl.Pause = false;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13) textBox1_Enter(null, e);
        }

        private void WorldTick_Tick(object sender, EventArgs e)
        {
            polygons[1].Shift(new Vector(-0.01 * WorldTickHandler.Correction, 0, 0));   //doesnt work
            WorldTickHandler.NextTick();
        }
        TickHandler WorldTickHandler;

        class TickHandler   //WIP
        {
            public Timer AttachedTimer;
            private DateTime PreviousTick;
            private bool IsFirstNow;        // To ensure all corrections in 1 tick uses the same DateTime.Now
            private bool RequireRecalculaion;
            private DateTime _Now;
            private double _TimeElapsed;
            private DateTime Now
            {
                get
                {
                    if (IsFirstNow)
                    {
                        _Now = DateTime.Now;
                        IsFirstNow = false;
                    }
                    return _Now;
                }
            }

            private double TimeElapsed
            {
                get
                {
                    if (RequireRecalculaion)
                        _TimeElapsed = (Now - PreviousTick).TotalSeconds;
                    return _TimeElapsed;
                }
            }

            public double Correction
                { get { return TimeElapsed / AttachedTimer.Interval; } }

            public TickHandler(Timer timer)
            {
                PreviousTick = DateTime.Now;
                AttachedTimer = timer;
                IsFirstNow = false;
                RequireRecalculaion = true;
            }                

            public void NextTick()
            {
                PreviousTick = Now;
                IsFirstNow = true;
                RequireRecalculaion = true;
            }
        }
    }
}
