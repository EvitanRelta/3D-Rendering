using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DRenderingSystem
{
    struct Vector
    {
        public double X;
        public double Y;
        public double Z;

        public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);
        public Vector UnitVector => this / Magnitude;

        // Constructors
        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        // For Convenience
        public override string ToString()
            { return $"<{X}, {Y}, {Z}>"; }
        public string ToString(int decimals, bool alwaysXDecimals = false)
        {
            if (alwaysXDecimals)
                return $"<{X.ToString(decimals, true)}, {Y.ToString(decimals, true)}, {Z.ToString(decimals, true)}>";
            else
                return $"<{X.ToString(decimals)}, {Y.ToString(decimals)}, {Z.ToString(decimals)}>";
        }
            
        public SphericalVec ToSphVec()
        {
            return new SphericalVec(X, Y, Z);
        }
            
        // Indexer
        public double this[int index]
        {
            get
            {
                if (index == 0) return X;
                else if (index == 1) return Y;
                else if (index == 2) return Z;
                else throw new ArgumentOutOfRangeException("index");
            }
            set
            {
                if (index == 0) X = value;
                else if (index == 1) Y = value;
                else if (index == 2) Z = value;
                else throw new ArgumentOutOfRangeException("index");
            }
        }

        // Operators
        public static Vector operator +(Vector a, Vector b)
            { return new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }

        public static Vector operator -(Vector a, Vector b)
            { return new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }

        public static Vector operator -(Vector a)           // Negative of Vector
            { return new Vector(-a.X, -a.Y, -a.Z); }

        public static Vector operator *(Vector a, double b)
            { return new Vector(a.X * b, a.Y * b, a.Z * b); }
        public static Vector operator *(double a, Vector b)
            { return new Vector(b.X * a, b.Y * a, b.Z * a); }

        public static Vector operator /(Vector a, double b)
            { return new Vector(a.X / b, a.Y / b, a.Z / b); }

        public static double operator %(Vector a, Vector b)     // Dot Product
            { return a.X * b.X + a.Y * b.Y + a.Z * b.Z; }

        public static Vector operator &(Vector a, Vector b)     // Cross Product
            { return new Vector(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X); }
    }


    struct SphericalVec
    {
        private double R;
        private double _HoriAngRad;
        private double _VertAngRad;
        private bool _EvokeSinCosCalculation;
        public double CosH;
        public double SinH;
        public double CosV;
        public double SinV;
        public bool EvokeSinCosCalculation
        {
            get { return _EvokeSinCosCalculation; }
            set
            {
                if (value)
                {
                    _EvokeSinCosCalculation = true;
                    CalculateSinCos();
                }
                else
                    _EvokeSinCosCalculation = false;
            }
        }


        public double HoriAngRad            // Azimuthal Horizontal Angle in Rad
        {
            get { return _HoriAngRad; }
            set
            {
                double correctedValue = value;
                while (correctedValue < 0)
                    correctedValue += 2 * Math.PI;
                while (correctedValue > 2 * Math.PI)
                    correctedValue -= 2 * Math.PI;
                _HoriAngRad = correctedValue;

                CalculateSinCos();
            }
        }
            
        public double VertAngRad            // Polar/Zenith Vertical Angle in Rad
        {
            get { return _VertAngRad; }
            set                             
            {
                if (value >= 0 && value <= Math.PI)     // Limited to [0,2Pi or 180] so that players cant look beyond 90 up and 90 down
                    _VertAngRad = value;

                //double correctedValue = value;        // Actual calculation for spherical coord vert. angle
                //int i = 0;
                //while (correctedValue > Math.PI || correctedValue < 0)
                //{
                //    if (value < 0)
                //    {
                //        correctedValue = -correctedValue;
                //        i++;
                //    }
                //    if (correctedValue > Math.PI)
                //    {
                //        correctedValue = 2 * Math.PI - correctedValue;
                //        i++;
                //    }
                //}
                //if (i % 2 == 1)     // if i is odd
                //    HoriAngRad += Math.PI;
                CalculateSinCos();
            }
        }
        
        public double HoriAngDeg                // Range = [0,360]
        {
            get { return (180 / Math.PI) * HoriAngRad; }
            set { HoriAngRad = (Math.PI / 180) * value; }
        }
        public double VertAngDeg              // Range = [0,180]
        {
            get { return (180 / Math.PI) * VertAngRad; }
            set { VertAngRad = (Math.PI / 180) * value; }
        }

        public double X
        {
            get { return - R * Math.Sin(VertAngRad) * Math.Sin(HoriAngRad); }       //*** some problems, shldnt be negative
            set
            {
                double NewR = Math.Sqrt(value * value + Y * Y + Z * Z);
                double NewVertAngRad = Math.Acos(Y / NewR);
                double NewHoriAngRad = -Math.Atan2(value, Z);

                R = NewR;
                VertAngRad = NewVertAngRad;
                HoriAngRad = NewVertAngRad;
            }
        }
        public double Y
        {
            get { return R * Math.Cos(VertAngRad); }
            set
            {
                double NewR = Math.Sqrt(X * X + value * value + Z * Z);
                double NewVertAngRad = Math.Acos(value / NewR);
                double NewHoriAngRad = Math.Atan2(X, Z);

                R = NewR;
                VertAngRad = NewVertAngRad;
                HoriAngRad = NewHoriAngRad;
            }
        }
        public double Z
        {
            get { return R * Math.Sin(VertAngRad) * Math.Cos(HoriAngRad); }
            set
            {
                double NewR = Math.Sqrt(X * X + Y * Y + value * value);
                double NewVertAngRad = Math.Acos(Y / NewR);
                double NewHoriAngRad = Math.Atan2(X, value);

                R = NewR;
                VertAngRad = NewVertAngRad;
                HoriAngRad = NewHoriAngRad;
            }
        }

        // Constructors
        public SphericalVec(double x, double y, double z, bool evokeSinCos = false) : this()
        {
            R = Math.Sqrt(x * x + y * y + z * z);
            _EvokeSinCosCalculation = false;
            VertAngRad = Math.Acos(y / R);
            HoriAngRad = - Math.Atan2(x, z);
            if (evokeSinCos)
                EvokeSinCosCalculation = true;
        }

        public SphericalVec(double r, double horiAng, double vertAng, bool rad, bool evokeSinCos = false) : this()
        {
            R = r;
            _EvokeSinCosCalculation = false;
            if (rad)
            {
                HoriAngRad = horiAng;
                VertAngRad = vertAng;
            }
            else
            {
                HoriAngDeg = horiAng;
                VertAngDeg = vertAng;
            }
            if (evokeSinCos)
                EvokeSinCosCalculation = true;
        }

        // Methods
        public void CalculateSinCos()
        {
            if (EvokeSinCosCalculation)
            {
                SinH = Math.Sin(HoriAngRad);
                CosH = Math.Cos(HoriAngRad);

                SinV = Math.Sin((Math.PI / 2) - VertAngRad);
                CosV = Math.Cos((Math.PI / 2) - VertAngRad);
            }
        }

        public Vector ToVector()
            { return new Vector(X, Y, Z); }

        public SphericalVec ReturnOffset(double rOffset, double horiAngOffset, double vertAngOffset, bool rad = false, bool evokeSinCos = false)   // Returns new SphericalVec with its values offset
        {
            SphericalVec Output = new SphericalVec(R + rOffset, HoriAngRad, VertAngRad, rad:true, evokeSinCos:false);
            if (rad)
            {
                if (horiAngOffset != 0)
                    Output.HoriAngRad += horiAngOffset;
                if (vertAngOffset != 0)
                    Output.VertAngRad += vertAngOffset;
            }
            else
            {
                if (horiAngOffset != 0)
                    Output.HoriAngDeg += horiAngOffset;
                if (vertAngOffset != 0)
                    Output.VertAngDeg += vertAngOffset;
            }
            
            if (evokeSinCos)
                Output.EvokeSinCosCalculation = true;
            return Output;
        }
    }





}
