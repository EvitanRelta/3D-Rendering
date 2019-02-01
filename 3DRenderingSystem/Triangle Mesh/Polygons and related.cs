using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace _3DRenderingSystem
{
    class Vertex
    {
        public Vector Coord;
        public HashSet<Face> ConnectedFaces = new HashSet<Face>();

        public Vertex(Vector Coordinate)
            { Coord = Coordinate; }
        public Vertex(double x, double y, double z)
            { Coord = new Vector(x, y, z); }
    }

    class Face
    {
        public Vertex[] ConnectedVertices;
        public Polygon ParentPolygon;
        public Color Color;

        public Vector DirectionVector
        {
            get
            {
                Vector AB = ConnectedVertices[1].Coord - ConnectedVertices[0].Coord;
                Vector AC = ConnectedVertices[2].Coord - ConnectedVertices[0].Coord;
                return (AB & AC).UnitVector;
            }
        }

        public Vector MiddlePoint
        {
            get
            {
                double x = (ConnectedVertices[0].Coord.X + ConnectedVertices[1].Coord.X + ConnectedVertices[2].Coord.X) / 3;
                double y = (ConnectedVertices[0].Coord.Y + ConnectedVertices[1].Coord.Y + ConnectedVertices[2].Coord.Y) / 3;
                double z = (ConnectedVertices[0].Coord.Z + ConnectedVertices[1].Coord.Z + ConnectedVertices[2].Coord.Z) / 3;
                return new Vector(x, y, z);
            }
        }

        public Face(Vertex a, Vertex b, Vertex c)
        {
            ConnectedVertices = new Vertex[3] { a, b, c };
            a.ConnectedFaces.Add(this);
            b.ConnectedFaces.Add(this);
            c.ConnectedFaces.Add(this);
            Color = Color.White;
        }

        public Face(Vertex a, Vertex b, Vertex c, Color color)
        {
            ConnectedVertices = new Vertex[3] { a, b, c };
            a.ConnectedFaces.Add(this);
            b.ConnectedFaces.Add(this);
            c.ConnectedFaces.Add(this);
            Color = color;
        }


        public double DistFromMidPt(Player player)
        {
            return (player.Coord - this.MiddlePoint).Magnitude;
        }

        public void Subdivide()
        {
            //***
        }
    }

    class Polygon
    {
        public HashSet<Face> Faces;
        public HashSet<Vertex> Vertices = new HashSet<Vertex>();

        public Polygon(HashSet<Face> faces)
        {
            Faces = faces;
            foreach (Face i in faces)
            {
                Vertices.UnionWith(i.ConnectedVertices);
                i.ParentPolygon = this;
            }
                
                
        }
        
        public void Color(Color color)
        {
            foreach (Face i in Faces)
                i.Color = color;
        }

        public void Shift(Vector shift)
        {
            foreach (Vertex i in Vertices)
                i.Coord += shift;
        }
    }

    static class Polygons
    {
        public static Polygon Cube(double width, Vector position)
        {
            double x = position.X;
            double y = position.Y;
            double z = position.Z;

            List<Vertex> v = new List<Vertex>           // Declare vertices
            {
                new Vertex(x, y, z),                            //  0
                new Vertex(x + width, y, z),                    //  1
                new Vertex(x + width, y + width, z),             //  2
                new Vertex(x, y + width, z),                    //  3
                new Vertex(x, y, z + width),                    //  4
                new Vertex(x + width, y, z + width),            //  5
                new Vertex(x + width, y + width, z + width),    //  6
                new Vertex(x, y + width, z + width),            //  7
            };
            HashSet<Face> faces = new HashSet<Face>();                // Declare faces
            faces.Add(new Face(v[0], v[1], v[3]));
            faces.Add(new Face(v[2], v[1], v[3]));
            faces.Add(new Face(v[4], v[5], v[7]));
            faces.Add(new Face(v[6], v[5], v[7]));
            faces.Add(new Face(v[0], v[1], v[4]));
            faces.Add(new Face(v[5], v[1], v[4]));
            faces.Add(new Face(v[3], v[2], v[7]));
            faces.Add(new Face(v[6], v[2], v[7]));
            faces.Add(new Face(v[0], v[3], v[4]));
            faces.Add(new Face(v[7], v[3], v[4]));
            faces.Add(new Face(v[1], v[2], v[5]));
            faces.Add(new Face(v[6], v[2], v[5]));

            return new Polygon(faces);
        }
    }
}
