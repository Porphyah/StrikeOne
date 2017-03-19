using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrikeOne.Core.Lua
{
    class LuaMath
    {
        public const double PI = System.Math.PI;
        public const double E = System.Math.E;
        public double Sin(double Value) { return System.Math.Sin(Value); }
        public double Cos(double Value) { return System.Math.Cos(Value); }
        public double Tan(double Value) { return System.Math.Tan(Value); }
        public double ArcSin(double Value) { return System.Math.Asin(Value); }
        public double ArcCos(double Value) { return System.Math.Acos(Value); }
        public double ArcTan(double Value) { return System.Math.Atan(Value); }
        public double ArcTan(double Y, double X) { return System.Math.Atan2(Y, X); }
        public double Log(double Value, double Base) { return System.Math.Log(Value, Base); }
        public double Ln(double Value) { return System.Math.Log(Value); }
        public double Pow(double Value, double Exp) { return System.Math.Pow(Value, Exp); }
        public double Exp(double Value) { return System.Math.Exp(Value); }
        public double Abs(double Value) { return System.Math.Abs(Value); }
        public double Max(params double[] Values) { return Values.Max(); }
        public double Min(params double[] Values) { return Values.Min(); }
    }
}
