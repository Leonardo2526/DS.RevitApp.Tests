﻿using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Path;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Points
{
    class PointConvertor
    {
        static readonly double PointsStepF = ModelSpacePointsGenerator.PointsStepF;


        public static XYZ StepPointToXYZ(StepPoint stepPoint)
        {
            XYZ refPoint = new XYZ(stepPoint.X * PointsStepF,
            stepPoint.Y * PointsStepF,
            stepPoint.Z * PointsStepF);

            return new XYZ(PointsInfo.MinBoundPoint.X + refPoint.X, 
                PointsInfo.MinBoundPoint.Y + refPoint.Y,
                PointsInfo.MinBoundPoint.Z + refPoint.Z);

        }

        public static StepPoint XYZToStepPoint(XYZ point)
        {
            XYZ refPoint = new XYZ(point.X - PointsInfo.MinBoundPoint.X,
                point.Y - PointsInfo.MinBoundPoint.Y,
                point.Z - PointsInfo.MinBoundPoint.Z);

            return new StepPoint((int)Math.Round(refPoint.X / PointsStepF),
            (int)Math.Round(refPoint.Y / PointsStepF),
            (int)Math.Round(refPoint.Z / PointsStepF));

        }

        public static XYZ StepPointToXYZByPoint(XYZ basePoint, StepPoint stepPoint)
        {
             XYZ point = new XYZ(basePoint.X + stepPoint.X * PointsStepF,
                    basePoint.Y + stepPoint.Y * PointsStepF,
                    basePoint.Z + stepPoint.Z * PointsStepF);

            return point;
        }
    }
}