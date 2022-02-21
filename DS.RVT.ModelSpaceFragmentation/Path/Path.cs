using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Lines;
using FrancoGustavo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation
{
    static class Path
    {
        public static List<XYZ> PathRefinement(List<PathFinderNode> path)
        {

            //Convert path to revit coordinates                
            List<XYZ> pathCoords = new List<XYZ>();
            pathCoords.Add(ElementInfo.StartElemPoint);

            foreach (PathFinderNode item in path)
            {
                XYZ point = new XYZ(item.ANX, item.ANY, item.ANZ);
                XYZ pathpoint = ConvertToModel(point);

                double xx = Math.Abs(pathCoords[pathCoords.Count - 1].X - pathpoint.X);
                double xy = Math.Abs(pathCoords[pathCoords.Count - 1].Y - pathpoint.Y);
                double xz = Math.Abs(pathCoords[pathCoords.Count - 1].Z - pathpoint.Z);

                if (xx > 0.01 || xy > 0.01 || xz > 0.01)
                    pathCoords.Add(pathpoint);

            }

            //check min distance
            //double minDist = 1.5 * ElementSize.ElemDiameterF;
            //if (Math.Abs(pathCoords[pathCoords.Count - 2].X - pathCoords[pathCoords.Count - 1].X) <= minDist &&
            //    Math.Abs(pathCoords[pathCoords.Count - 2].Y - pathCoords[pathCoords.Count - 1].Y) <= minDist &&
            //    Math.Abs(pathCoords[pathCoords.Count - 2].Z - pathCoords[pathCoords.Count - 1].Z) <= minDist)
            //    pathCoords.RemoveAt(pathCoords.Count - 2);

            pathCoords.Add(ElementInfo.EndElemPoint);

            return pathCoords;

        }

        public static void ShowPath(List<XYZ> pathCoords)
        {

            //Show path with lines
            LineCreator lineCreator = new LineCreator();
            lineCreator.CreateCurves(new CurvesByPointsCreator(pathCoords));

            //MEP system changing
            RevitUtils.MEP.PypeSystem pypeSystem = new RevitUtils.MEP.PypeSystem(Main.Uiapp, Main.Uidoc, Main.Doc, Main.CurrentElement);
            pypeSystem.CreatePipeSystem(pathCoords);

            RevitUtils.MEP.ElementEraser elementEraser = new RevitUtils.MEP.ElementEraser(Main.Doc);
            elementEraser.DeleteElement(Main.CurrentElement);
        }


        private static XYZ ConvertToModel(XYZ point)
        {
            XYZ newpoint = point.Multiply(InputData.PointsStepF);
            newpoint += InputData.ZonePoint1;

            return newpoint;
        }
    }
}
