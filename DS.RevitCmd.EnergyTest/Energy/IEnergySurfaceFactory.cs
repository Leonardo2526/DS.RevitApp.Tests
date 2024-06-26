﻿using Autodesk.Revit.DB;

namespace DS.RevitCmd.EnergyTest
{
    internal interface IEnergySurfaceFactory
    {
        EnergySurface CreateEnergySurface(BoundarySegment segment, Curve baseCurve, double height);
    }
}