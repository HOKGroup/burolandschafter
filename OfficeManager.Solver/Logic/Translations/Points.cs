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
    public static class Points
    {
        public static string PointToString(Point3d pt, int n)
        {
            return Math.Round(pt.X, n) + "," + Math.Round(pt.Y, n);
        }

        public static Point3d StringToPoint(string ptData)
        {
            var coords = ptData.Replace(" ", "").Split(',').ToList();

            //Logic.Utils.Debug.PrintList(ref coords);

            return new Point3d(Convert.ToDouble(coords[0]), Convert.ToDouble(coords[1]), 0);
        }

        public static Point3d ToSvgCoordinates(Point3d pt, CurveBounds bounds)
        {
            var refPt = new Point3d(bounds.XMin, bounds.YMax, 0);

            bounds.BaseCurve.Transform(Transform.Scale(refPt, 2.5));

            PointContainment anchorCheck = bounds.BaseCurve.Contains(pt, Plane.WorldXY, 0.01);
            var inside = anchorCheck == PointContainment.Inside || anchorCheck == PointContainment.Coincident;

            var newX = Math.Abs(pt.X - refPt.X);
            var newY = inside ? Math.Abs(pt.Y - refPt.Y) : Math.Abs(pt.Y - refPt.Y) * -1;

            return new Point3d(newX, newY, 0);
        }
    }
}
