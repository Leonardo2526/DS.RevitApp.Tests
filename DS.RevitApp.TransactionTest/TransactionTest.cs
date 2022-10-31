﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.ModelCurveUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.TransactionTest

{ 
    internal class TransactionTest
    {    
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        public TransactionTest(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
        }

        public void Run()
        {
            var path = new List<XYZ>
            {
                new XYZ(0,0,0),
                new XYZ(1,0,0),
                new XYZ(1,1,0),
                new XYZ(2,1,0),
                new XYZ(2,0,0)
            };

            using (TransactionGroup trg = new TransactionGroup(_doc))
            {
                trg.Start();

                var trb = new TransactionBuilder<Element>(_doc);
                trb.Build(() => ShowLines(path), "show lines");
                trb.Build(() => Showcurves(path), "show curves");

                _uiDoc.RefreshActiveView();

                trg.RollBack();
            }

            //TaskDialog.Show("DSMessage", "Hello");
        }

        private void ShowLines(List<XYZ> path)
        {
            var mcreator = new ModelCurveCreator(_doc);
            for (int i = 0; i < path.Count - 1; i++)
            {
                mcreator.Create(path[i], path[i + 1]);
            }
        }

        private void Showcurves(List<XYZ> path)
        {
            Reference reference = _uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Select element");
            var mEPCurve = _doc.GetElement(reference) as MEPCurve;

            var builder = new BuilderByPoints(mEPCurve, path).BuildMEPCurves().WithElbows();
        }
    }
}
