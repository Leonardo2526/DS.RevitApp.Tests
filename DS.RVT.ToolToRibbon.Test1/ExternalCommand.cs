﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace DS.RVT.ToolToRibbon.Test1
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;

            Intersection intersection = new Intersection();
            intersection.FindIntersections(uidoc, doc);

            TaskDialog.Show("Revit", "Done!");

            return Autodesk.Revit.UI.Result.Succeeded;
        }
       

      
    }
}