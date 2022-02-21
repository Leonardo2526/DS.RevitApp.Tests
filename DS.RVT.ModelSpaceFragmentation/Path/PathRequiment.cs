using Autodesk.Revit.DB;
using DS.PathSearch.GridMap;
using System;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PathRequiment : IPathRequiment
    {
        static double ClearanceDistance = 50;

        public byte Clearance
        { 
            //get { return 0; }
            get
            {
                double ClearanceF = UnitUtils.Convert(ClearanceDistance,
                             DisplayUnitType.DUT_MILLIMETERS,
                             DisplayUnitType.DUT_DECIMAL_FEET);

                double ElementWidthHalf = ElementSize.ElemDiameterF;
                //double ElementHeghtHalf = (ElementSize.ElemDiameterF / 2);

                double clearanceFull = ClearanceF + ElementWidthHalf;

                return (byte)Math.Round(clearanceFull / Main.PointsStepF);
            }
        }
        public byte MinAngleDistance 
        {
            //get { return 0; }
            get
            {
                double Rad = 1.5 * ElementSize.ElemDiameterF + 0.01;
                return (byte)Math.Round(Rad / Main.PointsStepF);
            }
        }
    }
}
