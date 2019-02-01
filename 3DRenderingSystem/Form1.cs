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
    public partial class Form1 : Form
    {
        
        Player player = new Player(new Vector(0, 2, 0));
        double FOV = 40;
        List<Polygon> polygons = new List<Polygon>();
        Rectangle screen;

        public Form1()
        {
            InitializeComponent();
            Form1_Load(null, null);
            textBox1.Enabled = false;
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Vector a = new SphericalVec(99, 5, 5).ToVector();
            SphericalVec b = new SphericalVec(5, 5, 5);
            b.X = 99;

            screen = new Rectangle(0, 0, this.Width, this.Height);
            //screen = new Rectangle(200,100, this.Width - 400, this.Height - 200);
            
            polygons.Add(Polygons.Cube(1, new Vector(0, 0, 5)));
            polygons.Add(Polygons.Cube(5, new Vector(5, 0, 5)));
            polygons.Add(Polygons.Cube(10, new Vector(-5, 0, 5)));


            Cursor.Position = new Point(500, 500);
            FrameTick.Start();
            WorldTickHandler = new TickHandler(WorldTick);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //Rectangle screen = new Rectangle(50, 50, 1000, 500);
            
            e.Graphics.DrawRectangle(Pens.Black, screen);
            RenderEngine r = new RenderEngine(e, screen, player, FOV);
            
            r.Render(polygons);       // Render cube

            //label1.Text = Tick.ToString() + "   " + r.Player.DirectionVector.VertAngDeg.ToString(1, true) + "   " + player.DirectionVector.ToVector().ToString(1, true);
            if ((DateTime.Now - ProgramStartTime).TotalMilliseconds % 150 < 50)
                tempStr = "FPS: " + FPS.ToString(1, true) + "\nInterval: " + FrameTick.Interval;

            label1.Text = "Coord: " + player.Coord.ToString(1, true) + "\nDirection: " + player.DirectionVector.ToVector().ToString(1, true) + "\n" + tempStr;


        }

        string tempStr = "";


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

            if (!CtrlIsDown && !Pause)
            {
                double rotationSensitivity = 8 * 0.01;                  // Const X, where ChangeInAngleInDeg = X * ChangeInScreenCoord
                double movementSpeed = 5;                               // UnitPerSec aka BlocksPerSec
                double distanceMoved = movementSpeed * TimeElapsed;     // UnitPerSec * TimeInSeconds

                // Up-down rotation
                double Y_Offset = 500 - Cursor.Position.Y;
                player.DirectionVector.VertAngDeg -= Y_Offset * rotationSensitivity;

                // Left-Right rotation
                double X_Offset = 500 - Cursor.Position.X;
                player.DirectionVector.HoriAngDeg -= X_Offset * rotationSensitivity;
                Cursor.Position = new Point(500, 500);

                // Movement
                if (W && !S)    // Forward
                    player.Coord += player.DirectionVector.ToVector() * distanceMoved;
                if (S && !W)    // Backwards
                    player.Coord -= player.DirectionVector.ToVector() * distanceMoved;
                if (A && !D)    // Left
                {
                    SphericalVec temp = player.DirectionVector.ReturnOffset(0, -(Math.PI / 2), 0, rad: true, evokeSinCos: false);
                    temp.VertAngRad = Math.PI / 2;
                    player.Coord += temp.ToVector() * distanceMoved;
                }
                if (D && !A)  // Right
                {
                    SphericalVec temp = player.DirectionVector.ReturnOffset(0, (Math.PI / 2), 0, rad: true, evokeSinCos: false);
                    temp.VertAngRad = Math.PI / 2;
                    player.Coord += temp.ToVector() * distanceMoved;
                }
                if (Space && !Shift)  // Fly up
                    player.Coord.Y += 1.0 * distanceMoved;
                if (Shift && !Space)  // Fly down
                    player.Coord.Y -= 1.0 * distanceMoved;
            }



            Invalidate();

            //Dynamic Frame Tick Interval Adjuster
            if (FPS < 60 && FPS != 0 && FrameTick.Interval > 5)
                FrameTick.Interval--;
            else
                FrameTick.Interval++;
        }



        bool CtrlIsDown = false;
        bool W = false;
        bool A = false;
        bool S = false;
        bool D = false;
        bool Space = false;
        bool Shift = false;
        bool Pause = false;
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                CtrlIsDown = true;
            }
                
            else
            {
                if (e.KeyCode == Keys.W)
                    W = true;
                if (e.KeyCode == Keys.A)
                    A = true;
                if (e.KeyCode == Keys.S)
                    S = true;
                if (e.KeyCode == Keys.D)
                    D = true;
                if (e.KeyCode == Keys.Space)
                    Space= true;
                if (e.KeyCode == Keys.ShiftKey)
                    Shift = true;
            }
            
            //if (Pause || CtrlIsDown)
            //    Cursor.Show();
            //else
            //    Cursor.Hide();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.T)
            {
                Pause = true;
                textBox1.Enabled = true;
                textBox1.Focus();
            }
                

            if (e.KeyCode == Keys.Escape)
                if (!Pause)
                {
                    Pause = true;
                }

                else
                {
                    Pause = false;
                }
                    
            
           
            if (!e.Control && !Pause)
            {
                Cursor.Position = new Point(500, 500);

                CtrlIsDown = false;
                if (e.KeyCode == Keys.W)
                    W = false;
                if (e.KeyCode == Keys.A)
                    A = false;
                if (e.KeyCode == Keys.S)
                    S = false;
                if (e.KeyCode == Keys.D)
                    D = false;
                if (e.KeyCode == Keys.Space)
                    Space = false;
                if (e.KeyCode == Keys.ShiftKey)
                    Shift = false;
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }
        bool temp = false;
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
                textBox1.Text = "";
                textBox1.Enabled = false;
                Pause = false;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13) textBox1_Enter(null, e);
        }

        private void WorldTick_Tick(object sender, EventArgs e)
        {
            polygons[1].Shift(new Vector(-0.01 * WorldTickHandler.Correction, 0, 0));
            WorldTickHandler.NextTick();
        }
        TickHandler WorldTickHandler;

        class TickHandler
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
