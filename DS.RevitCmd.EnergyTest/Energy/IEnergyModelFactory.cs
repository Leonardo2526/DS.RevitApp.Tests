﻿using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCmd.EnergyTest
{
    public interface IEnergyModelFactory
    {
        EnergyModel Create(Space space);
    }
}
