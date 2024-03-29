﻿using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RVT.AutoPipesCoordinarion
{
    class RevitElements
    {
        readonly Application App;
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public RevitElements(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }


        public void ModifyElements1(Element elementA, Element elementB)
        // Find collisions between elements and a selected element by solid
        {
            double offset = 100;

            XYZ newVector;

            //Uidoc.RefreshActiveView();

            bool solved = false;

            int i;
            for (offset = 100; offset < 1000; offset += 100)
            {
                Uidoc.RefreshActiveView();

                DocEvent docEvent = new DocEvent(Uiapp);
                docEvent.RegisterEvent();

                newVector = GetOffset(elementA, elementB, offset, false);
                CreateTransaction(elementB.Id, newVector);

                Collision collision = new Collision(App, Uiapp, Uidoc, Doc);
                if (collision.CheckCollisionsWithModifiedElements(docEvent.modifiedElementsIds) == false)
                {
                    solved = true;
                    break;
                }

                //Try to move elementB in another position
                newVector = GetOffset(elementA, elementB, offset, true);
                CreateTransaction(elementB.Id, newVector);


                // remove the event.
                Uiapp.Application.DocumentChanged -= docEvent.application_DocumentChanged;
            }


            ModifyElements1(elementA, elementB);


        }

        public void ModifyElements0(Element elementA, Element elementB, double offset)
        // Find collisions between elements and a selected element by solid
        {           
            DocEvent docEvent = new DocEvent(Uiapp);
            docEvent.RegisterEvent();

            XYZ newVector = GetOffset(elementA, elementB, offset, false);
            CreateTransaction(elementB.Id, newVector);

            //Uidoc.RefreshActiveView();

            bool solved = false;

            Collision collision = new Collision(App, Uiapp, Uidoc, Doc);
            if (collision.CheckCollisionsWithModifiedElements(docEvent.modifiedElementsIds) == true)
            {
                DocEvent docEvent1 = new DocEvent(Uiapp);
                docEvent1.RegisterEvent();

                newVector = GetOffset(elementA, elementB, offset, true);
                CreateTransaction(elementB.Id, newVector);                              

                Collision collision1 = new Collision(App, Uiapp, Uidoc, Doc);
                if (collision1.CheckCollisionsWithModifiedElements(docEvent1.modifiedElementsIds) == true)
                {
                    newVector = GetOffset(elementA, elementB, 500, false);
                    CreateTransaction(elementB.Id, newVector);
                }
                Uiapp.Application.DocumentChanged -= docEvent1.application_DocumentChanged;
            }
            else
                solved = true;
     

          
            // remove the event.
            Uiapp.Application.DocumentChanged -= docEvent.application_DocumentChanged;

            
          //if (solved == false)
            //    ModifyElements(elementA, elementB, offset += 100);
          
        }

        public void ModifyElements(Element elementA, Element elementB, ref ICollection<ElementId> modifiedElementsIds)
        // Find collisions between elements and a selected element by solid
        {
            double offset = 100;

            DocEvent docEvent = new DocEvent(Uiapp);
            docEvent.RegisterEvent();

            XYZ newVector = GetOffset(elementA, elementB, offset, false);
            CreateTransaction(elementB.Id, newVector);

            modifiedElementsIds = docEvent.modifiedElementsIds;

            Collision collision = new Collision(App, Uiapp, Uidoc, Doc);
            if (collision.CheckCollisionsWithModifiedElements(modifiedElementsIds) == true)
            {
                //docEvent.modifiedElementsIds.Clear();
                //modifiedElementsIds = docEvent.modifiedElementsIds;
                newVector = GetOffset(elementA, elementB, offset, true);
                CreateTransaction(elementB.Id, newVector);
            }
            /*
            if (solved == false)
                TaskDialog.Show("Revit", "No available path");
            else
                TaskDialog.Show("Revit", "Element moved");
            */
            // remove the event.
            Uiapp.Application.DocumentChanged -= docEvent.application_DocumentChanged;


            
        }

        public void check(ICollection<ElementId> modifiedElementsIds, Element elementA, Element elementB)
        {
            Collision collision = new Collision(App, Uiapp, Uidoc, Doc);
            if (collision.CheckCollisionsWithModifiedElements(modifiedElementsIds) == true)
            {
                XYZ newVector = GetOffset(elementA, elementB, 500, false);
                CreateTransaction(elementB.Id, newVector);
            }
        }

        void CreateTransaction(ElementId elementBId, XYZ newVector)
        {
            using (Transaction transNew = new Transaction(Doc, "newTransaction"))
            {
                try
                {
                    transNew.Start();
                    ElementTransformUtils.MoveElement(Doc, elementBId, newVector);
                    
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }
        }



        public XYZ CheckModifiesElements(Element elementA, Element elementB,
            ICollection<ElementId> modifiedElementsIds, ElementIntersectsSolidFilter intersectionFilter, double offset)
        {
            FilteredElementCollector collector = new FilteredElementCollector(Doc, modifiedElementsIds);
            collector.WherePasses(intersectionFilter);

            //Get all moved elements with solidA intersection
            IList<Element> newIntersectedElements = collector.ToElements();

            //Check modified elements
            if (newIntersectedElements.Count > 0)
            {
                return GetOffset(elementA, elementB, offset, true);
            }

            else
                return null;
        }


        public XYZ GetOffset(Element ElementA, Element ElementB, double offset, bool changeDirection)
        {
            GetPoints(ElementA, out XYZ startPointA, out XYZ endPointA, out XYZ centerPointElementA);
            GetPoints(ElementB, out XYZ startPointB, out XYZ endPointB, out XYZ centerPointElementB);

            double alfa;
            double beta;
            double offsetF;

            double fullOffsetX = 0;
            double fullOffsetY = 0;
            double fullOffsetZ = 0;

            //Get pipes sizes
            Pipe pipeA = ElementA as Pipe;
            double pipeSizeA = pipeA.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();
            Pipe pipeB = ElementB as Pipe;
            double pipeSizeB = pipeB.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();

            offsetF = UnitUtils.Convert(offset / 1000,
                                   DisplayUnitType.DUT_METERS,
                                   DisplayUnitType.DUT_DECIMAL_FEET);
            //check correct direction
            int K = 1;
            if (changeDirection == true)
                K = -1;

            if (Math.Round(startPointB.X, 3) == Math.Round(endPointB.X, 3))
            {

                fullOffsetX = (pipeSizeA + pipeSizeB) / 2 +
             K * (centerPointElementA.X - centerPointElementB.X) + offsetF;
            }
            else if (Math.Round(startPointB.Y, 3) == Math.Round(endPointB.Y, 3))
            {
                fullOffsetY = (pipeSizeA + pipeSizeB) / 2 +
             K * (centerPointElementA.Y - centerPointElementB.Y) + offsetF;
            }
            else
            {
                double A = (endPointB.Y - startPointB.Y) / (endPointB.X - startPointB.X);

                alfa = Math.Atan(A);
                double angle = alfa * (180 / Math.PI);
                beta = 90 * (Math.PI / 180) - alfa;
                angle = beta * (180 / Math.PI);

                double AX = Math.Cos(beta);
                double AY = Math.Sin(beta);

                double H = centerPointElementB.Y + A * (centerPointElementA.X - centerPointElementB.X);

                double deltaCenter = (centerPointElementA.Y - H) * Math.Cos(alfa);

                double fullOffset = ((pipeSizeA + pipeSizeB) / 2 - K * deltaCenter + offsetF);

                //Get full offset of element B from element A              
                fullOffsetX = fullOffset * AX;
                fullOffsetY = -fullOffset * AY;
                fullOffsetZ = 0;

            }


            XYZ XYZoffset = new XYZ(K * fullOffsetX, K * fullOffsetY, K * fullOffsetZ);

            return XYZoffset;
        }


        public void GetPoints(Element element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint)
        {
            //get the current location           
            LocationCurve lc = element.Location as LocationCurve;
            Curve c = lc.Curve;
            c.GetEndPoint(0);
            c.GetEndPoint(1);

            startPoint = c.GetEndPoint(0);
            endPoint = c.GetEndPoint(1);
            centerPoint = new XYZ((startPoint.X + endPoint.X) / 2,
                (startPoint.Y + endPoint.Y) / 2,
                (startPoint.Z + endPoint.Z) / 2);

        }


        public void CreateModelLine(XYZ startPoint, XYZ endPoint)
        {
            Line geomLine = Line.CreateBound(startPoint, endPoint);

            // Create a geometry plane in Revit application
            XYZ p1 = startPoint;
            XYZ p2 = endPoint;
            XYZ p3 = p2 + XYZ.BasisZ;
            Plane geomPlane = Plane.CreateByThreePoints(p1, p2, p3);

            using (Transaction transNew = new Transaction(Doc, "CreateModelLine"))
            {
                try
                {
                    transNew.Start();

                    // Create a sketch plane in current document
                    SketchPlane sketch = SketchPlane.Create(Doc, geomPlane);

                    // Create a ModelLine element using the created geometry line and sketch plane
                    ModelLine line = Doc.Create.NewModelCurve(geomLine, sketch) as ModelLine;
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }

                transNew.Commit();
            }
        }

        public XYZ GetPointInFeets(XYZ entryPoint)
        {
            double XF = UnitUtils.Convert(entryPoint.X / 1000,
                                           DisplayUnitType.DUT_METERS,
                                           DisplayUnitType.DUT_DECIMAL_FEET);
            double YF = UnitUtils.Convert(entryPoint.Y / 1000,
                                            DisplayUnitType.DUT_METERS,
                                            DisplayUnitType.DUT_DECIMAL_FEET);
            double ZF = UnitUtils.Convert(entryPoint.Z / 1000,
                                            DisplayUnitType.DUT_METERS,
                                            DisplayUnitType.DUT_DECIMAL_FEET);

            XYZ outPoint = new XYZ(XF, YF, ZF);

            return outPoint;
        }


    }
}
