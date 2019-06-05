using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform.Windows;
using System.Xml;
using System.IO;

namespace ConsoleApp1
{
    public enum TypeFigures { None, Line, Rect, Circle, Curve, Polygon, Ellipsoid }
    public enum Operations { None, Union, Interset }
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
        public static bool VectrCompare(Vector2d a, Vector2d b, Vector2d p)
        {
            double mx, my, minx, miny;
            if (a.X <= b.X) { mx = b.X; minx = a.X; } else { mx = a.X; minx = b.X; };
            if (a.Y <= b.Y) { my = b.Y; miny = a.Y; } else { my = a.Y; miny = b.Y; };

            if (minx <= p.X && p.X <= mx && miny <= p.Y && p.Y <= my)
                return true;
            else
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
                _bezieVerteces = new Vector2d[3];
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

                        Y = p1.Y - p2.Y;
                        X = p2.X - p1.X;

                        if (Math.Abs(X) < 0.01)
                        {
                            if ((point.Y < Begin.Y) && (End.Y < point.Y))
                                return this;
                        }
                        if (Math.Abs(Y) < 0.01)
                        {
                            if ((point.X < Begin.X) && (End.X < point.X))
                                return this;
                        }

                        D = p1.X * p2.Y - p2.X * p1.Y;

                        double res = Y * point.X + X * point.Y + D;
                        if (Math.Abs(res) < 0.01)
                            return this;
                        //double x = point.X - Begin.X;
                        //double y = point.Y - Begin.Y;

                        //double z1 = End.X - Begin.X;
                        //if (Math.Abs(z1) < 0.1)
                        //    if ((point.Y < Begin.Y) && (End.Y < point.Y))
                        //        return this;


                        //double z2 = End.Y - Begin.Y;
                        //if (Math.Abs(z2) < 0.1)
                        //    if ((point.X < Begin.X) && (End.X < point.X))
                        //        return this;


                        //double l1 = x / z1;
                        //double l2 = y / z2;

                        //if (Math.Abs(l1 - l2) < 0.1)
                        //    return this;
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
        public bool IsRender { get; set; }
        public bool IsDrawCenter { get; set; }
        public float LineWidth { get; set; } = 1.0f;


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
        public List<Triangle> _triangles = new List<Triangle>();

        public void ReCalc()
        {
            var t = Matrix4d.CreateTranslation(MoveTo.X, MoveTo.Y, 0);
            var r = Matrix4d.CreateRotationZ(MathHelper.DegreesToRadians(Angle));
            var s = Matrix4d.Scale(ScaleTo.X, ScaleTo.Y, 1.0);

            var rt = Matrix4d.Mult(r, t);
            TRS = Matrix4d.Mult(s, rt);

            TRSI = TRS;//.Inverted();
            TRSI.Transpose();

            Edges = mainFigure.ToList();
            Manipulators = manipul.ToArray();

            if (Center != new Vector2d(0))
            {
                for (int i = 0; i < Edges.Count; i++)
                    Edges[i] = Edges[i] - Center;
                for (int i = 0; i < Manipulators.Length; i++)
                    Manipulators[i] = Manipulators[i] - Center;
            }

            for (int i = 0; i < Edges.Count; i++)
                Edges[i] = MultiplyMatrixAndEdge(mainFigure[i], TRSI) + Center;

            for (int i = 0; i < Manipulators.Length; i++)
                Manipulators[i] = MultiplyMatrixAndVector(manipul[i], TRSI) + Center;

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
            if (indCurrEdge > -1)
            {
                Vector2d c = (mainFigure[indCurrEdge].Begin + mainFigure[indCurrEdge].End) / 2.0;
                if (IsClosed)
                {
                    if (indCurrEdge == 0)
                    {
                        mainFigure[1].Begin = c;
                        mainFigure[mainFigure.Count - 1].End = c;
                        mainFigure.RemoveAt(0);
                        indCurrEdge = -1;
                        return true;
                    }
                    else if (indCurrEdge == mainFigure.Count - 1)
                    {
                        mainFigure[0].Begin = c;
                        mainFigure[mainFigure.Count - 2].End = c;
                        mainFigure.RemoveAt(indCurrEdge);
                        indCurrEdge = -1;
                        return true;
                    }
                    else
                    {
                        mainFigure[indCurrEdge + 1].Begin = c;
                        mainFigure[indCurrEdge - 1].End = c;
                        mainFigure.RemoveAt(indCurrEdge);
                        indCurrEdge = -1;
                        return true;
                    }
                }
                else
                {
                    ;
                }
            }
            if (indP1 == 1 || indP1 == 0)
            {
                if (IsClosed)
                {
                    if (indE1 == 0)
                    {
                        mainFigure[indE1].End = mainFigure[1].End;
                        mainFigure.RemoveAt(1);
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
                    ;
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
                    indPoint1 = i;
                    return true;
                }
            }

            if ((Manipulators[8] - mousePos).LengthSquared < 0.01)
                return true;

            indPoint1 = -1;
            return false;
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

    public class Helper
    {
        public Figure Add(TypeFigures type, List<Vector2d> mousePos)
        {
            Vector2d fmp = mousePos[0];
            Vector2d smp = mousePos[1];

            Figure f = new Figure();

            switch (type)
            {
                case TypeFigures.Line:
                    f.Edges = new List<Edge>()
                        {
                            new Edge(new Vector2d(fmp.X, fmp.Y), new Vector2d(smp.X, smp.Y))
                        };
                    f.Type = TypeFigures.Line;
                    f.Center = (fmp + smp) / 2.0;
                    f.TranslateToCenterCoordinates();
                    f.SortAtClock();
                    f.IsClosed = false;
                    f.ReCalc();
                    break;

                case TypeFigures.Rect:
                    f.Edges = new List<Edge>()
                        {
                            new Edge(new Vector2d(fmp.X, fmp.Y), new Vector2d(fmp.X, smp.Y)),
                            new Edge(new Vector2d(fmp.X, smp.Y), new Vector2d(smp.X, smp.Y)),
                            new Edge(new Vector2d(smp.X, smp.Y), new Vector2d(smp.X, fmp.Y)),
                            new Edge(new Vector2d(smp.X, fmp.Y), new Vector2d(fmp.X, fmp.Y))
                        };
                    f.Type = TypeFigures.Rect;
                    f.Center = (fmp + smp) / 2.0;
                    f.TranslateToCenterCoordinates();
                    f.SortAtClock();
                    f.ReCalc();
                    break;

                case TypeFigures.Ellipsoid:
                    f.Edges = CalcEllipsoid(fmp, smp, 360).ToList();
                    f.Type = TypeFigures.Ellipsoid;
                    f.Center = (fmp + smp) / 2.0;
                    f.TranslateToCenterCoordinates();
                    f.ReCalc();
                    break;

                case TypeFigures.Polygon:
                    f.Edges = CalcEllipsoid(fmp, smp, 6).ToList();
                    f.Type = TypeFigures.Polygon;
                    f.Center = (fmp + smp) / 2.0;
                    f.TranslateToCenterCoordinates();
                    f.ReCalc();
                    break;

                case TypeFigures.Curve:
                    break;

                default:
                    break;
            }

            return f;
        }

        List<Edge> CalcEllipsoid(Vector2d fmp, Vector2d smp)
        {
            List<Edge> le = new List<Edge>();

            Vector2d c = (fmp + smp) / 2.0;
            double r1 = Vector2d.Max(fmp, smp).X - c.X;
            double r2 = Vector2d.Max(fmp, smp).Y - c.Y;

            double theta = MathHelper.DegreesToRadians(60);
            for (int i = 0; i < 6; i++)
            {
                double th = theta * i;
                var e = new Edge();
                e.Begin = new Vector2d(r1 * Math.Cos(th), r2 * Math.Sin(th)) + c;
                double t = th + theta;
                e.End = new Vector2d(r1 * Math.Cos(t), r2 * Math.Sin(t)) + c;
                le.Add(e);
            }

            return le;
        }
        List<Edge> CalcEllipsoid(Vector2d fmp, Vector2d smp, int countLines)
        {
            List<Edge> le = new List<Edge>();

            Vector2d c = (fmp + smp) / 2.0;
            double r1 = Vector2d.Max(fmp, smp).X - c.X;
            double r2 = Vector2d.Max(fmp, smp).Y - c.Y;

            double theta = MathHelper.DegreesToRadians(360.0 / countLines);
            for (int i = 0; i < countLines; i++)
            {
                double th = theta * i;
                var e = new Edge();
                e.Begin = new Vector2d(r1 * Math.Cos(th), r2 * Math.Sin(th)) + c;
                double t = th + theta;
                e.End = new Vector2d(r1 * Math.Cos(t), r2 * Math.Sin(t)) + c;
                le.Add(e);
            }

            return le;
        }
    }

    public class Triangulate
    {
        private Vector2d[] points; //вершины нашего многоугольника
        private Triangle[] triangles; //треугольники, на которые разбит наш многоугольник
        private bool[] taken; //была ли рассмотрена i-ая вершина многоугольника

        public List<Triangle> Triangles
        {
            get
            {
                if (triangles != null)
                    return triangles.ToList();
                else
                    return new List<Triangle>();
            }
            set { }
        }

        public Triangulate(Vector2d[] points) //points - х и y координаты
        {
            this.points = points; //преобразуем координаты в вершины

            triangles = new Triangle[this.points.Length - 2];

            taken = new bool[this.points.Length];

            if (points.Length > 2)
                triangulate(); //триангуляция
        }

        private void triangulate() //триангуляция
        {
            int trainPos = 0; //
            int leftPoints = points.Length; //сколько осталось рассмотреть вершин

            //текущие вершины рассматриваемого треугольника
            int ai = findNextNotTaken(0);
            int bi = findNextNotTaken(ai + 1);
            int ci = findNextNotTaken(bi + 1);

            int count = 0; //количество шагов

            while (leftPoints > 3) //пока не остался один треугольник
            {
                if (isLeft(points[ai], points[bi], points[ci]) && canBuildTriangle(ai, bi, ci)) //если можно построить треугольник
                {
                    triangles[trainPos++] = new Triangle(points[ai], points[bi], points[ci]); //новый треугольник
                    taken[bi] = true; //исключаем вершину b
                    leftPoints--;
                    bi = ci;
                    ci = findNextNotTaken(ci + 1); //берем следующую вершину
                }
                else
                { //берем следующие три вершины
                    ai = findNextNotTaken(ai + 1);
                    bi = findNextNotTaken(ai + 1);
                    ci = findNextNotTaken(bi + 1);
                }

                if (count > points.Length * points.Length)
                { //если по какой-либо причине (например, многоугольник задан по часовой стрелке) триангуляцию провести невозможно, выходим
                    triangles = null;
                    break;
                }

                count++;
            }

            if (triangles != null) //если триангуляция была проведена успешно
                triangles[trainPos] = new Triangle(points[ai], points[bi], points[ci]);
        }

        private int findNextNotTaken(int startPos) //найти следущую нерассмотренную вершину
        {
            startPos %= points.Length;
            if (!taken[startPos])
                return startPos;

            int i = (startPos + 1) % points.Length;
            while (i != startPos)
            {
                if (!taken[i])
                    return i;
                i = (i + 1) % points.Length;
            }

            return -1;
        }

        private bool isLeft(Vector2d a, Vector2d b, Vector2d c) //левая ли тройка векторов
        {
            double abX = b.X - a.X;
            double abY = b.Y - a.Y;
            double acX = c.X - a.X;
            double acY = c.Y - a.Y;

            return abX * acY - acX * abY > 0;
        }

        private bool isPointInside(Vector2d a, Vector2d b, Vector2d c, Vector2d p) //находится ли точка p внутри треугольника abc
        {
            double ab = (a.X - p.X) * (b.Y - a.Y) - (b.X - a.X) * (a.Y - p.Y);
            double bc = (b.X - p.X) * (c.Y - b.Y) - (c.X - b.X) * (b.Y - p.Y);
            double ca = (c.X - p.X) * (a.Y - c.Y) - (a.X - c.X) * (c.Y - p.Y);

            return (ab >= 0 && bc >= 0 && ca >= 0) || (ab <= 0 && bc <= 0 && ca <= 0);
        }

        private bool canBuildTriangle(int ai, int bi, int ci) //false - если внутри есть вершина
        {
            for (int i = 0; i < points.Length; i++) //рассмотрим все вершины многоугольника
                if (i != ai && i != bi && i != ci) //кроме троих вершин текущего треугольника
                    if (isPointInside(points[ai], points[bi], points[ci], points[i]))
                        return false;
            return true;
        }

        public Vector2d[] getPoints() //возвращает вершины
        {
            return points;
        }

        public Triangle[] getTriangles() //возвращает треугольники
        {
            return triangles;
        }

    }

    public class Triangle //треугольник
    {
        private Vector2d a, b, c;

        public Vector2d A { get { return a; } set { } }
        public Vector2d B { get { return b; } set { } }
        public Vector2d C { get { return c; } set { } }

        public Triangle(Vector2d a, Vector2d b, Vector2d c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public override string ToString()
        {
            return a.ToString() + " " + b.ToString() + " " + c.ToString() + "\n"; 
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

            //ChangeList(F1, pointsIntersection, 1);
            //ChangeList(F2, pointsIntersection, 2);
            ChangeList(F1, pointsIntersection);
            ChangeList(F2, pointsIntersection);

            switch (operation)
            {
                case Operations.Interset:
                    bool exit = false, change = false;
                    Vertex v = pointsIntersection[0];
                    int i = F1.IndexOf(v);
                    int begin = i;
                    res.Add(v.V);
                    while (!exit)
                    {
                        i++;
                        v = F1[i];

                        if (v.IsInOtherFigure)
                            res.Add(v.V);

                        if (begin == i)
                            exit = true;

                        if (v.IsPointIntersection && (exit == false))
                        {
                            res.Add(v.V);
                            int ind = F2.FindIndex(x => x.V == v.V);
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

                                    if (ind != F2.Count - 1)
                                        ind++;
                                    else
                                        ind = 0;
                                    v = F2[ind];
                                }
                            }

                            i = -1;
                            //exit = true;
                        }                      
                    }
                    break;

                default:
                    break;
            }

            return res;
        }
       
        public void ChangeList(List<Vertex> inputList, List<Vertex> pointsInter, int numList)
        {
            int i = 1;
            if (numList == 1)
            {
                foreach (Vertex v in pointsInter)
                {
                    if ((v.IndexIn1 + i) == figure1.Count && (v.IndexIn1 != figure1.Count - i))
                    {
                        inputList.Add(v);
                    }
                    else
                    {
                        inputList.Insert(v.IndexIn1 + i, v);
                        i++;
                    }
                }
            }
            else
            {
                foreach (Vertex v in pointsInter)
                {
                    if ((v.IndexIn2 + i) == figure1.Count && (v.IndexIn2 != figure1.Count - i))
                    {
                        inputList.Add(v);
                    }
                    else
                    {
                        inputList.Insert(v.IndexIn2 + i, v);
                        i++;
                    }
                }
            }
        }
        public void ChangeList(List<Vertex> inputList, List<Vertex> pointsInter)
        {
            bool e = true;
            int j = 1, k = 0, i = 0;
            Vertex v = pointsInter[k];
            while (e)
            {
                if (j == inputList.Count && MathVec.PointOnEdge(inputList[i].V, inputList[0].V, v.V))
                {
                    inputList.Add(v);
                    i = -1;
                    j = 0;
                    k++;
                    if (k == pointsInter.Count)
                        return;
                    v = pointsInter[k];
                }
                else
                if (MathVec.PointOnEdge(inputList[i].V, inputList[j].V, v.V))
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

    class Program
    {
        static void Main(string[] args)
        {
            //List<Vector2d> P = new List<Vector2d>()
            //{
            //    new Vector2d(3, 3),
            //    new Vector2d(7, 3),
            //    new Vector2d(7, 6),
            //    new Vector2d(3, 7)
            //};
            //List<Vector2d> T = new List<Vector2d>()
            //{
            //    new Vector2d(2, 2),
            //    new Vector2d(7, 2),
            //    new Vector2d(2, 7)
            //};

            // second test
            //List<Vector2d> P = new List<Vector2d>()
            //{
            //    new Vector2d(1, 1),
            //    new Vector2d(4, 1),
            //    new Vector2d(4, 3),
            //    new Vector2d(1, 3)
            //};
            //List<Vector2d> T = new List<Vector2d>()
            //{
            //    new Vector2d(3, 2),
            //    new Vector2d(6, 2),
            //    new Vector2d(6, 4),
            //    new Vector2d(3, 4)
            //};

            //List<Vector2d> P = new List<Vector2d>()
            //{
            //    new Vector2d(1, 1),
            //    new Vector2d(5, 1),
            //    new Vector2d(4, 3),
            //    new Vector2d(3, 4),
            //    new Vector2d(1, 5)
            //};
            //List<Vector2d> T = new List<Vector2d>()
            //{
            //    new Vector2d(2, 2),
            //    new Vector2d(6, 2),
            //    new Vector2d(5, 4),
            //    new Vector2d(4, 6)
            //};

            List<Vector2d> P = new List<Vector2d>()
            {
                new Vector2d(0, 0),
                new Vector2d(4, 0),
                new Vector2d(7, 3),
                new Vector2d(4, 6),
                new Vector2d(0, 6)
            };
            List<Vector2d> T = new List<Vector2d>()
            {
                new Vector2d(9, 6),
                new Vector2d(4, 9),
                new Vector2d(2, 3),
                new Vector2d(2, -2),
                new Vector2d(9, -2)
            };

            Console.WriteLine("P:");
            foreach (Vector2d v in P)
                Console.WriteLine(v);
            Console.WriteLine();
            Console.WriteLine("T:");
            foreach (Vector2d v in T)
                Console.WriteLine(v);

            Modificators modificators = new Modificators(P, T);
            modificators.Operation = Operations.Interset;

            var r = modificators.Result();
            Console.WriteLine("Res:");
            foreach (var v in r)
                Console.WriteLine(v);


            Console.ReadLine();
        }
    }
}
