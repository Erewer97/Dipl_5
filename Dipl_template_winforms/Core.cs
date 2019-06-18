using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform.Windows;
using System.Xml;

namespace Dipl_template_winforms
{
    public enum TypeFigures { None, Line, Rect, Circle, Curve, Polygon, Ellipsoid }
    public enum Operations { None, Union, Interset, Sub }
    public enum ActionWithFigure { None, Move, Rotate, Scale }
    public enum SelectingMode { Points, Edges }

    public static class MathVec
    {
        public static Vector2d AbsSub(Vector2d a, Vector2d b)
        {
            return new Vector2d(Math.Abs((a - b).X), Math.Abs((a - b).Y));
        }
        public static bool CompareLenSquared(Vector2d v, double c)
        {
            if (v.LengthSquared <= c)
                return true;
            return false;
        }
        public static bool DCompare(double a, double b)
        {
            if (Math.Abs(a - b) <= 0.001)
                return true;
            return false;
        }
        public static bool VectrCompare(Vector2d a, Vector2d b, Vector2d p)
        {
            double mx, my, minx, miny;
            if (a.X <= b.X) { mx = b.X; minx = a.X; } else { mx = a.X; minx = b.X; };
            if (a.Y <= b.Y) { my = b.Y; miny = a.Y; } else { my = a.Y; miny = b.Y; };

            if (minx <= p.X && p.X <= mx && miny <= p.Y && p.Y <= my)
                return true;
            else
            {
                bool bX = DCompare(minx, p.X) && DCompare(mx, p.X);
                bool bY = DCompare(miny, p.Y) && DCompare(my, p.Y);

                if (bX)
                {
                    if (miny <= p.Y && p.Y <= my)
                        return true;
                    else if (bY)
                        return true;
                    else
                        return false;
                }
                else if (bY)
                {
                    if (minx <= p.X && p.X <= mx)
                        return true;
                    else if (bX)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        public static bool PointOnEdge(Vector2d a, Vector2d b, Vector2d p)
        {
            if (VectrCompare(a, b, p))
            {
                double x = p.X - a.X;
                double y = p.Y - a.Y;

                double xz = b.X - a.X;
                if (Math.Abs(xz) < 0.01)
                    if ((a.Y <= p.Y && p.Y <= b.Y) || (a.Y >= p.Y && p.Y >= b.Y))
                        return true;
                    else
                        return false;

                double yz = b.Y - a.Y;
                if (Math.Abs(yz) < 0.01)
                    if ((a.X <= p.X && p.X <= b.X) || (a.X >= p.X && p.X >= b.X))
                        return true;
                    else
                        return false;

                double X = x / xz;
                double Y = y / yz;

                if (Math.Abs(X - Y) < 0.01)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public static bool VectrCompare(Vector2d a, Vector2d b)
        {
            Vector2d v = AbsSub(a, b);

            if (v.X <= 0.001 && v.Y <= 0.001)
                return true;
            else
                return false;
        }
        public static bool VectrCompare(Vector2d a, Vector2d b, double c)
        {
            Vector2d v = AbsSub(a, b);

            if (v.X <= c && v.Y <= c)
                return true;
            else
                return false;
        }
        public static bool DoubEquale(double a, double b, double e)
        {
            if (Math.Abs(a - b) <= 4 * e * Math.Max(Math.Abs(a), Math.Abs(b)))
                return true;
            return false;
        }

        public static Vector2d LinesIntersection(Vector2d x1, Vector2d y1, Vector2d x2, Vector2d y2)
        {
            Vector2d r = new Vector2d(double.NaN);

            Vector2d
            a = x1,
            b = y1,
            c = x2,
            d = y2;

            double D = (a.X - b.X) * (d.Y - c.Y) - (d.X - c.X) * (a.Y - b.Y);
            if (D == 0) // отрезки парралельны
                //throw new Exception("Линии паралельны");
                return r;

            double u = ((d.X - b.X) * (d.Y - c.Y) - (d.X - c.X) * (d.Y - b.Y)) / D;

            double v = ((a.X - b.X) * (d.Y - b.Y) - (d.X - b.X) * (a.Y - b.Y)) / D;

            //if (u <= double.MinValue && v <= double.MinValue)
            //throw new Exception("Линии совпадают"); 

            if ((0 <= u) && (u <= 1) && (0 <= v) && (v <= 1))
            {
                double x = u * a.X + (1 - u) * b.X;
                double y = u * a.Y + (1 - u) * b.Y;
                r = new Vector2d(x, y);
            }

            //Console.WriteLine(D + "\n" + u + "\n" + v);

            return r;
        }
        public static bool Hit(Vector2d v, List<Vector2d> Verteces)
        {
            Vector2d Point = v;
            bool result = false;
            int j = Verteces.Count - 1;


            for (int i = 0; i < Verteces.Count; i++)
            {
                if (
                    (Verteces[i].Y < Point.Y && Verteces[j].Y >= Point.Y
                    ||
                    Verteces[j].Y < Point.Y && Verteces[i].Y >= Point.Y)
                    &&
                     (Verteces[i].X + (Point.Y - Verteces[i].Y) / (Verteces[j].Y - Verteces[i].Y) * (Verteces[j].X - Verteces[i].X) < Point.X))
                    result = !result;
                j = i;
            }

            return result;
        }
    }

    public class AABB
    {
        public Vector2d Max { get; set; } = new Vector2d(double.NaN);
        public Vector2d Min { get; set; } = new Vector2d(double.NaN);
        public Vector2d Center { get; set; } = new Vector2d(double.NaN);

        public AABB() { ; }
        public AABB(List<Vector2d> verteces)
        {
            double maxx = verteces[0].X,
                       maxy = verteces[0].Y,
                       minx = verteces[0].X,
                       miny = verteces[0].Y;

            foreach (var r in verteces)
            {
                if ((r.X >= maxx))
                    maxx = r.X;
                if ((r.X <= minx))
                    minx = r.X;
                if ((r.Y >= maxy))
                    maxy = r.Y;
                if ((r.Y <= miny))
                    miny = r.Y;
            }

            Max = new Vector2d(maxx, maxy);
            Min = new Vector2d(minx, miny);

            Center = ((Min + Max) / 2.0);
        }

        public bool HitInAABB(Vector2d v)
        {
            if (Min.X <= v.X && v.X <= Max.X && Min.Y <= v.Y && v.Y <= Max.Y)
                return true;
            return false;
        }
        public void Calc(List<Vector2d> verteces)
        {
            if (verteces.Count > 0)
            {
                double maxx = verteces[0].X,
                       maxy = verteces[0].Y,
                       minx = verteces[0].X,
                       miny = verteces[0].Y;

                foreach (var r in verteces)
                {
                    if ((r.X >= maxx))
                        maxx = r.X;
                    if ((r.X <= minx))
                        minx = r.X;
                    if ((r.Y >= maxy))
                        maxy = r.Y;
                    if ((r.Y <= miny))
                        miny = r.Y;
                }

                Max = new Vector2d(maxx, maxy);
                Min = new Vector2d(minx, miny);

                Center = ((Min + Max) / 2.0);
            }
        }
        public void Draw()
        {
            GL.PushMatrix();
            GL.Begin(BeginMode.Points);
            GL.PointSize(5f);
            GL.Vertex2(Max);
            GL.Vertex2(Min);
            GL.Vertex2(Center);
            GL.Vertex2(Max.X, Min.Y);
            GL.Vertex2(Min.X, Max.Y);
            GL.End();
            GL.PopMatrix();
        }
    }

    public class Edge
    {
        public Vector2d Begin { get; set; }
        public Vector2d End { get; set; }
        public Vector2d BeginControlPoint { get; set; } = new Vector2d(double.NaN);
        public Vector2d EndControlPoint { get; set; } = new Vector2d(double.NaN);
        public bool IsBezie { get; set; } = false;
        public int BezieSegments { get; set; } = 32;
        public Vector2d[] BeziePoints { get { return _bezieVerteces; } private set { } }

        Vector2d[] _bezieVerteces = new Vector2d[32 - 1];

        public Edge() {; }
        public Edge(Vector2d begin, Vector2d end)
        {
            Begin = begin; End = end;
        }
        public Edge(Vector2d begin, Vector2d end, Vector2d cp1, Vector2d cp2)
        {
            Begin = begin; End = end; BeginControlPoint = cp1; EndControlPoint = cp2; IsBezie = true; CalcBeziePoints();
        }

        public Vector2d this[int index]
        {
            get
            {
                if (index == 0) return Begin;
                if (index == 1) return End;
                if (index == 2) return BeginControlPoint;
                if (index == 3) return EndControlPoint;
                throw new Exception("Incorrect index"); ;
            }
            set
            {
                if (index == 0) Begin = value;
                if (index == 1) End = value;
                if (index == 2) BeginControlPoint = value;
                if (index == 3) EndControlPoint = value;
                if (index < 0 || index > 4) throw new Exception("Incorrect index"); ;
            }
        }

        /// <summary>
        /// Функция, проверяющая, расположены ли точки против часовой стрелки
        /// </summary>
        /// <returns>Возвращает true если против часовой, иначе false</returns>
        public bool IsTrueTrend(Vector2d centerPoint)
        {
            Vector2d line = End - Begin;
            Vector2d normal = centerPoint - Begin;

            double r = (line.X * normal.Y) - (line.Y * normal.X);

            if (r > 0.0)
                return true;

            return false;
        }
        double LenToLine(Vector2d a, Vector2d b, Vector2d p)
        {
            double ch1 = (p.X - a.X) * (b.X - a.X);
            double ch2 = (p.Y - a.Y) * (b.Y - a.Y);
            double znamenatel = (b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y);

            double t = (ch1 + ch2) / znamenatel;
            if (Math.Abs(t) <= 0.5)
                return 1;
            return -1;
        }
        public void CalcBeziePoints()
        {
            BezierCurveCubic b = new BezierCurveCubic((Vector2)Begin, (Vector2)End, (Vector2)BeginControlPoint, (Vector2)EndControlPoint);
            if (IsBezie == false) IsBezie = true;

            for (int i = 1; i <= BezieSegments - 1; i++)
                _bezieVerteces[i - 1] = (Vector2d)b.CalculatePoint(i / (float)BezieSegments);
        }
        public void ConvertToBezie()
        {
            if (IsBezie == false)
            {
                IsBezie = true;
                Vector2d v = (Begin - End) / 4;

                EndControlPoint = End + v;
                BeginControlPoint = Begin - v;

                CalcBeziePoints();
            }
        }
        public void ConvertToLine()
        {
            if (IsBezie)
            {
                EndControlPoint = BeginControlPoint = new Vector2d(double.NaN);
                _bezieVerteces = new Vector2d[31];
                IsBezie = false;
            }
        }
        public Edge PointAtEdge(Vector2d point)
        {
            double Y = 0, X = 0, D = 0;
            if (IsBezie)
            {
                if (_bezieVerteces.Length > 0)
                {
                    Vector2d p1;
                    Vector2d p2;
                    for (int i = 0; i <= _bezieVerteces.Length; i++)
                    {
                        if (i == 0)
                        {
                            p1 = Begin;
                            p2 = _bezieVerteces[i + 1];
                        }
                        else if (i == _bezieVerteces.Length)
                        {
                            p1 = _bezieVerteces[i - 1];
                            p2 = End;
                        }
                        else
                        {
                            p1 = _bezieVerteces[i - 1];
                            p2 = _bezieVerteces[i];
                        }

                        if (LenToLine(p1, p2, point) == 1)
                            return this;
                    }
                }
            }
            else
            {
                Y = Begin.Y - End.Y;
                X = End.X - Begin.X;

                if ((Math.Abs(X) < 0.01) && (Math.Abs(point.X - Begin.X) < 0.01))
                    if ((point.Y < Begin.Y) && (End.Y < point.Y))
                        return this;
                    else
                        return null;

                D = Begin.X * End.Y - End.X * Begin.Y;

                double res = Y * point.X + X * point.Y + D;
                if (Math.Abs(res) < 0.1)
                    return this;
                //double x = point.X - Begin.X;
                //double y = point.Y - Begin.Y;

                //double z1 = End.X - Begin.X;
                //if (Math.Abs(z1) < 0.1)
                //    if ((point.Y < Begin.Y) && (End.Y < point.Y))
                //        return this;
                //    else
                //        return null;

                //double z2 = End.Y - Begin.Y;
                //if (Math.Abs(z2) < 0.1)
                //    if ((point.X < Begin.X) && (End.X < point.X))
                //        return this;
                //    else
                //        return null;

                //double l1 = x / z1;
                //double l2 = y / z2;

                //if (0.0 < l1 && l1 < 1.0 && 0.0 < l2 && l2 < 1.0)
                //    return this;
                //else
                //    return null;
            }
            return null;
        }
        public Vector2d MaxInEdge()
        {
            Vector2d r = new Vector2d(double.NegativeInfinity);

            r = Vector2d.Max(Begin, End);

            foreach (var t in _bezieVerteces)
                if ((t.X >= r.X) && (t.Y >= r.Y))
                    r = t;

            return r;
        }
        public Vector2d MinInEdge()
        {
            Vector2d r = new Vector2d(double.PositiveInfinity);

            r = Vector2d.Min(Begin, End);

            foreach (var t in _bezieVerteces)
                if ((t.X <= r.X) && (t.Y <= r.Y))
                    r = t;

            return r;
        }

        public static Edge operator -(Edge edge, Vector2d point)
        {
            if (edge.IsBezie)
                return new Edge(
                    edge.Begin - point,
                    edge.End - point,
                    edge.BeginControlPoint - point,
                    edge.EndControlPoint - point
                    );
            return new Edge(
                edge.Begin - point,
                edge.End - point
                );
        }
        public static Edge operator +(Edge edge, Vector2d point)
        {
            if (edge.IsBezie)
                return new Edge
                    (
                        edge.Begin + point,
                        edge.End + point,
                        edge.BeginControlPoint + point,
                        edge.EndControlPoint + point
                    );
            return new Edge(
                edge.Begin + point,
                edge.End + point
                );
        }
        public override string ToString()
        {
            return "Begin: " + Begin.ToString() + "\nEnd:   " + End.ToString() + "\n" +
                "CP1:  " + BeginControlPoint.ToString() + "\n" +
                "CP2:  " + EndControlPoint.ToString() + "\n";
        }
    }

    public class Figure
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool IsClosed { get; set; } = true; // Замкнута ли фигура?
        public Vector2d MoveTo { get; set; } = new Vector2d(0);
        public Vector2d ScaleTo { get; set; } = new Vector2d(1.0);
        public double Angle { get; set; } = 0;
        public List<Edge> Edges { get; set; } = new List<Edge>();
        public Vector2d[] Manipulators { get; set; } = new Vector2d[9];
        public List<Vector2d> Verteces { get; set; } = new List<Vector2d>();
        public Vector2d Center { get; set; }
        public bool IsDrawPoint { get; set; }
        public ActionWithFigure LabelOFAction { get; private set; }
        public TypeFigures Type { get; set; } = TypeFigures.None;
        public bool IsSelect { get; set; }
        public bool IsEdit { get; set; } // Редактируют ли сейчас эту фигуру?
        public bool IsRender { get; set; } = true; // Правда - если надо отрисовать, иначе ложь
        public bool IsDrawCenter { get; set; }
        public Color FillColor { get; set; }
        public Color BorderColor { get; set; }
        public float LineWidth { get; set; } = 1.0f;
        public List<Triangle> Triangles { get { return _triangles; } private set {; } }
        public AABB AABB { get; set; } = new AABB();

        int indPoint = -1;
        Matrix4d TRS = new Matrix4d();
        Matrix4d TRSI = new Matrix4d();
        List<Edge> mainFigure = new List<Edge>();
        Vector2d[] manipul = new Vector2d[9];
        Vector2d maxPointAABB, minPointAABB;
        private int indE1 = -1;
        private int indE2 = -1;
        int indP1 = -1, indP2 = -1;
        private int indPoint1 = -1;
        int indAroundScale = -1;
        private int indCurrEdge = -1;
        List<Triangle> _triangles = new List<Triangle>();
        private int indPoint2 = -1;

        public void ReCalc()
        {
            var t = Matrix4d.CreateTranslation(MoveTo.X, MoveTo.Y, 0);
            var r = Matrix4d.CreateRotationZ(MathHelper.DegreesToRadians(Angle));
            var s = Matrix4d.Scale(ScaleTo.X, ScaleTo.Y, 1.0);

            var rt = Matrix4d.Mult(r, t);
            TRS = Matrix4d.Mult(s, rt);

            TRSI = TRS;
            TRSI.Transpose();

            Edges = mainFigure.ToList();
            Manipulators = manipul.ToArray();

            for (int i = 0; i < Edges.Count; i++)
                Edges[i] = MultiplyMatrixAndEdge(mainFigure[i], TRSI);

            for (int i = 0; i < Manipulators.Length; i++)
                Manipulators[i] = MultiplyMatrixAndVector(manipul[i], TRSI);

            Verteces.Clear();
            if (IsClosed)
            {
                for (int i = 0; i < Edges.Count; i++)
                {
                    if (!Edges[i].IsBezie)
                        Verteces.Add(Edges[i].Begin);
                    else
                    {
                        Verteces.Add(Edges[i].Begin);
                        for (int tt = 0; tt < Edges[i].BeziePoints.Length; tt++)
                            Verteces.Add(Edges[i].BeziePoints[tt]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < Edges.Count; i++)
                {
                    if (!Edges[i].IsBezie)
                        Verteces.Add(Edges[i].Begin);
                    else
                    {
                        Verteces.Add(Edges[i].Begin);
                        for (int tt = 0; tt < Edges[i].BeziePoints.Length; tt++)
                            Verteces.Add(Edges[i].BeziePoints[tt]);
                    }
                }
                Verteces.Add(Edges[Edges.Count - 1].End);
            }

            Triangulate tr = new Triangulate(Verteces.ToArray());
            _triangles = tr.Triangles;

            AABB.Calc(Verteces);
        }
        public Vector2d MultiplyMatrixAndVector(Vector2d Vector, Matrix4d m)
        {
            Vector4d v = new Vector4d(Vector.X, Vector.Y, 0, 1);

            Vector4d mRes = new Vector4d(
                m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z + m.M14 * v.W,
                m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z + m.M24 * v.W,
                m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z + m.M34 * v.W,
                m.M41 * v.X + m.M42 * v.Y + m.M43 * v.Z + m.M44 * v.W);

            return new Vector2d(mRes.X, mRes.Y);
        }
        public Edge MultiplyMatrixAndEdge(Edge e, Matrix4d m)
        {
            Edge re = new Edge();

            re.Begin = MultiplyMatrixAndVector(e.Begin, TRSI);
            re.End = MultiplyMatrixAndVector(e.End, TRSI);
            re.BeginControlPoint = MultiplyMatrixAndVector(e.BeginControlPoint, TRSI);
            re.EndControlPoint = MultiplyMatrixAndVector(e.EndControlPoint, TRSI);
            if (e.IsBezie)
                re.CalcBeziePoints();

            return re;
        }
        public void TranslateToCenterCoordinates()
        {
            MoveTo = Center;
            for (int i = 0; i < Edges.Count; i++)
                Edges[i] = Edges[i] - Center;

            if (Edges.Count > 0)
            {
                double maxx = Edges[0].Begin.X,
                       maxy = Edges[0].Begin.Y,
                       minx = Edges[0].Begin.X,
                       miny = Edges[0].Begin.Y;

                foreach (var r in Edges)
                {
                    if ((r.MaxInEdge().X >= maxx))
                        maxx = r.MaxInEdge().X;
                    if ((r.MinInEdge().X <= minx))
                        minx = r.MinInEdge().X;
                    if ((r.MaxInEdge().Y >= maxy))
                        maxy = r.MaxInEdge().Y;
                    if ((r.MinInEdge().Y <= miny))
                        miny = r.MinInEdge().Y;
                }

                maxPointAABB = new Vector2d(maxx, maxy);
                minPointAABB = new Vector2d(minx, miny);

                Manipulators[0] = maxPointAABB;
                Manipulators[1] = new Vector2d(maxPointAABB.X, (minPointAABB.Y + maxPointAABB.Y) / 2.0);
                Manipulators[2] = new Vector2d(maxPointAABB.X, minPointAABB.Y);

                Manipulators[3] = minPointAABB;
                Manipulators[4] = new Vector2d(minPointAABB.X, (minPointAABB.Y + maxPointAABB.Y) / 2.0);
                Manipulators[5] = new Vector2d(minPointAABB.X, maxPointAABB.Y);

                Manipulators[6] = new Vector2d((minPointAABB.X + maxPointAABB.X) / 2.0, minPointAABB.Y);
                Manipulators[7] = new Vector2d((minPointAABB.X + maxPointAABB.X) / 2.0, maxPointAABB.Y);

                Manipulators[8] = new Vector2d(0);
            }

            manipul = Manipulators.ToArray();
            mainFigure = Edges.ToList();

            Center = new Vector2d(0);
        }
        public void SetCenter(Vector2d NewCenter)
        {
            Center = NewCenter - MoveTo;
        }
        public bool IsTrueTrend()
        {
            if (mainFigure.Count > 0)
            {
                var e = Edges[0];
                return e.IsTrueTrend(Center);
            }
            return false;
        }
        public void SortAtClock()
        {
            if (!IsTrueTrend())
            {
                for (int i = 0; i < Edges.Count; i++)
                {
                    if (Edges[i].IsBezie)
                    { 
                        var e = new Edge(Edges[i].End, Edges[i].Begin, Edges[i].EndControlPoint, Edges[i].BeginControlPoint);
                        Edges[i] = e;
                    }
                    else
                    {
                        var e = new Edge(Edges[i].End, Edges[i].Begin);
                        Edges[i] = e;
                    }
                }

                Edges.Reverse();
                mainFigure = Edges.ToList();
                //for (int i = 0; i < Edges.Count; i++)
                //    mainFigure[i] = Edges[i] - MoveTo;
            }
        }
        // Hit in figure
        public bool Hit(Vector2d v)
        {
            Vector2d Point = v;
            bool result = false;
            int j = Verteces.Count - 1;

            if (Type != TypeFigures.Line && Type != TypeFigures.Curve)
                for (int i = 0; i < Verteces.Count; i++)
                {
                    if (
                        (Verteces[i].Y < Point.Y && Verteces[j].Y >= Point.Y
                        ||
                        Verteces[j].Y < Point.Y && Verteces[i].Y >= Point.Y)
                        &&
                         (Verteces[i].X + (Point.Y - Verteces[i].Y) / (Verteces[j].Y - Verteces[i].Y) * (Verteces[j].X - Verteces[i].X) < Point.X))
                        result = !result;
                    j = i;
                }
            else if (Type == TypeFigures.Line)
            {
                var y1 = Verteces[0].Y;
                var y2 = Verteces[1].Y;
                var x1 = Verteces[0].X;
                var x2 = Verteces[1].X;

                if (
                    ((y2 - y1) > 0.01 || (y2 - y1) < -0.01) &&
                    ((x2 - x1) > 0.01 || (x2 - x1) < -0.01) &&
                    (((y1 - y2) * Point.X + (x2 - x1) * Point.Y + (x1 * y2 - x2 * y1) < 0.05) &&
                    ((y1 - y2) * Point.X + (x2 - x1) * Point.Y + (x1 * y2 - x2 * y1) > -0.05))
                    )
                    return true;
            }
            else
            {
                return HitInBorder(v);
            }

            return result;

        }
        /// <summary>
        /// Проверка попали ли мы на ребро текущего объекта
        /// </summary>
        /// <param name="v">Позиция мыши</param>
        /// <returns>Ребро, если попали, иначе null</returns>
        public bool HitInBorder(Vector2d v)
        {
            Edge r = null;
            for (int i = 0; i < Edges.Count; i++)
                if ((r = Edges[i].PointAtEdge(v)) != null)
                {
                    indCurrEdge = i;
                    return true;
                }
            indCurrEdge = -1;
            return false;
        }
        public bool HitInPoint(Vector2d MousePos)
        {
            if (IsClosed)
            {
                for (int i = 0; i < Edges.Count; i++)
                {
                    Vector2d b = MathVec.AbsSub(MousePos, Edges[i].Begin);
                    Vector2d e = MathVec.AbsSub(MousePos, Edges[i].End);

                    if (i == 0)
                    { 
                        if (MathVec.CompareLenSquared(b, 0.01))
                        {
                            indE1 = 0; indE2 = Edges.Count - 1; indP1 = 0; indP2 = 1; return true;
                        }
                        if (MathVec.CompareLenSquared(e, 0.01))
                        {
                            indE1 = 0; indE2 = 1; indP1 = 1; indP2 = 0; return true;
                        }
                        if (Edges[i].IsBezie)
                        {
                            Vector2d cp1 = MathVec.AbsSub(MousePos, Edges[i][2]);
                            Vector2d cp2 = MathVec.AbsSub(MousePos, Edges[i][3]);

                            if (MathVec.CompareLenSquared(cp1, 0.01))
                            {
                                indE1 = 0; indE2 = 0; indP1 = 2; indP2 = 2; return true;
                            }
                            if (MathVec.CompareLenSquared(cp2, 0.01))
                            {
                                indE1 = 0; indE2 = 0; indP1 = 3; indP2 = 3; return true;
                            }
                        }
                    }
                    else if (i == Edges.Count - 1)
                    {
                        if (MathVec.CompareLenSquared(b, 0.01))
                        {
                            indE1 = Edges.Count - 1; indE2 = Edges.Count - 2; indP1 = 0; indP2 = 1; return true;
                        }
                        if (MathVec.CompareLenSquared(e, 0.01))
                        {
                            indE1 = Edges.Count - 1; indE2 = 0; indP1 = 1; indP2 = 0; return true;
                        }
                        if (Edges[i].IsBezie)
                        {
                            Vector2d cp1 = MathVec.AbsSub(MousePos, Edges[i][2]);
                            Vector2d cp2 = MathVec.AbsSub(MousePos, Edges[i][3]);

                            if (MathVec.CompareLenSquared(cp1, 0.01))
                            {
                                indE1 = Edges.Count - 1; indE2 = Edges.Count - 1; indP1 = 2; indP2 = 2; return true;
                            }
                            if (MathVec.CompareLenSquared(cp2, 0.01))
                            {
                                indE1 = Edges.Count - 1; indE2 = Edges.Count - 1; indP1 = 3; indP2 = 3; return true;
                            }
                        }
                    }
                    else
                    {
                        if (MathVec.CompareLenSquared(b, 0.01))
                        {
                            indE1 = i; indE2 = i + 1; indP1 = 0; indP2 = 1; return true;
                        }
                        if (MathVec.CompareLenSquared(e, 0.01))
                        {
                            indE1 = i; indE2 = i + 1; indP1 = 1; indP2 = 0; return true;
                        }
                        if (Edges[i].IsBezie)
                        {
                            Vector2d cp1 = MathVec.AbsSub(MousePos, Edges[i][2]);
                            Vector2d cp2 = MathVec.AbsSub(MousePos, Edges[i][3]);

                            if (MathVec.CompareLenSquared(cp1, 0.01))
                            {
                                indE1 = i; indE2 = i; indP1 = 2; indP2 = 2; return true;
                            }
                            if (MathVec.CompareLenSquared(cp2, 0.01))
                            {
                                indE1 = i; indE2 = i; indP1 = 3; indP2 = 3; return true;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < Edges.Count; i++)
                {
                    Vector2d b = MathVec.AbsSub(MousePos, Edges[i].Begin);
                    Vector2d e = MathVec.AbsSub(MousePos, Edges[i].End);

                    if (i == 0)
                    {
                        if (MathVec.CompareLenSquared(b, 0.01))
                        {
                            indE1 = 0; indE2 = 0; indP1 = 0; indP2 = 0; return true;
                        }
                        if (MathVec.CompareLenSquared(e, 0.01))
                        {
                            if (Edges.Count == 1)
                            {
                                indE1 = 0; indE2 = 0; indP1 = 1; indP2 = 1; return true;
                            }
                            else
                            {
                                indE1 = 0; indE2 = 1; indP1 = 1; indP2 = 0; return true;
                            }
                        }
                        if (Edges[i].IsBezie)
                        {
                            Vector2d cp1 = MathVec.AbsSub(MousePos, Edges[i][2]);
                            Vector2d cp2 = MathVec.AbsSub(MousePos, Edges[i][3]);

                            if (MathVec.CompareLenSquared(cp1, 0.01))
                            {
                                indE1 = 0; indE2 = 0; indP1 = 2; indP2 = 2; return true;
                            }
                            if (MathVec.CompareLenSquared(cp2, 0.01))
                            {
                                indE1 = 0; indE2 = 0; indP1 = 3; indP2 = 3; return true;
                            }
                        }
                    }
                    else if (i == Edges.Count - 1)
                    {
                        if (MathVec.CompareLenSquared(b, 0.01))
                        {
                            indE1 = Edges.Count - 1; indE2 = Edges.Count - 2; indP1 = 0; indP2 = 1; return true;
                        }
                        if (MathVec.CompareLenSquared(e, 0.01))
                        {
                            indE1 = Edges.Count - 1; indE2 = Edges.Count - 1; indP1 = 1; indP2 = 1; return true;
                        }
                        if (Edges[i].IsBezie)
                        {
                            Vector2d cp1 = MathVec.AbsSub(MousePos, Edges[i][2]);
                            Vector2d cp2 = MathVec.AbsSub(MousePos, Edges[i][3]);

                            if (MathVec.CompareLenSquared(cp1, 0.01))
                            {
                                indE1 = Edges.Count - 1; indE2 = Edges.Count - 1; indP1 = 2; indP2 = 2; return true;
                            }
                            if (MathVec.CompareLenSquared(cp2, 0.01))
                            {
                                indE1 = Edges.Count - 1; indE2 = Edges.Count - 1; indP1 = 3; indP2 = 3; return true;
                            }
                        }
                    }
                    else
                    {
                        if (MathVec.CompareLenSquared(b, 0.01))
                        {
                            indE1 = i; indE2 = i + 1; indP1 = 0; indP2 = 1; return true;
                        }
                        if (MathVec.CompareLenSquared(e, 0.01))
                        {
                            indE1 = i; indE2 = i + 1; indP1 = 1; indP2 = 0; return true;
                        }
                        if (Edges[i].IsBezie)
                        {
                            Vector2d cp1 = MathVec.AbsSub(MousePos, Edges[i][2]);
                            Vector2d cp2 = MathVec.AbsSub(MousePos, Edges[i][3]);

                            if (MathVec.CompareLenSquared(cp1, 0.01))
                            {
                                indE1 = i; indE2 = i; indP1 = 2; indP2 = 2; return true;
                            }
                            if (MathVec.CompareLenSquared(cp2, 0.01))
                            {
                                indE1 = i; indE2 = i; indP1 = 3; indP2 = 3; return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public bool SmoothControlPoints()
        {
            if (indE1 > -1) 
            {
                var e1 = mainFigure[indE1];
                var e2 = mainFigure[indE2];

                if (e1.IsBezie && e2.IsBezie)
                {
                    var p2 = e2.End;
                    var p1 = e1.Begin;

                    var p = e1.End;

                    var v1 = p1 - p;
                    var v2 = p2 - p;

                    Vector2d res = (v1 + v2).Normalized();
                    res = new Vector2d(res.Y, -res.X);

                    mainFigure[indE1][3] = p - res;
                    mainFigure[indE2][2] = p + res;
                    return true;
                }
                
            }
            return false;
        }

        public bool DelElement()
        {
            if (indP1 == 1 || indP1 == 0)
            {
                if (IsClosed)
                {
                    if (indE1 == 0)
                    {
                        if (indP1 == 0)
                            mainFigure[mainFigure.Count - 1].End = mainFigure[0].End;
                        if (indP2 == 1)
                            mainFigure[1].Begin = mainFigure[0].Begin;
                        mainFigure.RemoveAt(0);
                        indCurrEdge = -1;
                        return true;
                    }
                    else if (indE1 == mainFigure.Count - 1)
                    {
                        mainFigure[indE1].End = mainFigure[0].End;
                        mainFigure.RemoveAt(0);
                        indCurrEdge = -1;
                        return true;
                    }
                    else
                    {
                        mainFigure[indE1].End = mainFigure[indE1 + 1].End;
                        mainFigure.RemoveAt(indE1 + 1);
                        indCurrEdge = -1;
                        return true;
                    }
                }
                else
                {
                    ;
                }
            }
            return false;
        }

        public void ToLine()
        {
            if (indCurrEdge > -1)
            {
                mainFigure[indCurrEdge].ConvertToLine();
                ReCalc();
            }
        }
        public void ToBezie()
        {
            if (indCurrEdge > -1)
            {
                mainFigure[indCurrEdge].ConvertToBezie();
                ReCalc();
            }
        }

        public void SubDivEdge()
        {
            if (indCurrEdge > -1)
            {
                Edge e = mainFigure[indCurrEdge];
                if (e.IsBezie)
                {
                    Vector2d A = e.Begin;
                    Vector2d D = e.End;

                    Vector2d B = e.BeginControlPoint;
                    Vector2d C = e.EndControlPoint;

                    Vector2d E = (A + B) / 2;
                    Vector2d F = (B + C) / 2;
                    Vector2d G = (C + D) / 2;
                    Vector2d H = (E + F) / 2;
                    Vector2d J = (F + G) / 2;
                    Vector2d K = (H + J) / 2;

                    var e1 = new Edge(A, K, E, H);
                    var e2 = new Edge(K, D, J, G);

                    mainFigure.RemoveAt(indCurrEdge);
                    mainFigure.Insert(indCurrEdge, e1);
                    mainFigure.Insert(indCurrEdge + 1, e2);

                    ReCalc();
                }
                else
                {
                    var e1 = new Edge(e.Begin, (e.Begin + e.End) / 2.0);
                    var e2 = new Edge((e.Begin + e.End) / 2.0, e.End);

                    mainFigure.RemoveAt(indCurrEdge);
                    mainFigure.Insert(indCurrEdge, e1);
                    mainFigure.Insert(indCurrEdge + 1, e2);

                    ReCalc();
                }
                indCurrEdge = -1;
            }
        }

        public void CalcAngle(Vector2d secondMousePos)
        {
            Vector2d v = new Vector2d();
            if (Center == new Vector2d(0))
                v = secondMousePos - MoveTo;
            else
                v = secondMousePos - Center;

            v.Normalize();
            var a = manipul[indPoint].Normalized();

            //mouseRot = v;

            var sm = Vector2d.Dot(v, a);
            var angle = Math.Acos(sm);

            var psevdoScal = (a.X * v.Y) - (a.Y * v.X);

            if (psevdoScal < 0)
                angle = -angle;

            Angle = MathHelper.RadiansToDegrees(angle);
        }
        public void CalcsScale(Vector2d secondMousePos)
        {
            if (indPoint > -1)
            {
                int i = FindIndScale(indPoint);
                Vector2d m = manipul[indPoint];
                Vector2d smp = secondMousePos - MoveTo;
                if (i == 1 || i == 4)
                    ScaleTo = new Vector2d(smp.X / m.X, ScaleTo.Y);
                else if (i == 6 || i == 7)
                    ScaleTo = new Vector2d(ScaleTo.X, smp.Y / m.Y);
                else
                ScaleTo = new Vector2d(smp.X / m.X, smp.Y / m.Y);
            }
        }
        public void SetNewPoint(Vector2d MousePos)
        {
            if (indE1 > -1)
            {
                indCurrEdge = -1;
                Vector2d v = MultiplyMatrixAndVector(MousePos, TRSI.Inverted());

                mainFigure[indE1][indP1] = v;
                mainFigure[indE2][indP2] = v;

                if (indP1 == 2 || indP1 == 3)
                    mainFigure[indE1].CalcBeziePoints();

                Translate();
                ReCalc();
            }
        }
        public void SetNewEdge(Vector2d secondMousePos, Vector2d firstMousePos)
        {
            if (indCurrEdge > -1)
            {
                Vector2d s = MultiplyMatrixAndVector(secondMousePos, TRSI.Inverted());
                Vector2d f = MultiplyMatrixAndVector(firstMousePos, TRSI.Inverted());
                Vector2d moveTo = s - f;

                Edge e = mainFigure[indCurrEdge];
                if (e.IsBezie)
                {
                    e.Begin = e.Begin + moveTo;
                    e.End = e.End + moveTo;
                    e.BeginControlPoint = e.BeginControlPoint + moveTo;
                    e.EndControlPoint = e.EndControlPoint + moveTo;
                    e.CalcBeziePoints();
                }
                else
                {
                    e.Begin = e.Begin + moveTo;
                    e.End = e.End + moveTo;
                }

                if (IsClosed)
                {
                    if (indCurrEdge == 0)
                    {
                        indE1 = 1; indE2 = Edges.Count - 1; indP1 = 0; indP2 = 1;
                    }
                    else if (indCurrEdge == Edges.Count - 1)
                    {
                        indE1 = 0; indE2 = Edges.Count - 2; indP1 = 0; indP2 = 1;
                    }
                    else
                    {
                        indE1 = indCurrEdge + 1; indP1 = 0;
                        indE2 = indCurrEdge - 1; indP2 = 1;
                    }
                }
                else
                {
                    if (indCurrEdge == 0)
                    {
                        if (mainFigure.Count == 1)
                        {
                            ;// indE1 = 0; indE2 = 0; indP1 = 0; indP2 = 1;
                        }
                        else
                        {
                            indE1 = 1; indE2 = 1; indP1 = 0; indP2 = 0;
                        }
                    }
                    else if (indCurrEdge == Edges.Count - 1)
                    {
                        indE1 = Edges.Count - 2; indE2 = Edges.Count - 2; indP1 = 1; indP2 = 1;
                    }
                    else
                    {
                        indE1 = indCurrEdge + 1; indP1 = 0;
                        indE2 = indCurrEdge - 1; indP2 = 1;
                    }
                }

                if (indE1 > -1)
                {
                    if (indE1 == indE2)
                        mainFigure[indE1][indP1] += moveTo;
                    else
                    {
                        mainFigure[indE1][indP1] += moveTo;
                        mainFigure[indE2][indP2] += moveTo;
                    }
                }

                if (mainFigure.Count > 0)
                {
                    double maxx = mainFigure[0].Begin.X,
                           maxy = mainFigure[0].Begin.Y,
                           minx = mainFigure[0].Begin.X,
                           miny = mainFigure[0].Begin.Y;

                    foreach (var r in mainFigure)
                    {
                        if ((r.MaxInEdge().X >= maxx))
                            maxx = r.MaxInEdge().X;
                        if ((r.MinInEdge().X <= minx))
                            minx = r.MinInEdge().X;
                        if ((r.MaxInEdge().Y >= maxy))
                            maxy = r.MaxInEdge().Y;
                        if ((r.MinInEdge().Y <= miny))
                            miny = r.MinInEdge().Y;
                    }

                    maxPointAABB = new Vector2d(maxx, maxy);
                    minPointAABB = new Vector2d(minx, miny);

                    Manipulators[0] = maxPointAABB;
                    Manipulators[1] = new Vector2d(maxPointAABB.X, (minPointAABB.Y + maxPointAABB.Y) / 2.0);
                    Manipulators[2] = new Vector2d(maxPointAABB.X, minPointAABB.Y);

                    Manipulators[3] = minPointAABB;
                    Manipulators[4] = new Vector2d(minPointAABB.X, (minPointAABB.Y + maxPointAABB.Y) / 2.0);
                    Manipulators[5] = new Vector2d(minPointAABB.X, maxPointAABB.Y);

                    Manipulators[6] = new Vector2d((minPointAABB.X + maxPointAABB.X) / 2.0, minPointAABB.Y);
                    Manipulators[7] = new Vector2d((minPointAABB.X + maxPointAABB.X) / 2.0, maxPointAABB.Y);

                    Manipulators[8] = new Vector2d(0);
                }

                manipul = Manipulators.ToArray();
                //mainFigure = Edges.ToList();

                ReCalc();
            }
        }
        public ActionWithFigure HitOnManipulators(Vector2d mousePos)
        {
            for (int i = 0; i < Manipulators.Length - 1; i++)
            {
                // for Scale
                if ((Manipulators[i] - mousePos).LengthSquared < 0.01)
                {
                    IsDrawPoint = true;
                    indPoint = i;
                    indAroundScale = FindIndScale(indPoint);
                    return LabelOFAction = ActionWithFigure.Scale;
                }
                // for Rotate
                if ((Manipulators[i] - mousePos).LengthSquared < 0.05)
                {
                    IsDrawPoint = true;
                    indPoint = i;
                    return LabelOFAction = ActionWithFigure.Rotate;
                }
            }

            if ((Manipulators[8] - mousePos).LengthSquared < 0.01)
                return LabelOFAction = ActionWithFigure.Move;

            indPoint = -1;
            IsDrawPoint = false;
            return LabelOFAction = ActionWithFigure.None;
        }
        public bool HitOnManipulators1(Vector2d mousePos)
        {
            for (int i = 0; i < Manipulators.Length - 1; i++)
            {
                // for Scale
                if ((Manipulators[i] - mousePos).LengthSquared < 0.01)
                {
                    indPoint1 = i;
                    return true;
                }
                // for Rotate
                if ((Manipulators[i] - mousePos).LengthSquared < 0.05)
                {
                    indPoint2 = i;
                    return true;
                }
            }

            if ((Manipulators[8] - mousePos).LengthSquared < 0.01)
                return true;

            indPoint2 = indPoint1 = -1;
            return false;
        }
        public void Draw()
        {
            if (!IsRender)
                return;

            if (IsClosed)
            {
                GL.Begin(BeginMode.Triangles);
                GL.Color3(FillColor);
                foreach (var t in _triangles)
                {
                    GL.Vertex2(t.A);
                    GL.Vertex2(t.B);
                    GL.Vertex2(t.C);
                }
                GL.End();

                GL.Begin(BeginMode.LineLoop);
                GL.LineWidth(LineWidth);
                GL.Color3(BorderColor);
                for (int i = 0; i < Verteces.Count; i++)
                    GL.Vertex2(Verteces[i]);
                GL.End();
            }
            else if (Type == TypeFigures.Curve)
            {
                GL.Begin(BeginMode.LineStrip);
                GL.LineWidth(LineWidth);
                GL.Color3(BorderColor);
                for (int i = 0; i < Verteces.Count; i++)
                    GL.Vertex2(Verteces[i]);
                GL.End();
            }
            else
            {
                GL.Begin(BeginMode.LineLoop);
                GL.LineWidth(LineWidth);
                GL.Color3(BorderColor);
                for (int i = 0; i < Verteces.Count; i++)
                    GL.Vertex2(Verteces[i]);
                GL.End();
            }
            
            
        }
        public void DrawSelect()
        {
            if (IsSelect)
            {
                GL.PushMatrix();
                GL.Color3(Color.Black);
                GL.PointSize(5.0f);

                GL.Begin(BeginMode.Points);
                for (int i = 0; i < Manipulators.Length; i++)
                    GL.Vertex2(Manipulators[i]);
                GL.End();

                if (indPoint1 > -1)
                {
                    GL.Begin(BeginMode.LineStrip);
                    Vector2d c = Manipulators[indPoint1];
                    for (int i = 0; i < 360; i++)
                    {
                        double t = MathHelper.DegreesToRadians(i);
                        double x = 0.1 * Math.Cos(t);
                        double y = 0.1 * Math.Sin(t);
                        GL.Vertex2(c.X + x, c.Y + y);
                    }
                    GL.End();
                }
                else if (indPoint2 > -1)
                {
                    GL.Begin(BeginMode.LineStrip);
                    Vector2d c = Manipulators[indPoint2];
                    for (int i = 0; i < 360; i++)
                    {
                        double t = MathHelper.DegreesToRadians(i);
                        double x = 0.2 * Math.Cos(t);
                        double y = 0.2 * Math.Sin(t);
                        GL.Vertex2(c.X + x, c.Y + y);
                    }
                    GL.End();
                }
                indPoint2 = indPoint1 = -1;
                GL.PopMatrix();
            }
            if (IsEdit)
            {
                GL.PushMatrix();
                GL.Color3(Color.OrangeRed);
                GL.PointSize(5.0f);
                for (int i = 0; i < Edges.Count; i++)
                {
                    if (Edges[i].IsBezie)
                    {
                        GL.Begin(BeginMode.Points);
                        GL.Vertex2(Edges[i].BeginControlPoint);
                        GL.Vertex2(Edges[i].EndControlPoint);
                        GL.End();

                        GL.Begin(BeginMode.Lines);
                        GL.Vertex2(Edges[i].BeginControlPoint);
                        GL.Vertex2(Edges[i].Begin);
                        GL.Vertex2(Edges[i].EndControlPoint);
                        GL.Vertex2(Edges[i].End);
                        GL.End();
                    }

                    GL.Begin(BeginMode.Points);
                    GL.Vertex2(Edges[i].Begin);
                    GL.Vertex2(Edges[i].End);
                    GL.End();
                }

                GL.PopMatrix();

                if (indCurrEdge > -1)
                {
                    GL.PushMatrix();
                    GL.Color3(Color.OrangeRed);

                    if (Edges[indCurrEdge].IsBezie)
                    {
                        GL.Begin(BeginMode.LineStrip);
                        GL.Vertex2(Edges[indCurrEdge].Begin);

                        foreach (var r in Edges[indCurrEdge].BeziePoints)
                            GL.Vertex2(r);

                        GL.Vertex2(Edges[indCurrEdge].End);
                        GL.End();
                    }
                    else
                    {
                        GL.Begin(BeginMode.Lines);
                        GL.Vertex2(Edges[indCurrEdge].Begin);
                        GL.Vertex2(Edges[indCurrEdge].End);
                        GL.End();
                    }

                    

                    GL.PopMatrix();
                }
            }
        }

        public override string ToString()
        {
            string r = TRSI.ToString() + "\n";
            r += "  Verteces:\n";
            foreach (var t in Verteces)
                r += t.ToString() + "\n";
            r += "  Manipuls: \n";
            foreach (var t in Manipulators)
                r += t.X.ToString("F") + " " + t.Y.ToString("F") + "\n";
            r += "\n";
            return r;
        }       

        int FindIndScale(int index)
        {
            switch (index)
            {
                case 0: return 3;
                case 1: return 4;
                case 2: return 5;

                case 3: return 0;
                case 4: return 1;
                case 5: return 2;

                case 6: return 7;
                case 7: return 6;
            }
            return -1;
        }
        void Translate()
        {
            if (mainFigure.Count > 0)
            {
                double maxx = mainFigure[0].Begin.X,
                       maxy = mainFigure[0].Begin.Y,
                       minx = mainFigure[0].Begin.X,
                       miny = mainFigure[0].Begin.Y;

                foreach (var r in mainFigure)
                {
                    if ((r.MaxInEdge().X >= maxx))
                        maxx = r.MaxInEdge().X;
                    if ((r.MinInEdge().X <= minx))
                        minx = r.MinInEdge().X;
                    if ((r.MaxInEdge().Y >= maxy))
                        maxy = r.MaxInEdge().Y;
                    if ((r.MinInEdge().Y <= miny))
                        miny = r.MinInEdge().Y;
                }

                maxPointAABB = new Vector2d(maxx, maxy);
                minPointAABB = new Vector2d(minx, miny);

                manipul[0] = maxPointAABB;
                manipul[1] = new Vector2d(maxPointAABB.X, (minPointAABB.Y + maxPointAABB.Y) / 2.0);
                manipul[2] = new Vector2d(maxPointAABB.X, minPointAABB.Y);

                manipul[3] = minPointAABB;
                manipul[4] = new Vector2d(minPointAABB.X, (minPointAABB.Y + maxPointAABB.Y) / 2.0);
                manipul[5] = new Vector2d(minPointAABB.X, maxPointAABB.Y);

                manipul[6] = new Vector2d((minPointAABB.X + maxPointAABB.X) / 2.0, minPointAABB.Y);
                manipul[7] = new Vector2d((minPointAABB.X + maxPointAABB.X) / 2.0, maxPointAABB.Y);

                Center = ((minPointAABB + maxPointAABB) / 2.0);

                manipul[8] = new Vector2d(0);
            }

            MoveTo = Center + MoveTo;
            for (int i = 0; i < mainFigure.Count; i++)
                mainFigure[i] = mainFigure[i] - Center;

            Center = new Vector2d(0);
        }
    }

    public class Layer
    {
        public string Name { get; set; }
        public string ID { get; set; }

        List<Figure> Figures { get; set; } = new List<Figure>();

        public Layer(string name, string id)
        {
            Name = name; ID = id;
        }

        // Methods for figures
        public void AddFigure(Figure figure)
        {
            Figures.Add(figure);
        }
        public Figure FindFigure(Figure figure)
        {
            return Figures.Find(x => figure.Id == x.Id);
        }
        public Figure FindFigure(string id)
        {
            return Figures.Find(x => id == x.Id);
        }
        public Figure FindFigureName(string name)
        {
            return Figures.Find(x => name == x.Name);
        }
        public Figure FindFigure(Vector2d point)
        {
            int i = Figures.Count - 1;
            while (i > -1)
            {
                if (Figures[i].Hit(point))
                    return Figures[i];
                i--;
            }
            return null;
        }
        public int FindFigureIndex(Vector2d point)
        {
            int i = Figures.Count - 1;
            while (i > -1)
            {
                if (Figures[i].Hit(point))
                    return i;
                i--;
            }
            return -1;
        }
        public int FindFigureIndex(string text)
        {
            int i = Figures.Count - 1;
            while (i > -1)
            {
                if (Figures[i].Name == text)
                    return i;
                i--;
            }
            return -1;
        }
        public void DelFigure(Figure figure)
        {
            Figures.Remove(figure);
        }

        // For TreeVieW
        public TreeNode[] NodesForTree()
        {
            List<TreeNode> r = new List<TreeNode>();

            foreach (Figure f in Figures)
                r.Add(new TreeNode() { Name = f.Id, Text = f.Name });
            
            return r.ToArray();
        }

        // Draw all figures in the layer
        public void Draw()
        {
            foreach (Figure f in Figures)
                f.Draw();
        }

        // Debug
        public string Info()
        {
            string s = "";

            foreach (var f in Figures)
            {
                s += "Name: " + f.Name + "\n";
                s += "IsSelect: " + f.IsSelect.ToString() + "\n";
            }

            return s;
        }
    }

    public class SVG
    {
        public string Res { get; set; }
        public ulong ID { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        WorkWithMouse mouse;
        int sx, sy;

        List<Layer> _layers = new List<Layer>();
        public List<Layer> Layers
        {
            get
            {
                return _layers;
            }
            set { _layers = value; }
        }

        public SVG() {; }
        public SVG(string fileName, int controlWidth, int controlH)
        {
            if (fileName.Length == 0)
                return;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(fileName);

            // получим корневой элемент
            XmlElement xRoot = xDoc.DocumentElement;

            Width = int.Parse(xRoot.Attributes["viewBox"].Value.Split(new char[] { ' ' })[2]);
            Height = int.Parse(xRoot.Attributes["viewBox"].Value.Split(new char[] { ' ' })[3]);

            sx = (controlWidth - Width) / 2;
            sy = (controlH - Height) / 2;

            mouse = new WorkWithMouse(controlWidth, controlH, 10.0f);

            // обход всех узлов в корневом элементе
            int i = 0;
            foreach (XmlNode xnode in xRoot)
            {
                if (xnode.Name == "g")
                {
                    Layer layer = new Layer("layer_" + i.ToString(), "L" + i.ToString());
                    Res += ParseGroup(layer, xnode);
                    _layers.Add(layer);
                    i++;
                }
            }
        }

        string ParseGroup(Layer layer, XmlNode xmlNode)
        {
            string id = xmlNode.Attributes["id"].Value;
            string d = "G: " + id + "\n";
            XmlNodeList c = xmlNode.ChildNodes;

            //if (layer != null)
            //    foreach (XmlNode node in c)
            //    {
            //        switch (node.Name)
            //        {
            //            case "metadata":
            //                d += "  Meta:\n";
            //                break;

            //            case "rect":
            //                d += "  Rectangle:\n";
            //                double x = double.Parse(node.Attributes["x"].Value, System.Globalization.CultureInfo.InvariantCulture);
            //                double y = double.Parse(node.Attributes["y"].Value, System.Globalization.CultureInfo.InvariantCulture);

            //                double w = double.Parse(node.Attributes["width"].Value, System.Globalization.CultureInfo.InvariantCulture);
            //                double h = double.Parse(node.Attributes["height"].Value, System.Globalization.CultureInfo.InvariantCulture);

            //                var a = mouse.MousePosition((sx + (int)x), (sy + (int)y));
            //                var b = mouse.MousePosition((sx + (int)x + (int)w), (sy + (int)y + (int)h));

            //                Figure rect = new Figure(ID++, a, b);
            //                SetAttr(rect, node);
            //                rect.CalcAABB();
            //                rect.IsSelect = false;
            //                rect.TranslateToCenterAxis();
            //                layer.AddFigure(rect);
            //                break;

            //            case "ellipse":
            //                d += "  Ellipse:\n";
            //                x = double.Parse(node.Attributes["cx"].Value, System.Globalization.CultureInfo.InvariantCulture);
            //                y = double.Parse(node.Attributes["cy"].Value, System.Globalization.CultureInfo.InvariantCulture);

            //                double rx = double.Parse(node.Attributes["rx"].Value, System.Globalization.CultureInfo.InvariantCulture);
            //                double ry = double.Parse(node.Attributes["ry"].Value, System.Globalization.CultureInfo.InvariantCulture);

            //                a = mouse.MousePosition((sx + (int)x), (sy + (int)y));
            //                rx = Math.Abs(mouse.MousePosition(sx + (int)rx, 0).X - mouse.MousePosition(sx, 0).X);
            //                ry = Math.Abs(mouse.MousePosition(sy + (int)ry, 0).X - mouse.MousePosition(sy, 0).X);
            //                Ellipsoid circle = new Ellipsoid(ID++, new Vector2d(a.X, a.Y), rx, ry);
            //                circle.CalcPoints();
            //                SetAttr(circle, node);
            //                circle.CalcAABB();
            //                circle.IsSelect = false;
            //                circle.TranslateToCenterAxis();
            //                layer.AddFigure(circle);
            //                break;

            //            case "circle":
            //                d += "  Circle:\n";
            //                x = double.Parse(node.Attributes["cx"].Value, System.Globalization.CultureInfo.InvariantCulture);
            //                y = double.Parse(node.Attributes["cy"].Value, System.Globalization.CultureInfo.InvariantCulture);

            //                double r = double.Parse(node.Attributes["r"].Value, System.Globalization.CultureInfo.InvariantCulture);

            //                a = mouse.MousePosition((sx + (int)x), (sy + (int)y));
            //                r = Math.Abs(mouse.MousePosition(sx + (int)r, 0).X - mouse.MousePosition(sx, 0).X);

            //                Ellipsoid cc = new Ellipsoid(ID++, new Vector2d(a.X, a.Y), r, r);
            //                cc.CalcPoints();
            //                SetAttr(cc, node);
            //                cc.CalcAABB();
            //                cc.IsSelect = false;
            //                cc.TranslateToCenterAxis();
            //                layer.AddFigure(cc);
            //                break;

            //            case "polygon":
            //                d += "  Polygon:\n";
            //                string s = GetAttr("points", node);
            //                string[] points = s.Split(new char[] { ' ' });
            //                List<Vector2d> l = new List<Vector2d>();
            //                for (int k = 0; k < points.Length - 1; k++)
            //                {
            //                    string[] s1 = points[k].Split(new char[] { ',' });
            //                    double xx = double.Parse(s1[0], System.Globalization.CultureInfo.InvariantCulture);
            //                    double yy = double.Parse(s1[1], System.Globalization.CultureInfo.InvariantCulture);
            //                    l.Add(mouse.MousePosition((sx + xx), (sy + yy)));
            //                }
            //                Polygon poli = new Polygon(ID++, l);
            //                poli.CalcAABB();
            //                SetAttr(poli, node);
            //                poli.CalcAABB();
            //                poli.IsSelect = false;
            //                poli.TranslateToCenterAxis();
            //                layer.AddFigure(poli);
            //                break;

            //            case "path":
            //                d += "  Path:\n";

            //                break;

            //            case "line":
            //                d += "  Line:\n";
            //                double x1 = double.Parse(node.Attributes["x1"].Value, System.Globalization.CultureInfo.InvariantCulture);
            //                double y1 = double.Parse(node.Attributes["y1"].Value, System.Globalization.CultureInfo.InvariantCulture);

            //                double x2 = double.Parse(node.Attributes["x2"].Value, System.Globalization.CultureInfo.InvariantCulture);
            //                double y2 = double.Parse(node.Attributes["y2"].Value, System.Globalization.CultureInfo.InvariantCulture);

            //                a = mouse.MousePosition((sx + (int)x1), (sy + (int)y1));
            //                b = mouse.MousePosition((sx + (int)x2), (sy + (int)y2));

            //                Line line = new Line(
            //                    ID++,
            //                    a,
            //                    b);
            //                SetAttr(line, node);
            //                line.CalcAABB();
            //                line.IsSelect = false;
            //                line.TranslateToCenterAxis();
            //                layer.AddFigure(line);
            //                break;

            //            case "g":
            //                d += ParseGroup(layer, node);
            //                break;

            //            default:
            //                break;
            //        }
            //        //return d;
            //    }

            return d;
        }
        string GetAttr(string attr, XmlNode xmlNode)
        {
            if (xmlNode.Attributes[attr] != null)
                return xmlNode.Attributes[attr].Value;
            else
                return null;
        }
        void SetAttr(Figure figure, XmlNode xmlNode)
        {
            string s = null;
            switch (figure.Type)
            {
                case TypeFigures.Line:
                    s = GetAttr("stroke", xmlNode);
                    if (s != null)
                        figure.FillColor = ColorTranslator.FromHtml(s);

                    s = GetAttr("stroke-width", xmlNode);
                    if (s != null)
                        figure.LineWidth = float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

                    if ((s = GetAttr("id", xmlNode)) != null)
                        figure.Name = s;
                    break;

                case TypeFigures.Circle:
                    s = GetAttr("stroke", xmlNode);
                    if (s != null)
                        figure.BorderColor = ColorTranslator.FromHtml(s);

                    s = GetAttr("stroke-width", xmlNode);
                    if (s != null)
                        figure.LineWidth = float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

                    if ((s = GetAttr("fill", xmlNode)) != null)
                        figure.FillColor = ColorTranslator.FromHtml(s);

                    if ((s = GetAttr("id", xmlNode)) != null)
                        figure.Name = s;
                    break;

                case TypeFigures.Rect:
                    s = GetAttr("stroke", xmlNode);
                    if (s != null)
                        figure.BorderColor = ColorTranslator.FromHtml(s);

                    if ((s = GetAttr("fill", xmlNode)) != null)
                        figure.FillColor = ColorTranslator.FromHtml(s);

                    s = GetAttr("stroke-width", xmlNode);
                    if (s != null)
                        figure.LineWidth = float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

                    if ((s = GetAttr("id", xmlNode)) != null)
                        figure.Name = s;
                    break;

                case TypeFigures.Polygon:
                    s = GetAttr("stroke", xmlNode);
                    if (s != null)
                        figure.BorderColor = ColorTranslator.FromHtml(s);

                    s = GetAttr("stroke-width", xmlNode);
                    if (s != null)
                        figure.LineWidth = float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

                    if ((s = GetAttr("fill", xmlNode)) != null)
                        figure.FillColor = ColorTranslator.FromHtml(s);

                    if ((s = GetAttr("id", xmlNode)) != null)
                        figure.Name = s;
                    break;

                default:
                    break;
            }
        }

        public void Draw()
        {
            foreach (var l in _layers)
            {
                l.Draw();
            }
        }
    }

    public class ExportInGEO
    {
        public string PathToExport { get; set; } = string.Empty;
        public double Weight { get; set; } = 0.1;

        public ExportInGEO() {; }
        public ExportInGEO(string pathToExport)
        {
            PathToExport = pathToExport;
        }

        public void WriteInfile(List<Figure> figures)
        {
            if (figures.Count > 0 && PathToExport.Length > 0)
            {
                // сменить локаль для корректного вывода дробных чисел
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(PathToExport))
                {
                    foreach (var f in figures)
                    {
                        StringBuilder sb = new StringBuilder("///// Create in\n");

                        // Points(i) = {x, y, 0, Weight};
                        for (int i = 0, j = 1; i < f.Verteces.Count; i++, j++)
                            sb.Append("Point(" + j.ToString() + ") = { " +
                                f.Verteces[i].X.ToString("F") +
                                ", " +
                                f.Verteces[i].Y.ToString("F") +
                                ", 0, " +
                                Weight.ToString("F") +
                                " };\n");

                        sb.Append("\n");

                        int k = 1, p1 = 1, p2 = 2;
                        for (int i = 0; i < f.Verteces.Count; i++)
                        {
                            if (p1 == f.Verteces.Count)
                                p2 = 1;
                            sb.Append(
                                "Line(" +
                                k.ToString() +
                                ") = { " +
                                p1.ToString() +
                                ", " +
                                p2.ToString() +
                                " };\n"
                                    );
                            k++; p1++; p2++;
                        }
                        //foreach (var e in f.Edges)
                        //{
                        //    if (p1 == f.Edges.Count)
                        //        p2 = 1;
                        //    if (e.IsBezie)
                        //        for (int m = 0; m < e.BezieSegments; m++)
                        //        {
                        //            if (p1 == f.Verteces.Count)
                        //                p2 = 1;
                        //            sb.Append("Line(" + k.ToString() + ") = { " +
                        //                p1.ToString() + ", " + p2.ToString() +
                        //                " };\n"
                        //                );
                        //            k++;p1++;p2++;
                        //        }
                        //    else
                        //    {
                        //        if (p1 == f.Verteces.Count)
                        //            p2 = 1;
                        //        sb.Append("Line(" + k.ToString() + ") = { " +
                        //            p1.ToString() + ", " + p2.ToString() +
                        //            " };\n"
                        //            );
                        //        k++;
                        //        p1++; p2++;
                        //    }
                        //}
                        sb.Append("\n");

                        // формирование петли, состоящей из прямых описанных выше
                        sb.Append("Curve Loop(1) = { ");
                        for (int i = 1; i <= k - 2; i++)
                            sb.Append(i.ToString() + ", ");
                        sb.Append((k - 1).ToString() + " };\n");

                        sb.Append("\n");

                        sb.Append("Plane Surface(1) = { 1 };");

                        sw.WriteLine(sb.ToString());
                    }
                }
            }
            else
                throw new Exception("Not figures or incorrect path to file!");
        }
    }

    public class ImportFromGEO
    {
        public string Path { get; set; } = "";
        public double W { get; set; } = -1;
        public Vector2d Center { get; set; }

        public ImportFromGEO() {; }
        public ImportFromGEO(string pathToFile)
        {
            Path = pathToFile;
        }

        public List<Edge> Result()
        {
            List<Edge> r = new List<Edge>();
            Dictionary<int, Vector2d> points = new Dictionary<int, Vector2d>();
            string[] lines = null;
            string l;
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            using ( System.IO.StreamReader sr = new System.IO.StreamReader(Path))
            {
                l = sr.ReadToEnd();
            }

            lines = l.Split(new char[] { '\n' });
            foreach (string s in lines)
            {
                if (s.Length > 0 && s[0] == '/' && s[1] == '/') ;
                else if (s.Contains("Point"))
                {
                    string[] ll = s.Split(new char[] { '(', ')', '{', '}', ',' });

                    points.Add(
                        int.Parse(ll[1]),
                        new Vector2d(
                            double.Parse(ll[3]),
                            double.Parse(ll[4])
                            )
                        );
                }
                else if (s.Contains("Line"))
                {
                    string[] g = s.Split('{', '}', ',');

                    int b = int.Parse(g[1]), e = int.Parse(g[2]);

                    r.Add(
                        new Edge(points[b], points[e])
                        );
                }
                else if (s.Contains("Plane")) ;
                else if (s.Contains("Loop")) ;
                else if (s.Length > 0 && s[0] != '/')
                {
                    string[] ll = s.Split(' ', ';');
                    W = double.Parse(ll[2]);
                }
            }

            foreach (var u in points.Values)
                Center += u;

            Center = Center / points.Count;

            return r;
        }
    }

    public class Vertex
    {
        public Vector2d V { get; set; } = new Vector2d(double.NaN);
        public int IndexIn1 { get; set; } = -1;
        public int IndexIn2 { get; set; } = -1;
        public bool IsInOtherFigure { get; set; } = false;
        public bool IsPointIntersection { get; set; } = false;

        public Vertex() {; }
        public Vertex(Vector2d v, int i1, int i2, bool pointInterset)
        {
            V = v; IndexIn1 = i1; IndexIn2 = i2; IsPointIntersection = pointInterset;
        }

        public override string ToString()
        {
            return "Vert: " + V.ToString() + " I1: " + IndexIn1.ToString() + " I2: " + IndexIn2.ToString() + " Other: " + IsInOtherFigure.ToString()
                + " Interset: " + IsPointIntersection.ToString() + "\n";
        }
    }

    public class Group
    {
        public List<Figure> Figures { get; set; } = new List<Figure>();
        public string ID { get; set; }
        public AABB AABB { get; set; } = new AABB();
        public bool IsEmpty { get { if (Figures.Count > 0) return false; else return true; } private set {; } }
        public Figure SelectingFigure { get; set; } = null;
        public int CountFigures { get { return Figures.Count; } private set {; } }

        public Group() { ; }

        public void Add(Figure f)
        {
            if (f != null && !Figures.Contains(f))
            {
                f.IsSelect = true;
                Figures.Add(f);
                SelectingFigure = Figures[Figures.Count - 1];

                double mx = Figures[0].AABB.Max.X;
                double my = Figures[0].AABB.Max.Y;

                double mnx = Figures[0].AABB.Min.X;
                double mny = Figures[0].AABB.Min.Y;

                foreach (Figure ff in Figures)
                {
                    if (ff.AABB.Max.X >= mx)
                        mx = ff.AABB.Max.X;
                    if (ff.AABB.Max.Y >= my)
                        my = ff.AABB.Max.Y;

                    if (ff.AABB.Min.X <= mnx)
                        mnx = ff.AABB.Min.X;
                    if (ff.AABB.Min.Y <= mny)
                        mny = ff.AABB.Min.Y;
                }

                AABB.Min = new Vector2d(mnx, mny); AABB.Max = new Vector2d(mx, my);
                AABB.Center = (AABB.Max + AABB.Min) / 2.0;
            }
        }
        public void Del(Figure figure)
        {
            Figures.Remove(figure);
        }
        public void Clear()
        {
            foreach (Figure f in Figures)
            {
                f.IsSelect = false;
                
            }
            SelectingFigure = null;
            Figures.Clear();
            AABB = new AABB();
        }
        public bool HitInCenter(Vector2d v)
        {
            if (MathVec.VectrCompare(AABB.Center, v, 0.1))
                return true;
            return false;
        }
        public void MoveTo(Vector2d v)
        {
            Vector2d oldMax = AABB.Max - AABB.Center;
            Vector2d oldMin = AABB.Min - AABB.Center;

            foreach (Figure f in Figures)
            {
                f.MoveTo = f.MoveTo - AABB.Center + v;
                f.ReCalc();               
            }
            AABB.Center = v;
            AABB.Max = v + oldMax;
            AABB.Min = v + oldMin;
        }
        public void DrawAABB()
        {
            GL.PushMatrix();
            GL.Begin(BeginMode.Points);
            GL.PointSize(5f);
            GL.Vertex2(AABB.Max);
            GL.Vertex2(AABB.Min);
            GL.Vertex2(AABB.Center);
            GL.Vertex2(AABB.Max.X, AABB.Min.Y);
            GL.Vertex2(AABB.Min.X, AABB.Max.Y);
            GL.End();

            GL.Begin(BeginMode.LineLoop);
            GL.Vertex2(AABB.Max);
            GL.Vertex2(AABB.Max.X, AABB.Min.Y);
            GL.Vertex2(AABB.Min);          
            GL.Vertex2(AABB.Min.X, AABB.Max.Y);
            GL.End();
            GL.PopMatrix();
        }
        public bool IsHitInGroup(Vector2d v)
        {
            if (AABB.HitInAABB(v))
            {
                return true;
            }
            return false;
        }
        public void ReCalcAABB()
        {
            if (Figures.Count > 0)
            {
                double mx = Figures[0].AABB.Max.X;
                double my = Figures[0].AABB.Max.Y;

                double mnx = Figures[0].AABB.Min.X;
                double mny = Figures[0].AABB.Min.Y;

                foreach (Figure ff in Figures)
                {
                    if (ff.AABB.Max.X >= mx)
                        mx = ff.AABB.Max.X;
                    if (ff.AABB.Max.Y >= my)
                        my = ff.AABB.Max.Y;

                    if (ff.AABB.Min.X <= mnx)
                        mnx = ff.AABB.Min.X;
                    if (ff.AABB.Min.Y <= mny)
                        mny = ff.AABB.Min.Y;
                }

                AABB.Min = new Vector2d(mnx, mny); AABB.Max = new Vector2d(mx, my);
                AABB.Center = (AABB.Max + AABB.Min) / 2.0;
            }
        }
    }

    public class Modificators
    {
        List<Figure> figures = new List<Figure>();
        Figure f1, f2;
        Operations operation = Operations.None;
        List<Vector2d> figure1 = new List<Vector2d>();
        List<Vector2d> figure2 = new List<Vector2d>();

        public Operations Operation { get { return operation; } set { operation = value; } }
        public string ResTime { get; private set; }

        public Modificators() {; }
        public Modificators(List<Vector2d> InputFigure1, List<Vector2d> Figure2)
        {
            figure1 = InputFigure1; figure2 = Figure2;
        }

        public List<Vertex> Calculate()
        {
            int count1 = figure1.Count, count2 = figure2.Count;
            List<Vertex> res = new List<Vertex>();

            Vector2d v = new Vector2d(double.NaN);

            for (int i = 0, k = 0; i < count1; i++)
            {
                for (int j = 0, m = 0; j < count2; j++)
                {
                    k = i; m = j;
                    try
                    {
                        if ((j == count2 - 1) && (i == count1 - 1))
                            v = LinesIntersection(figure1[i], figure1[0], figure2[j], figure2[0]);
                        else if ((i == count1 - 1) && (j != count2 - 1))
                        {
                            m++;
                            v = LinesIntersection(figure1[i], figure1[0], figure2[j], figure2[m]);
                        }
                        else if ((i != count1 - 1) && (j == count2 - 1))
                        {
                            k++;
                            v = LinesIntersection(figure1[i], figure1[k], figure2[j], figure2[0]);
                        }
                        else
                        {
                            m++; k++;
                            v = LinesIntersection(figure1[i], figure1[k], figure2[j], figure2[m]);
                        }

                        if (!double.IsNaN(v.X))
                        {
                            res.Add(new Vertex(v, i, j, true));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message + "\n");
                    }
                }
            }

            return res;
        }
        public List<Vertex> Hits()
        {
            List<Vertex> res = new List<Vertex>();

            foreach (var v in figure1)
                if (Hit(v, figure2))
                    res.Add(new Vertex() { V = v, IsInOtherFigure = true });

            foreach (var v in figure2)
                if (Hit(v, figure1))
                    res.Add(new Vertex() { V = v, IsInOtherFigure = true });

            return res;
        }
        public List<Vector2d> Result()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            List<Vector2d> res = new List<Vector2d>();
            List<Vertex> F1 = new List<Vertex>();
            List<Vertex> F2 = new List<Vertex>();

            // помечаем вершины которые попали в другую фигуру 
            foreach (var v in figure1)
                if (Hit(v, figure2))
                    F1.Add(new Vertex() { V = v, IsInOtherFigure = true });
                else
                    F1.Add(new Vertex() { V = v, IsInOtherFigure = false });

            foreach (var v in figure2)
                if (Hit(v, figure1))
                    F2.Add(new Vertex() { V = v, IsInOtherFigure = true });
                else
                    F2.Add(new Vertex() { V = v, IsInOtherFigure = false });
            //-------------------------------------------------

            // составляем список из точек пересечений
            List<Vertex> pointsIntersection = Calculate();
            //-------------------------------------------------

            ChangeList(F1, pointsIntersection);
            ChangeList(F2, pointsIntersection);

            switch (operation)
            {
                case Operations.Interset:
                    if (F1[0].IsInOtherFigure)
                        res = intersect(F1, F2, pointsIntersection);
                    else if (F2[0].IsInOtherFigure)
                        res = intersect(F2, F1, pointsIntersection);
                    else
                        res = intersect1(F1, F2, pointsIntersection);

                    stopWatch.Stop();
                    // Get the elapsed time as a TimeSpan value.
                    TimeSpan ts = stopWatch.Elapsed;

                    // Format and display the TimeSpan value.
                    ResTime = ts.Milliseconds.ToString();

                    if (res.Count > 2)
                        return res;
                    else
                        return new List<Vector2d>();
                    break;

                case Operations.Union:
                    if (F1[0].IsInOtherFigure && F2[0].IsInOtherFigure)
                        res = union1(F1, F2, pointsIntersection);
                    else if (F1[0].IsInOtherFigure)
                        res = union(F2, F1, pointsIntersection);
                    else
                        res = union(F1, F2, pointsIntersection);

                    stopWatch.Stop();
                    // Get the elapsed time as a TimeSpan value.
                    ts = stopWatch.Elapsed;

                    // Format and display the TimeSpan value.
                    ResTime = ts.Milliseconds.ToString();

                    return res;

                    break;

                case Operations.Sub:
                    if (F1[0].IsInOtherFigure && F2[0].IsInOtherFigure)
                        res = sub1(F1, F2, pointsIntersection);
                    else if (F1[0].IsInOtherFigure && (!F2[0].IsInOtherFigure))
                        res = sub2(F1, F2, pointsIntersection);
                    else
                        res = sub(F1, F2, pointsIntersection);

                    stopWatch.Stop();
                    // Get the elapsed time as a TimeSpan value.
                    ts = stopWatch.Elapsed;

                    // Format and display the TimeSpan value.
                    ResTime = ts.Milliseconds.ToString();

                    return res;

                    break;

                default:
                    break;
            }

            return res;
        }

        public List<Vector2d> intersect(List<Vertex> F1, List<Vertex> F2, List<Vertex> pointsIntersection)
        {
            List<Vector2d> res = new List<Vector2d>();
            bool exit = false, change = false;
            Vertex v = new Vertex();
            int i = 0;
            int begin = 0;
            //res.Add(v.V);
            while (!exit)
            {
                v = F1[i];                

                if (v.IsInOtherFigure)
                    res.Add(v.V); 

                if (v.IsPointIntersection && (exit == false))
                {
                    res.Add(v.V);
                    //int ind = F2.FindIndex(x => x.V == v.V);
                    int ind = F2.FindIndex(
                        x => {
                            return MathVec.DCompare(x.V.X, v.V.X) && MathVec.DCompare(x.V.Y, v.V.Y);
                        }
                    );
                    if (ind == F2.Count - 1)
                    {
                        ind = 0;
                        v = F2[ind];
                        while (!change)
                        {
                            if (v.IsInOtherFigure)
                                res.Add(v.V);

                            if (v.IsPointIntersection)
                            {
                                //res.Add(v.V);
                                change = true;
                            }

                            ind++;
                            v = F2[ind];
                        }
                    }
                    else
                    {
                        ind++;
                        v = F2[ind];
                        while (!change)
                        {
                            if (v.IsInOtherFigure)
                                res.Add(v.V);

                            if (v.IsPointIntersection)
                            {
                                res.Add(v.V);
                                change = true;
                            }

                            if (change == false)
                            {
                                if (ind != F2.Count - 1)
                                    ind++;
                                else
                                    ind = 0;
                                v = F2[ind];
                            }
                        }
                    }

                    i = F1.FindIndex(
                        x =>
                        {
                            return MathVec.DCompare(x.V.X, v.V.X) && MathVec.DCompare(x.V.Y, v.V.Y);
                        });
                    //exit = true;
                }

                if (i == F1.Count - 1)
                    exit = true;

                i++;
            }

            return res;
        }
        public List<Vector2d> intersect1(List<Vertex> F1, List<Vertex> F2, List<Vertex> pointsIntersection)
        {
            List<Vector2d> res = new List<Vector2d>();
            bool exit = false, change = false;
            
            int i = F1.FindIndex(x => x.IsPointIntersection == true);
            Vertex v = F1[i];

            res.Add(v.V);
            while (!exit)
            {
                i++;
                v = F1[i];

                if (v.IsInOtherFigure)
                    res.Add(v.V);

                if (v.IsPointIntersection && (exit == false))
                {
                    res.Add(v.V);

                    int ind = F2.FindIndex(x => x.IsPointIntersection == true);
                    if (ind == F2.Count - 1)
                    {
                        ind = 0;
                        v = F2[ind];
                        while (!change)
                        {
                            if (v.IsInOtherFigure)
                                res.Add(v.V);

                            if (v.IsPointIntersection)
                            {
                                //res.Add(v.V);
                                change = true;
                            }

                            ind++;
                            v = F2[ind];
                        }
                    }
                    else
                    {
                        ind++;
                        v = F2[ind];
                        while (!change)
                        {
                            if (v.IsInOtherFigure)
                                res.Add(v.V);

                            if (v.IsPointIntersection)
                            {
                                //res.Add(v.V);
                                change = true;
                            }

                            if (change == false)
                            {
                                if (ind != F2.Count - 1)
                                    ind++;
                                else
                                    ind = 0;
                                v = F2[ind];
                            }
                        }
                    }
                    exit = true;
                }
            }

            return res;
        }
        public List<Vector2d> union(List<Vertex> F1, List<Vertex> F2, List<Vertex> pointsIntersection)
        {
            List<Vector2d> res = new List<Vector2d>();
            bool exit = false, change = false;
            Vertex v = new Vertex();
            int i = 0;
            int begin = 0;
            //res.Add(v.V);
            while (!exit)
            {
                v = F1[i];

                if (!v.IsInOtherFigure)
                    res.Add(v.V);

                if (v.IsPointIntersection && (exit == false))
                {
                    res.Add(v.V);
                    //int ind = F2.FindIndex(x => x.V == v.V);
                    int ind = F2.FindIndex(
                        x => {
                            return MathVec.DCompare(x.V.X, v.V.X) && MathVec.DCompare(x.V.Y, v.V.Y);
                        }
                    );
                    if (ind == F2.Count - 1)
                    {
                        ind = 0;
                        v = F2[ind];
                        while (!change)
                        {
                            if (!v.IsInOtherFigure)
                                res.Add(v.V);

                            if (v.IsPointIntersection)
                            {
                                //res.Add(v.V);
                                change = true;
                            }

                            ind++;
                            v = F2[ind];
                        }
                    }
                    else
                    {
                        ind++;
                        v = F2[ind];
                        while (!change)
                        {
                            if (!v.IsInOtherFigure)
                                res.Add(v.V);

                            if (v.IsPointIntersection)
                            {
                                res.Add(v.V);
                                change = true;
                            }

                            if (change == false)
                            {
                                if (ind != F2.Count - 1)
                                    ind++;
                                else
                                    ind = 0;
                                v = F2[ind];
                            }
                        }
                    }

                    i = F1.FindIndex(
                        x =>
                        {
                            return MathVec.DCompare(x.V.X, v.V.X) && MathVec.DCompare(x.V.Y, v.V.Y);
                        });
                    //exit = true;
                }

                if (i == F1.Count - 1)
                    exit = true;

                i++;
            }

            return res;
        }
        public List<Vector2d> union1(List<Vertex> F1, List<Vertex> F2, List<Vertex> pointsIntersection)
        {
            List<Vector2d> res = new List<Vector2d>();
            bool exit = false, change = false;

            int i = F1.FindIndex(x => x.IsPointIntersection == true);
            Vertex v = F1[i];

            res.Add(v.V);
            while (!exit)
            {
                i++;
                v = F1[i];

                if (!v.IsInOtherFigure)
                    res.Add(v.V);

                if (v.IsPointIntersection && (exit == false))
                {
                    res.Add(v.V);

                    int ind = F2.FindIndex(x => x.IsPointIntersection == true);
                    if (ind == F2.Count - 1)
                    {
                        ind = 0;
                        v = F2[ind];
                        while (!change)
                        {
                            if (!v.IsInOtherFigure)
                                res.Add(v.V);

                            if (v.IsPointIntersection)
                            {
                                //res.Add(v.V);
                                change = true;
                            }

                            ind++;
                            v = F2[ind];
                        }
                    }
                    else
                    {
                        ind++;
                        v = F2[ind];
                        while (!change)
                        {
                            if (!v.IsInOtherFigure)
                                res.Add(v.V);

                            if (v.IsPointIntersection)
                            {
                                //res.Add(v.V);
                                change = true;
                            }

                            if (change == false)
                            {
                                if (ind != F2.Count - 1)
                                    ind++;
                                else
                                    ind = 0;
                                v = F2[ind];
                            }
                        }
                    }
                    exit = true;
                }
            }

            return res;
        }
        public List<Vector2d> sub(List<Vertex> F1, List<Vertex> F2, List<Vertex> pointsIntersection)
        {
            List<Vector2d> res = new List<Vector2d>();
            bool exit = false, change = false;

            int i = 0;// F1.FindIndex(x => x.IsPointIntersection == true);
            Vertex v = F1[i];

            res.Add(v.V);
            while (!exit)
            {
                i++;
                v = F1[i];

                if (!v.IsInOtherFigure)
                    res.Add(v.V);

                if (v.IsPointIntersection && (exit == false))
                {
                    res.Add(v.V);

                    int ind = F2.FindIndex(x => { return MathVec.DCompare(x.V.X, v.V.X) && MathVec.DCompare(x.V.Y, v.V.Y); });
                    if (ind == F2.Count - 1)
                    {
                        //ind = 0;
                        v = F2[ind];
                        while (!change)
                        {
                            if (v.IsInOtherFigure)
                                res.Add(v.V);

                            if (v.IsPointIntersection)
                            {
                                //res.Add(v.V);
                                change = true;
                            }

                            ind--;
                            v = F2[ind];
                        }
                    }
                    else
                    {
                        ind--;
                        v = F2[ind];
                        while (!change)
                        {
                            if (v.IsInOtherFigure)
                                res.Add(v.V);

                            if (v.IsPointIntersection)
                            {
                                //res.Add(v.V);
                                change = true;
                            }

                            if (change == false)
                            {
                                if (ind != 0)
                                    ind--;
                                else
                                    ind = F2.Count - 1;
                                v = F2[ind];
                            }
                        }
                    }

                    res.Add(v.V);
                    i = F1.FindIndex(x => { return MathVec.DCompare(x.V.X, v.V.X) && MathVec.DCompare(x.V.Y, v.V.Y); });

                    if (i == F1.Count - 1)
                    {
                        exit = true;
                    }
                    else
                    {        
                        while (i != F1.Count - 1)
                        {
                            i++;
                            v = F1[i];

                            if (!v.IsInOtherFigure)
                                res.Add(v.V);
                        }
                    }

                    exit = true;
                }
            }

            return res;
        }
        public List<Vector2d> sub1(List<Vertex> F1, List<Vertex> F2, List<Vertex> pointsIntersection)
        {
            List<Vector2d> res = new List<Vector2d>();
            bool exit = false, change = false;

            int i = F1.FindIndex(x => x.IsPointIntersection == true);
            Vertex v = F1[i];

            res.Add(v.V);
            while (!exit)
            {
                i++;
                v = F1[i];

                if (!v.IsInOtherFigure)
                    res.Add(v.V);

                if (v.IsPointIntersection && (exit == false))
                {
                    res.Add(v.V);

                    int ind = F2.FindIndex(x => x.IsPointIntersection == true);
                    if (ind == F2.Count - 1)
                    {
                        //ind = 0;
                        v = F2[ind];
                        while (!change)
                        {
                            if (v.IsInOtherFigure)
                                res.Add(v.V);

                            if (v.IsPointIntersection)
                            {
                                //res.Add(v.V);
                                change = true;
                            }

                            ind--;
                            v = F2[ind];
                        }
                    }
                    else
                    {
                        ind--;
                        v = F2[ind];
                        while (!change)
                        {
                            if (v.IsInOtherFigure)
                                res.Add(v.V);

                            if (v.IsPointIntersection)
                            {
                                //res.Add(v.V);
                                change = true;
                            }

                            if (change == false)
                            {
                                if (ind != 0)
                                    ind--;
                                else
                                    ind = F2.Count - 1;
                                v = F2[ind];
                            }
                        }
                    }
                    exit = true;
                }
            }

            return res;
        }
        public List<Vector2d> sub2(List<Vertex> F1, List<Vertex> F2, List<Vertex> pointsIntersection)
        {
            List<Vector2d> res = new List<Vector2d>();
            bool exit = false, change = false;

            int i = F1.FindIndex(x => x.IsPointIntersection == true);
            Vertex v = F1[i];

            res.Add(v.V);
            while (!exit)
            {
                i++;
                v = F1[i];

                if (!v.IsInOtherFigure)
                    res.Add(v.V);

                if (v.IsPointIntersection && (exit == false))
                {
                    res.Add(v.V);

                    int ind = F2.FindIndex(x => { return MathVec.DCompare(x.V.X, v.V.X) && MathVec.DCompare(x.V.Y, v.V.Y); });
                    if (ind == F2.Count - 1)
                    {
                        //ind = 0;
                        v = F2[ind];
                        while (!change)
                        {
                            if (v.IsInOtherFigure)
                                res.Add(v.V);

                            if (v.IsPointIntersection)
                            {
                                //res.Add(v.V);
                                change = true;
                            }

                            ind--;
                            v = F2[ind];
                        }
                    }
                    else
                    {
                        ind--;
                        v = F2[ind];
                        while (!change)
                        {
                            if (v.IsInOtherFigure)
                                res.Add(v.V);

                            if (v.IsPointIntersection)
                            {
                                //res.Add(v.V);
                                change = true;
                            }

                            if (change == false)
                            {
                                if (ind != 0)
                                    ind--;
                                else
                                    ind = F2.Count - 1;
                                v = F2[ind];
                            }
                        }
                    }
                    exit = true;
                }
            }

            return res;
        }

        public void ChangeList(List<Vertex> inputList, List<Vertex> pointsInter)
        {
            bool e = true;
            int j = 1, k = 0, i = 0;
            Vertex v = pointsInter[k];
            while (e)
            {
                if (j == inputList.Count)
                {
                    if (MathVec.PointOnEdge(inputList[i].V, inputList[0].V, v.V))
                    {
                        inputList.Add(v);
                        i = -1;
                        j = 0;
                        k++;
                        if (k == pointsInter.Count)
                            return;
                        v = pointsInter[k];
                    }
                }
                else if (MathVec.PointOnEdge(inputList[i].V, inputList[j].V, v.V))
                {
                    inputList.Insert(j, v);
                    i = -1;
                    j = 0;
                    k++;
                    if (k == pointsInter.Count)
                        return;
                    v = pointsInter[k];
                }
                i++;
                j++;
            }

        }
        Vector2d LinesIntersection(Vector2d x1, Vector2d y1, Vector2d x2, Vector2d y2)
        {
            Vector2d r = new Vector2d(double.NaN);

            Vector2d
            a = x1,
            b = y1,
            c = x2,
            d = y2;

            double D = (a.X - b.X) * (d.Y - c.Y) - (d.X - c.X) * (a.Y - b.Y);
            if (D == 0) // отрезки парралельны
                //throw new Exception("Линии паралельны");
                return r;

            double u = ((d.X - b.X) * (d.Y - c.Y) - (d.X - c.X) * (d.Y - b.Y)) / D;

            double v = ((a.X - b.X) * (d.Y - b.Y) - (d.X - b.X) * (a.Y - b.Y)) / D;

            //if (u <= double.MinValue && v <= double.MinValue)
            //throw new Exception("Линии совпадают"); 

            if ((0 <= u) && (u <= 1) && (0 <= v) && (v <= 1))
            {
                double x = u * a.X + (1 - u) * b.X;
                double y = u * a.Y + (1 - u) * b.Y;
                r = new Vector2d(x, y);
            }

            //Console.WriteLine(D + "\n" + u + "\n" + v);

            return r;
        }
        public bool Hit(Vector2d v, List<Vector2d> Verteces)
        {
            Vector2d Point = v;
            bool result = false;
            int j = Verteces.Count - 1;


            for (int i = 0; i < Verteces.Count; i++)
            {
                if (
                    (Verteces[i].Y < Point.Y && Verteces[j].Y >= Point.Y
                    ||
                    Verteces[j].Y < Point.Y && Verteces[i].Y >= Point.Y)
                    &&
                     (Verteces[i].X + (Point.Y - Verteces[i].Y) / (Verteces[j].Y - Verteces[i].Y) * (Verteces[j].X - Verteces[i].X) < Point.X))
                    result = !result;
                j = i;
            }

            return result;
        }
    }

    public class TrianglesBool
    {
        public List<Triangle> Result1 { get; set; } = new List<Triangle>();
        public List<Triangle> Result2 { get; set; } = new List<Triangle>();
        public List<Triangle> Intersect { get; set; } = new List<Triangle>();
        public List<Triangle> Union { get; set; } = new List<Triangle>();
        public List<Triangle> Sub { get; set; } = new List<Triangle>();
        public string ResTime { get; private set; }

        public TrianglesBool() {; }
        public TrianglesBool(List<Triangle> Tr1, List<Triangle> Tr2, List<Vector2d> F1, List<Vector2d> F2, Operations o)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Triangulating(Tr1, Tr2, F1, F2);
            CalcCenters(F1, F2, o);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            ResTime = ts.Milliseconds.ToString();
        }
        public TrianglesBool(Figure f1, Figure f2, Operations operations)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Triangulating(f1.Triangles, f2.Triangles, f1.Verteces, f2.Verteces);
            CalcCenters(f1.Verteces, f2.Verteces, operations);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            ResTime = ts.Milliseconds.ToString();
        }


        void Subdiv(List<Triangle> triangles, Vector2d v1, Vector2d v2)
        {
            List<Triangle> res = new List<Triangle>();
            List<Triangle> input = triangles.ToList();
            triangles.Clear();

            foreach (var t in input)
            {
                Triangle triangle = t;

                Vector2d End = v1;
                Vector2d Begin = v2;

                Vector2d p1 = (MathVec.LinesIntersection(triangle.A, triangle.B, End, Begin));
                Vector2d p2 = (MathVec.LinesIntersection(triangle.B, triangle.C, End, Begin));
                Vector2d p3 = (MathVec.LinesIntersection(triangle.C, triangle.A, End, Begin));

                List<Triangle> tr = new List<Triangle>();
                List<Vector2d> lv = new List<Vector2d>();

                Triangulate triangulate = new Triangulate();

                if (triangulate.isPointInside(triangle, End))
                    Begin = End;

                int countPointIntersect = 0;

                if (!double.IsNaN(p1.X)) countPointIntersect++;
                if (!double.IsNaN(p2.X)) countPointIntersect++;
                if (!double.IsNaN(p3.X)) countPointIntersect++;

                if (countPointIntersect == 0)
                {
                    res.Add(t);
                }
                if (countPointIntersect == 3)
                {
                    if (MathVec.VectrCompare(p1, p2))
                    {
                        tr.Add(new Triangle(triangle.A, triangle.B, p3));
                        tr.Add(new Triangle(triangle.B, triangle.C, p3));
                    }
                    if (MathVec.VectrCompare(p2, p3))
                    {
                        tr.Add(new Triangle(triangle.A, p1, triangle.C));
                        tr.Add(new Triangle(triangle.B, triangle.C, p1));
                    }
                    if (MathVec.VectrCompare(p1, p3))
                    {
                        tr.Add(new Triangle(triangle.A, triangle.B, p2));
                        tr.Add(new Triangle(triangle.A, triangle.C, p2));
                    }
                }
                if (countPointIntersect == 2)
                {
                    if (double.IsNaN(p1.X))
                    {
                        if (!MathVec.VectrCompare(p2, p3))
                        {
                            tr.Add(new Triangle(p2, triangle.C, p3));
                            lv.AddRange(new List<Vector2d>() { triangle.A, triangle.B, p2, p3 });
                        }
                        else
                        {
                            tr.Add(t);
                        }
                    }
                    if (double.IsNaN(p2.X))
                    {
                        if (!MathVec.VectrCompare(p1, p3))
                        {
                            tr.Add(new Triangle(p3, triangle.A, p1));
                            lv.AddRange(new List<Vector2d>() { p1, triangle.B, triangle.C, p3 });
                        }
                        else
                        {
                            tr.Add(t);
                        }
                    }
                    if (double.IsNaN(p3.X))
                    {
                        if (!MathVec.VectrCompare(p1, p2))
                        {
                            tr.Add(new Triangle(p1, triangle.B, p2));
                            lv.AddRange(new List<Vector2d>() { p1, p2, triangle.C, triangle.A });
                        }
                        else
                        {
                            tr.Add(t);
                        }
                    }
                }
                if (countPointIntersect == 1)
                {
                    if (!double.IsNaN(p1.X))
                    {
                        tr.Add(new Triangle(triangle.A, p1, Begin));
                        tr.Add(new Triangle(triangle.B, p1, Begin));
                        tr.Add(new Triangle(triangle.B, triangle.C, Begin));
                        tr.Add(new Triangle(Begin, triangle.C, triangle.A));
                    }
                    if (!double.IsNaN(p2.X))
                    {
                        tr.Add(new Triangle(triangle.A, triangle.B, Begin));
                        tr.Add(new Triangle(triangle.B, p2, Begin));
                        tr.Add(new Triangle(p2, triangle.C, Begin));
                        tr.Add(new Triangle(Begin, triangle.C, triangle.A));
                    }
                    if (!double.IsNaN(p3.X))
                    {
                        tr.Add(new Triangle(triangle.A, Begin, p3));
                        tr.Add(new Triangle(triangle.A, Begin, triangle.B));
                        tr.Add(new Triangle(triangle.B, triangle.C, Begin));
                        tr.Add(new Triangle(Begin, triangle.C, p3));
                    }
                }

                List<Triangle> oo = null;
                if (lv.Count > 0)
                {
                    Triangulate ttt = new Triangulate(lv.ToArray());
                    oo = ttt.Triangles;
                }

                tr.RemoveAll(x => { return MathVec.VectrCompare(x.A, x.B) || MathVec.VectrCompare(x.B, x.C) || MathVec.VectrCompare(x.A, x.C); });

                res.AddRange(tr);
                if (oo != null)
                    res.AddRange(oo);

            }
            triangles.AddRange(res);
        }
        public void Triangulating(List<Triangle> tr1, List<Triangle> tr2, List<Vector2d> f1, List<Vector2d> f2)
        {
            List<Triangle> res1 = tr1.ToList();
            List<Triangle> res2 = tr2.ToList();

            for (int i = 0, j = 1; i < f2.Count; i++, j++)
            {
                if (j == f2.Count)
                    Subdiv(res1, f2[i], f2[0]);
                else
                    Subdiv(res1, f2[i], f2[j]);
            }

            for (int i = 0, j = 1; i < f1.Count; i++, j++)
            {
                if (j == f1.Count)
                    Subdiv(res2, f1[i], f1[0]);
                else
                    Subdiv(res2, f1[i], f1[j]);
            }

            Result1 = res1;
            Result2 = res2;
        }
        void CalcCenters(List<Vector2d> f1, List<Vector2d> f2, Operations operations)
        {
            if (operations == Operations.Interset)
            {
                for (int i = 0; i < Result1.Count; i++)
                    if (MathVec.Hit(Result1[i].Center, f2))
                        Intersect.Add(Result1[i]);

                for (int i = 0; i < Result2.Count; i++)
                    if (MathVec.Hit(Result2[i].Center, f1))
                        Intersect.Add(Result2[i]);
            }
            if (operations == Operations.Union)
            {
                for (int i = 0; i < Result1.Count; i++)
                    Union.Add(Result1[i]);

                for (int i = 0; i < Result2.Count; i++)
                        Union.Add(Result2[i]);
            }
            if (operations == Operations.Sub)
            {
                for (int i = 0; i < Result1.Count; i++)
                    if (!MathVec.Hit(Result1[i].Center, f2))
                        Intersect.Add(Result1[i]);
            }
        }
        void Verteces()
        {
            if (Intersect.Count > 0)
            {

            }
        }
        public void Draw()
        {
            GL.Begin(BeginMode.Triangles);
            GL.Color3(Color.Green);
            foreach(Triangle t in Intersect)
            {
                GL.Vertex2(t.A);
                GL.Vertex2(t.B);
                GL.Vertex2(t.C);
            }
            GL.End();

            GL.Begin(BeginMode.Triangles);
            GL.Color3(Color.Green);
            foreach (Triangle t in Union)
            {
                GL.Vertex2(t.A);
                GL.Vertex2(t.B);
                GL.Vertex2(t.C);
            }
            GL.End();

            GL.Begin(BeginMode.Triangles);
            GL.Color3(Color.Green);
            foreach (Triangle t in Sub)
            {
                GL.Vertex2(t.A);
                GL.Vertex2(t.B);
                GL.Vertex2(t.C);
            }
            GL.End();

            GL.Begin(BeginMode.LineStrip);
            GL.Color3(Color.Blue);
            foreach (Triangle t in Intersect)
            {
                GL.Vertex2(t.A);
                GL.Vertex2(t.B);
                GL.Vertex2(t.C);
            }
            GL.End();

            GL.Begin(BeginMode.LineStrip);
            GL.Color3(Color.Blue);
            foreach (Triangle t in Union)
            {
                GL.Vertex2(t.A);
                GL.Vertex2(t.B);
                GL.Vertex2(t.C);
            }
            GL.End();

            GL.Begin(BeginMode.LineStrip);
            GL.Color3(Color.Blue);
            foreach (Triangle t in Sub)
            {
                GL.Vertex2(t.A);
                GL.Vertex2(t.B);
                GL.Vertex2(t.C);
            }
            GL.End();
        }
    }

    public class Core
    {
        ulong _ids = 0;
        ulong _idLayers = 0;
        int _indL = -1;
        List<Layer> _layers = new List<Layer>();


        public ulong Ids { get { _ids++; return _ids; } private set { } }
        public List<Layer> Layers { get { return _layers; } private set { } }
        public int CountLayers { get { return Layers.Count; } private set { } }
        public Layer CurrentLayer { get; set; } = new Layer("-", "-");

        public void Del(Figure figure)
        {
            foreach (Layer L in _layers)
                L.DelFigure(figure);
        }
        public void Add(Figure figure)
        {
            CurrentLayer.AddFigure(figure);
        }
        public Figure Find(Vector2d point)
        {
            Figure f = null;
            int i = _layers.Count - 1;
            for (; i > -1; i--)
            {
                if ((f = _layers[i].FindFigure(point)) != null)
                    return f;
            }
            return null;
        }
        public Figure Find(string name)
        {
            Figure f = null;
            int i = _layers.Count - 1;
            for (; i > -1; i--)
            {
                if ((f = _layers[i].FindFigure(name)) != null)
                    return f;
            }
            return null;
        }

        // Methods for layer
        public void AddLayer()
        {
            _idLayers++;
            _layers.Add(new Layer("Layer " + _idLayers.ToString(), "L" + _idLayers.ToString()));
            CurrentLayer = _layers[_layers.Count - 1];
        }
        public bool DelLayer()
        {
            if (Layers.Count == 1)
            {
                MessageBox.Show("This last layer");
                return false;
            }
            Layers.Remove(CurrentLayer);
            CurrentLayer = Layers[Layers.Count - 1];
            return true;
        }
        public void FindLayer(string name)
        {
            var r = _layers.Find(x => x.Name == name);
            if (r != null)
                CurrentLayer = r;
        }
        public TreeNode[] NodesForTree()
        {
            List<TreeNode> r = new List<TreeNode>();

            for (int i = 0; i < _layers.Count; i++)
            {
                r.Add(new TreeNode(_layers[i].Name, _layers[i].NodesForTree()));

            }

            return r.ToArray();
        }


        public void Draw()
        {
            foreach (Layer L in _layers)
                L.Draw();
        }
    }
}
