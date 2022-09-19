﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointModel;
using DS.RevitApp.Test.PathFindTest.PathFinders;
using DS.RevitApp.Test.PathFindTest.Solution;
using DS.RevitApp.Test.PathFindTest.Solution.Creators;
using DS.RevitApp.Test.TransformTest;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPBypass.ElementsTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

namespace DS.RevitApp.Test.PathFindTest.ConnectionPointService
{
    internal class ConnectionPointsClient
    {

        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private readonly IPathFinder _pathFinder;
        private MEPSystemModel _mEPSystemModel;
        private SketchSolutionModel _sketchSolutionModel;
        private MEPCurveModel _mEPCurveModel;
        private ConnectionPoint _point1;
        private ConnectionPoint _point2;
        private string _transactionPrefix;


        public ConnectionPointsClient(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = _uidoc.Document;
            _pathFinder = new SimplePathFinder(1, 1);
        }
        

        public void Run()
        {
            //Get MEPSystemModel
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element");
            MEPCurve element = _doc.GetElement(reference) as MEPCurve;

            var mEPSystemBuilder = new SimpleMEPSystemBuilder(element);
            _mEPSystemModel = mEPSystemBuilder.Build();

            //var (point1, point2) = GetPointsManually();
            //List<XYZ> points = pathGenerator.FindPath(point1.Value, point2.Value);
            List<XYZ>  path = GetPath();

            //Show path
            var creator = new ModelCurveCreator(_doc);
            var lines = new List<Line>();
            for (int i = 0; i < path.Count - 1; i++)
            {
                creator.Create(path[i], path[i + 1]);
                var line = Line.CreateBound(path[i], path[i + 1]);
                lines.Add(line);
            }

            var lineModels = BuildLineModels(lines, _sketchSolutionModel);

            var elems = _mEPSystemModel.Root.GetElements(_point1.Element, _point2.Element);
            var allAccIds = _mEPSystemModel.Root.Accessories.Select(a => a.Id);
            var accecoriesSpan = elems.Where(obj => allAccIds.Contains(obj.Id)).OfType<FamilyInstance>().ToList();

            var builder = new FamToLineMultipleBuilder(accecoriesSpan, lineModels, path, 50.mmToFyt2(), null, _mEPCurveModel);
            List<FamToLineTransformModel> transformModels = builder.Build().Cast<FamToLineTransformModel>().ToList();


            foreach (var model in transformModels)
            {
                MEPElementUtils.Disconnect(model.SourceObject);
                TransformElement(model.SourceObject, model.Transforms);
            }
            
        }

        private (KeyValuePair<Element, XYZ> point1, KeyValuePair<Element, XYZ> point2) GetPointsManually()
        {
            var selector = new ConnectionElementSelector(_uidoc);
            var element1 = selector.Select("Element1");
            var element2 = selector.Select("Element2");

            return (element1, element2);
        }

        private List<XYZ> GetPath()
        {
            var strategy = new LoopCheckPointsStrategy(_pathFinder, null, _mEPSystemModel);
            var solutionCreator = new SketchSolutionCreator(strategy, _mEPSystemModel);
            _sketchSolutionModel = solutionCreator.Create() as SketchSolutionModel;
            _point1 = _sketchSolutionModel.Point1 as ConnectionPoint;
            _point2 = _sketchSolutionModel.Point2 as ConnectionPoint;

            return _sketchSolutionModel.Path;
        }

        private List<LineModel> BuildLineModels(List<Line> lines, SketchSolutionModel sketchSolutionModel)
        {
            var startLineModel = GetStartLineModel(sketchSolutionModel);

            Basis initBasis = startLineModel.Basis.Clone();

            var lineModels = new List<LineModel>();
            foreach (var line in lines)
            {
                var trModel = new BasisLineTransformBuilder(initBasis, line).Build();
                initBasis.Transform(trModel.Transforms);

                var lineModel = new LineModel(line, initBasis);
                lineModels.Add(lineModel);
                initBasis = initBasis.Clone();
            }

            return lineModels;
        }

        private LineModel GetStartLineModel(SketchSolutionModel sketchSolutionModel)
        {
            var p1 = sketchSolutionModel.Point1 as ConnectionPoint;
            MEPCurve startMEPCurve = null;
            if (p1.Element is MEPCurve)
            {
                startMEPCurve = p1.Element as MEPCurve;
            }
            else
            {
                while (startMEPCurve is null)
                {
                    var elems = ConnectorUtils.GetConnectedElements(p1.Element);
                    var mcurves = elems.OfType<MEPCurve>().ToList();
                    if (mcurves is not null && mcurves.Any())
                    {
                        startMEPCurve = mcurves.First();
                    }
                }
            }

            _mEPCurveModel = new MEPCurveModel(startMEPCurve, new SolidModel(ElementUtils.GetSolid(startMEPCurve)));
            return new LineModel(startMEPCurve);
        }

        private Element TransformElement(Element element, TransformModel transformModel)
        {
            using (Transaction transNew = new Transaction(_doc, _transactionPrefix + "Transform " + element.Id))
            {
                try
                {
                    transNew.Start();
                    if (transformModel.MoveVector is not null)
                    {
                        ElementTransformUtils.MoveElement(_doc, element.Id, transformModel.MoveVector);
                    }
                    foreach (var rot in transformModel.Rotations)
                    {
                        ElementTransformUtils.RotateElement(_doc, element.Id, rot.Axis, rot.Angle);
                    }
                }

                catch (Exception e)
                { return null; }

                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

            return element;
        }
    }
}
