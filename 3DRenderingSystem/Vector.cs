using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DRenderingSystem
{
    struct Vector
    {
        public double X {get; set;}
        public double Y {get; set;}
        public double Z {get; set;}

        public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);
        public Vector UnitVector => this / Magnitude;
		
        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
		
		public double this[int i]
        {
            get => 
				(i == 0) ? X : 
				(i == 1) ? Y : 
				(i == 2) ? Z : 
				throw new ArgumentOutOfRangeException($"Index = {i}");
            
            set
			{
				if (i == 0) X = value;
                else if (i == 1) Y = value;
                else if (i == 2) Z = value;
                else throw new ArgumentOutOfRangeException("i");
			}
        }
		
        #region 'To' Methods
        public override string ToString() => $"<{X}, {Y}, {Z}>";
		
        public string ToString(int decimalPlaces, bool trailingZerosAfterDecimal = false)
        {
			if (trailingZerosAfterDecimal)
                return $"<{X.ToString(decimalPlaces, true)}, {Y.ToString(decimalPlaces, true)}, {Z.ToString(decimalPlaces, true)}>";
            else
                return $"<{X.ToString(decimalPlaces)}, {Y.ToString(decimalPlaces)}, {Z.ToString(decimalPlaces)}>";
        }
		
        public SphericalVec ToSphVec() => new SphericalVec(X, Y, Z);
        #endregion

        #region Operators
        public static Vector operator +(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector operator -(Vector a, Vector b) => new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector operator -(Vector a) => new Vector(-a.X, -a.Y, -a.Z);

        public static Vector operator *(Vector a, double b) => new Vector(a.X * b, a.Y * b, a.Z * b);
			
        public static Vector operator *(double a, Vector b) => new Vector(b.X * a, b.Y * a, b.Z * a);

        public static Vector operator /(Vector a, double b) => new Vector(a.X / b, a.Y / b, a.Z / b);

        public static double operator %(Vector a, Vector b) => DotProduct(a, b);

        public static Vector operator &(Vector a, Vector b) => CrossProduct(a, b);
		
		public static double DotProduct(Vector a, Vector b) 
			=> a.X * b.X + a.Y * b.Y + a.Z * b.Z;
		public static Vector CrossProduct(Vector a, Vector b) 
			=> new Vector(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        #endregion
    }


    struct SphericalVec
    {
        public double R { get; set; }
        public double CosH { get; set; }
        public double SinH { get; set; }
        public double CosV { get; set; }
        public double SinV { get; set; }
        private double horiAngRad;
        private double vertAngRad;
        private bool evokeSinCosCalculation;
		
        #region Properties
        public bool EvokeSinCosCalculation
        {
            get => evokeSinCosCalculation;
            set
            {
                if (!value)
					evokeSinCosCalculation = false;
				else
                {
                    evokeSinCosCalculation = true;
                    CalculateSinCos();
                }
            }
        }

        public double HoriAngRad            // Azimuthal Horizontal Angle in Rad
        {
            get { return horiAngRad; }
            set
            {
                double correctedValue = value;
                while (correctedValue < 0)
                    correctedValue += 2 * Math.PI;
                while (correctedValue > 2 * Math.PI)
                    correctedValue -= 2 * Math.PI;
                horiAngRad = correctedValue;

                CalculateSinCos();
            }
        }
        public double VertAngRad            // Polar/Zenith Vertical Angle in Rad
        {
            get { return vertAngRad; }
            set                             
            {
                if (value >= 0 && value <= Math.PI)     // Limited to [0,2Pi or 180] so that players cant look beyond 90 up and 90 down
                    vertAngRad = value;

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
                //if (i % 2 == 1)     //  i is odd
                //    HoriAngRad += Math.PI;
                CalculateSinCos();
            }
        }
        
        public double HoriAngDeg                // Range = [0,360]
        {
            get => (180 / Math.PI) * HoriAngRad;
            set => HoriAngRad = (Math.PI / 180) * value;
        }
        public double VertAngDeg              // Range = [0,180]
        {
            get => (180 / Math.PI) * VertAngRad;
            set => VertAngRad = (Math.PI / 180) * value;
        }

        public double X
        {
            get => - R * Math.Sin(VertAngRad) * Math.Sin(HoriAngRad);       //*** some problems, shldnt be negative
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
            get => R * Math.Cos(VertAngRad);
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
        #endregion
		
        #region Constructors
        public SphericalVec(double x, double y, double z, bool evokeSinCos = false) : this()
        {
            R = Math.Sqrt(x * x + y * y + z * z);
            EvokeSinCosCalculation = false;
            VertAngRad = Math.Acos(y / R);
            HoriAngRad = - Math.Atan2(x, z);
            if (evokeSinCos)
                EvokeSinCosCalculation = true;
        }

        public SphericalVec(double r, double horiAng, double vertAng, bool rad, bool evokeSinCos = false) : this()
        {
            R = r;
            evokeSinCosCalculation = false;
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
        #endregion
		
        #region Methods
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
                if(vertAngOffset != 0)
                    Output.VertAngDeg += vertAngOffset;
            }
            
            if (evokeSinCos)
                Output.EvokeSinCosCalculation = true;
            return Output;
        }
            #region 'To' Methods
		public override string ToString() => $"R={R}, Hori. Angle={HoriAngDeg}°, Vert. Angle={VertAngDeg}°";
		
        public string ToString(int decimalPlaces, bool trailingZerosAfterDecimal = false)
        {
            if (trailingZerosAfterDecimal)
                return $"R={R.ToString(decimalPlaces, true)}, Hori. Angle={HoriAngDeg.ToString(decimalPlaces, true)}°, Vert. Angle={VertAngDeg.ToString(decimalPlaces, true)}°";
            else
                return $"R={R.ToString(decimalPlaces)}, Hori. Angle={HoriAngDeg.ToString(decimalPlaces)}°, Vert. Angle={VertAngDeg.ToString(decimalPlaces)}°";
        }
		
		public Vector ToVector() => new Vector(X, Y, Z);
	        #endregion
		
        #endregion
    }

}
