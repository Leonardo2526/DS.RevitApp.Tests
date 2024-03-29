﻿using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace DS.PipesCollisionsElliminator
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

        public void GetElementInfo()
        {
            ElementUtils elementUtils = new ElementUtils();
            Element element = elementUtils.GetCurrent(new PickedElement(Uidoc, Doc));

            elementUtils.OutputInfo(new ElementInfo(element, elementUtils));

        }

        public void InitiateProcess()
        {
            ElementUtils elementUtils = new ElementUtils();
            Element element = elementUtils.GetCurrent(new PickedElement(Uidoc, Doc));

            Collision collision = new Collision(App, Uiapp, Uidoc, Doc, elementUtils);
            IList<Element> collisionElements = collision.GetCollisions(element);

            if (collisionElements.Count == 0)
            {
                TaskDialog.Show("Revit", "No collision found!");
                return;
            }

            Path path = new Path(App, Uiapp, Uidoc, Doc, elementUtils);
            path.FindAvailable(element, collisionElements);


        }


        public void ExtractPoints()
        {
            ElementUtils elementUtils = new ElementUtils();
            Element element = elementUtils.GetCurrent(new PickedElement(Uidoc, Doc));

            GeneralPointExtractor generalPointExtractor = new GeneralPointExtractor(element);

            generalPointExtractor.GetGeneralPoints(out List<XYZ> points);

            VisiblePointsCreator linesCreator = new VisiblePointsCreator();
            linesCreator.Create(Doc, points);
        }


    }
}
