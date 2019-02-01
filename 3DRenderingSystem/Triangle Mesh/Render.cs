using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;


namespace _3DRenderingSystem
{
    class RenderEngine
    {
        private Graphics g;
        private PaintEventArgs _paintEventArgs;
        public PaintEventArgs PaintEventArgs
        {
            get { return _paintEventArgs; }
            set
            {
                _paintEventArgs = value;
                g = value.Graphics;
            }
        }
        
        public Player Player;
        public Rectangle Screen;
        private double _horizontalFOV;      //HorizontalFOV = 30 means player can see 30degrees to left and 30 to right
        private double _verticalFOV;
        private double TanHorizontalFOV;
        private double TanVerticalFOV;

        public double HorizontalFOV
        {
            get { return _horizontalFOV; }
            set
            {
                _horizontalFOV = value;
                double Adj = (Screen.Width / 2.0) / Math.Tan((Math.PI / 180) * value);
                _verticalFOV = (180 / Math.PI) * Math.Atan((Screen.Height / 2.0) / Adj);
                TanHorizontalFOV = Math.Tan((Math.PI / 180) * _horizontalFOV);
                TanVerticalFOV = (Screen.Height / 2.0) / Adj;
            }
        }  
        public double VerticalFOV
        {
            get { return _verticalFOV; }
            set
            {
                _verticalFOV = value;
                double Adj = (Screen.Height / 2.0) / Math.Tan((Math.PI / 180) * value);
                _horizontalFOV = (180 / Math.PI) * Math.Atan((Screen.Width / 2.0) / Adj);
                TanHorizontalFOV = (Screen.Width / 2.0) / Adj;
                TanVerticalFOV = Math.Tan((Math.PI / 180) * _verticalFOV);
            }
        }

        //Constructor
        public RenderEngine(PaintEventArgs e, Rectangle screen, Player player, double horizontalFOV)
        {
            PaintEventArgs = e;
            Player = player;
            Screen = screen;
            HorizontalFOV = horizontalFOV;
        }
        
        public void Render(List<Polygon> polygon)     //testing without rotation
        {
            List<Face> hasBeenProcessed = new List<Face>();

            foreach (Polygon p in polygon)
                foreach (Vertex i in p.Vertices)
                {
                    Vector localCoord = ToLocalCoord(i.Coord);      //translate global to local coord
                    if (Math.Abs(localCoord.Y) <= localCoord.Z * TanVerticalFOV)        //checks if vertex is within vertical FOV
                        if (Math.Abs(localCoord.X) <= localCoord.Z * TanHorizontalFOV)  //checks if vertex is within horizontal FOV
                            foreach (Face c in i.ConnectedFaces)        //renders all faces connected to the vertex
                                if (!hasBeenProcessed.Contains(c))
                                    hasBeenProcessed.Add(c);            //pick out all the faces to be rendered
                }

            hasBeenProcessed.Sort((y, x) => x.DistFromMidPt(Player).CompareTo(y.DistFromMidPt(Player)));    //sorts with furtherst face first, so the closer faces will be drawn on top

            double d = 0.08;
            foreach (Face i in hasBeenProcessed)        //draw faces
            {
                Color color = Color.FromArgb((255 * d).Round(),0,0);
                i.Color = color;
                d += 0.01;
                Point[] screenCoord = new Point[3];
                for (int k = 0; k < 3; k++)
                {
                    Vector L = ToLocalCoord(i.ConnectedVertices[k].Coord);      //local coords
                    double temp = L.Z * TanHorizontalFOV;
                    int Sx = (((temp - L.X) / temp) * (Screen.Width / 2.0)).Round();                   //Screen coords

                    temp = L.Z * TanVerticalFOV;
                    int Sy = (((temp - L.Y) / temp) * (Screen.Height / 2.0)).Round();
                    screenCoord[k] = new Point(Sx + Screen.X, Sy + Screen.Y);
                }
                g.FillPolygon(new SolidBrush(i.Color), screenCoord);
            }
        }

        private Vector ToLocalCoord(Vector globalCoord)
        {
            Vector localCoord = globalCoord - Player.Coord;      //translation

            //rotation transformation
            double NewX, NewY, NewZ;
            double x = localCoord.X;
            double y = localCoord.Y;
            double z = localCoord.Z;
            double cosH = Player.DirectionVector.CosH;
            double sinH = Player.DirectionVector.SinH;
            double cosV = Player.DirectionVector.CosV;
            double sinV = Player.DirectionVector.SinV;

            NewX = (x * cosH) + (z * sinH);
            NewY = (x * sinV * sinH) + (y * cosV) - (z * sinV * cosH);
            NewZ = -(x * cosV * sinH) + (y * sinV) + (z * cosV * cosH);

            return new Vector(NewX, NewY, NewZ);
        }
    }

    class Player
    {
        public Vector Coord;
        public SphericalVec DirectionVector;
        public Player(Vector coord, SphericalVec directionVector)
        {
            Coord = coord;
            DirectionVector = directionVector;
            if (!DirectionVector.EvokeSinCosCalculation)
                DirectionVector.EvokeSinCosCalculation = true;
        }

        public Player(Vector coord)
        {
            Coord = coord;
            DirectionVector = new SphericalVec(0, 0, 1, evokeSinCos:true);
        }
    }



}
