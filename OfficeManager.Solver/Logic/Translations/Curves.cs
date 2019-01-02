using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HOK.OfficeManager.Formats;
using HOK.OfficeManager.Logic;
using Rhino;
using Rhino.Geometry;

namespace HOK.OfficeManager.Logic.Translations
{
    public static class Curves
    {
        /// <summary>
        /// Converts 2D rhino curves to quadratic bezier svg data.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="normalized">Should values be scaled to fit in a one unit bounding square?</param>
        /// <param name="bounds">Optional rectangular region to reference when creating data. If null, will use curve's bounding box.</param>
        /// <returns></returns>
        public static string ToSvg(Curve curve, Curve bounds, bool normalized)
        {
            BezierCurve[] bezierCurves = BezierCurve.CreateCubicBeziers(curve, 0.1, 0.1);

            var curveBox = curve.GetBoundingBox(Plane.WorldXY);
            var curveBounds = bounds == null ? new CurveBounds(new Rectangle3d(new Plane(curveBox.Center, Vector3d.XAxis, Vector3d.YAxis), curveBox.Min, curveBox.Max).ToNurbsCurve()) : new CurveBounds(bounds);

            var precision = 3;

            var output = "M";

            foreach (BezierCurve bezier in bezierCurves)
            {
                var startPt = Points.ToSvgCoordinates(bezier.GetControlVertex3d(0), curveBounds);
                var leftAnchor = Points.ToSvgCoordinates(bezier.GetControlVertex3d(1), curveBounds);
                var rightAnchor = Points.ToSvgCoordinates(bezier.GetControlVertex3d(2), curveBounds);

                var addendum = Points.PointToString(startPt, precision) + " C" + Points.PointToString(leftAnchor, precision) + " " + Points.PointToString(rightAnchor, precision) + " ";

                output = output + addendum;

                if (bezier == bezierCurves.Last())
                {
                    output = output + Points.PointToString(Points.ToSvgCoordinates(bezier.ToNurbsCurve().PointAtEnd, curveBounds), precision);
                }
            }

            return output;
        }

        public static Curve ToNurbsCurve(string svgdata)
        {
            var splitData = svgdata.Replace("M", " ").Split('C');

            var ptData = new List<List<Point3d>>();

            //Split string of point data and convert to Rhino points.
            for (int i = 0; i < splitData.Length; i++)
            {
                var pts = splitData[i].Split(' ');

                var ptCache = new List<Point3d>();

                foreach (string pt in pts)
                {
                    if (pt.Length > 2)
                    {
                        var svgPt = Points.StringToPoint(pt);
                        var correctedPt = new Point3d(svgPt.X, svgPt.Y * -1, 0);

                        ptCache.Add(correctedPt);
                    }
                }

                //Utils.Debug.PrintList(ref ptCache, " / ");

                ptData.Add(ptCache);
            }

            //Create bezier curves from points.
            var bezierSegments = new List<Curve>();

            for (int i = 0; i < ptData.Count - 1; i++)
            {
                var pts = new List<Point3d>();

                if (i == 0)
                {
                    pts.Add(ptData[i][i]);     //Start point.
                    pts.Add(ptData[i + 1][0]); //Left control point.
                    pts.Add(ptData[i + 1][1]); //Right control point.
                    pts.Add(ptData[i + 1][2]); //End point.

                    bezierSegments.Add(new BezierCurve(pts).ToNurbsCurve());

                    continue;
                }

                pts.Add(ptData[i][2]);     //Start point.
                pts.Add(ptData[i + 1][0]); //Left control point.
                pts.Add(ptData[i + 1][1]); //Right control point.
                pts.Add(ptData[i + 1][2]); //End point.

                bezierSegments.Add(new BezierCurve(pts).ToNurbsCurve());
            }

            //Join bezier segments and return final curve.
            var output = bezierSegments.Count > 1 ? Curve.JoinCurves(bezierSegments)[0] : bezierSegments[0];

            return output;
        }
    }
}
