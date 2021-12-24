﻿using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.ModelSpaceFragmentation
{
    class Main
    {
        readonly Application App;
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public Main(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }


        public void FragmentSpace()
        {
            ElementUtils elementUtils = new ElementUtils();
            Element element = elementUtils.GetCurrent(new PickedElement(Uidoc, Doc));

            BoundPoints boundPoints = new BoundPoints();
            boundPoints.GetPoints(element);
           
                ModelSpacePointsGenerator modelSpacePointsGenerator = 
                new ModelSpacePointsGenerator(BoundPoints.MinPoint, BoundPoints.MaxPoint);
            List<XYZ> spacePoints = modelSpacePointsGenerator.Generate();

            ModelSolid modelSolid = new ModelSolid(Doc);
            Dictionary<Element, List<Solid>> solids = modelSolid.GetSolids();

            PointsSeparator pointsSeparator = new PointsSeparator(spacePoints);
            pointsSeparator.Separate(Doc);

            Visualize(pointsSeparator);
        }

        

        void Visualize(PointsSeparator pointsSeparator)
        {
            VisiblePointsCreator visiblePointsCreator = new VisiblePointsCreator();
            visiblePointsCreator.Create(Doc, pointsSeparator.PassablePoints);

            visiblePointsCreator = new VisiblePointsCreator();
            visiblePointsCreator.Create(Doc, pointsSeparator.UnpassablePoints);

            GraphicOverwriter graphicOverwriter = new GraphicOverwriter();
            Color color = new Color(255, 0, 0);
            graphicOverwriter.OverwriteElementsGraphic(visiblePointsCreator.Instances, color);
        }

    }
}
