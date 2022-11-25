﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitApp.TransactionTest.View;

namespace DS.RevitApp.TransactionTest
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

            var startWindow = new TestWindow(doc, uidoc, uiapp);
            //var startWindow = new TransactionWindow(doc, uidoc, uiapp);
            startWindow.Show();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}