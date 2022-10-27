﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Points;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DS.RevitApp.Test
{
    internal class SolidOperationTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private XYZ _dir;
        private TransactionBuilder<Element> _trb;

        public SolidOperationTest(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
        }

        public void Run()
        {
            _trb = new TransactionBuilder<Element>(_doc);

            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            var element = _doc.GetElement(reference);

            var solid = ElementUtils.GetSolid(element);
            _dir = ElementUtils.GetMainDirection(element);
            XYZ center = solid.ComputeCentroid();
            center.Show(_doc);

            List<Curve> faceCurves = GetFaceCurves(solid);
            List<Curve> offsetCurves = GetOffsetCurves(solid, faceCurves);

            List<Line> lines = offsetCurves.OfType<Line>().ToList();
            List<Curve> connectedCurves = lines.Any() ? 
                new LinesConnector(lines).Connect().Cast<Curve>().ToList() : 
                offsetCurves;

            //_trb.Build(() => connectedCurves.ForEach(obj => obj.Show(_doc)), "show offset");
            //_uiDoc.RefreshActiveView();

            //var clonedSolid = SolidUtils.Clone(solid);
            //Plane plane = Plane.CreateByNormalAndOrigin(_dir, center);
            //BooleanOperationsUtils.CutWithHalfSpaceModifyingOriginalSolid(clonedSolid, plane);

            Solid exSolid = CreateExtrudedSolid(connectedCurves);
            _trb.Build(() => exSolid.ShowEdges(_doc), "show solid");
        }

        private List<Curve> GetFaceCurves(Solid solid)
        {
            var faceCurves = new List<Curve>();

            Face face1 = solid.Faces.get_Item(0);
            EdgeArray edgeArray1 = face1.EdgeLoops.get_Item(0);
            for (int i = 0; i < 4; i++)
            {
                Edge edge = edgeArray1.get_Item(i);
                var curve = edge.AsCurve();
                faceCurves.Add(curve);
            }

            return faceCurves;
        }

        private List<Curve> GetOffsetCurves(Solid solid, List<Curve> curves)
        {
            var offsectCurves = new List<Curve>();
            Face face1 = solid.Faces.get_Item(0);
            EdgeArray edgeArray1 = face1.EdgeLoops.get_Item(0);

            Face face2 = solid.Faces.get_Item(1);
            EdgeArray edgeArray2 = face2.EdgeLoops.get_Item(0);

            var edgeArrayPoints1 = edgeArray1.GetPolygon();
            var edgeArrayPoints2 = edgeArray2.GetPolygon();
            XYZ edgeAttayCenter1 = XYZUtils.GetAverage(edgeArrayPoints1);
            XYZ edgeAttayCenter2 = XYZUtils.GetAverage(edgeArrayPoints2);
            XYZ dir = edgeAttayCenter1 - edgeAttayCenter2;

            double offset = 50.mmToFyt2();

            foreach (var curve in curves)
            {
                var offsetCurve = curve.CreateOffset(offset, dir);
                offsectCurves.Add(offsetCurve);
            }
           
            return offsectCurves;
        }

        private Solid CreateExtrudedSolid(List<Curve> curves)
        {           
            var cloop = new CurveLoop();

            foreach (var curve in curves)
            {
                cloop.Append(curve);
            }


            IList<CurveLoop> loop = new List<CurveLoop>() { cloop };

            return GeometryCreationUtilities.CreateExtrusionGeometry(loop, _dir, 1);
        }
    }
}
