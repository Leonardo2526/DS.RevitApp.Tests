﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.MEP.SystemTree;
using System.Xml.Linq;
using DS.RevitLib.Utils.MEP;

namespace DS.RevitApp.SymbolPlacerTest
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application application = uiapp.Application;

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;

            var selector = new FamiliesSelectorTest(uidoc, doc, uiapp);
            selector.RunTest();
            List<MEPCurve> _targerMEPCurves = new List<MEPCurve>();
            _targerMEPCurves.AddRange(selector.MEPCurves);

            SymbolPlacerClient symbolPlacer = new SymbolPlacerClient(selector.Families, _targerMEPCurves, selector.Points);
            symbolPlacer.Run();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }


}