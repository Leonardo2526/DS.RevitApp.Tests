﻿using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.WaveAlgorythm
{
    class Cell
    {
        readonly Application App;
        readonly UIApplication Uiapp;
        readonly Document Doc;
        readonly UIDocument Uidoc;

        public Cell(Application app, UIApplication uiapp, Document doc, UIDocument uidoc)
        {
            App = app;
            Uiapp = uiapp;
            Doc = doc;
            Uidoc = uidoc;
        }

        public double CellSize { get; set; }
        public double AreaSize { get; set; }

        double cellSizeF;

        List<FamilyInstance> cells = new List<FamilyInstance>();
        ICollection<ElementId> cellsIds = new List<ElementId>();

        //List for cells XYZ which collide with other model elements
        List<XYZ> ICLocations = new List<XYZ>();

        public void GetCells()
        {
            ICLocations = new List<XYZ>();
            AreaSize = 1000;
            double areaSizeF = UnitUtils.Convert(AreaSize / 1000,
                                  DisplayUnitType.DUT_METERS,
                                  DisplayUnitType.DUT_DECIMAL_FEET);

            XYZ corner1 = new XYZ(0, 0, 0);
            XYZ corner2 = new XYZ(areaSizeF, areaSizeF, 0);

            CellSize = 50;
            cellSizeF = UnitUtils.Convert(CellSize / 1000,
                                  DisplayUnitType.DUT_METERS,
                                  DisplayUnitType.DUT_DECIMAL_FEET);

            List<Family> families = new List<Family>();
            Family family = new Family(App, Uiapp, Doc, Uidoc);

            //List for cells XYZ
            List<XYZ> cellsLocations = new List<XYZ>();

            //Open transaction cells creation
            using (Transaction transNew = new Transaction(Doc, "newTransaction"))
            {
                try
                {
                    transNew.Start();
                    for (double Z = 0; Z <= corner2.Z; Z += cellSizeF)
                    {
                        for (double Y = 0; Y <= corner2.Y; Y += cellSizeF)
                        {
                            for (double X = 0; X <= corner2.X; X += cellSizeF)
                            {
                                XYZ centralPoint = new XYZ(X, Y, Z);
                                cellsLocations.Add(centralPoint);
                                families.Add(family);
                                family.CreateCell(centralPoint);
                                
                            }

                        }
                    }

                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }

            cells = family.familyInstances;
            cellsIds = family.cellElementsIds;
        }

        public List<XYZ> FindCollisions()
        //Search for collisions between created cells and model elements
        {
            List<XYZ> ICLocations = new List<XYZ>();
            
            Collision collision = new Collision(App, Uiapp, Doc, Uidoc);
           
            ExclusionFilter exclusionFilter = new ExclusionFilter(cellsIds);

            Color color = new Color(255, 0, 0);

            //find collisions between each cell and other model elements by filters
            foreach (FamilyInstance familyInstance in cells)
            {
                XYZ point = collision.FindCollision(familyInstance, exclusionFilter);
                if (point != null)
                {
                    OverwriteGraphic(familyInstance, color);
                    ICLocations.Add(point);
                }

            }

            return ICLocations;

        }

        void OverwriteGraphic(FamilyInstance instance, Color color)
        {
            OverrideGraphicSettings pGraphics = new OverrideGraphicSettings();
            pGraphics.SetProjectionLineColor(color);

            
            var patternCollector = new FilteredElementCollector(Doc);
            patternCollector.OfClass(typeof(FillPatternElement));
            FillPatternElement solidFillPattern = patternCollector.ToElements().Cast<FillPatternElement>().First(a => a.GetFillPattern().IsSolidFill);


            pGraphics.SetSurfaceForegroundPatternId(solidFillPattern.Id);
            pGraphics.SetSurfaceBackgroundPatternColor(color);
            
            using (Transaction transNew = new Transaction(Doc, "newTransaction"))
            {
                try
                {
                    transNew.Start();
                    Doc.ActiveView.SetElementOverrides(instance.Id, pGraphics);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }

        }

        public void OverwriteCell(int x, int y, Color color)
        {
            XYZ centerPoint = new XYZ(x * cellSizeF, y * cellSizeF, 0);

            BoundingBoxContainsPointFilter boundingBoxContainsPointFilter = new BoundingBoxContainsPointFilter(centerPoint);

            FilteredElementCollector cellsCollector = new FilteredElementCollector(Doc, cellsIds);
            IList<Element> cellsElements = cellsCollector.WherePasses(boundingBoxContainsPointFilter).ToElements();

            foreach (FamilyInstance familyInstance in cellsElements)
            {
                    OverwriteGraphic(familyInstance, color);
            }

        }

        void CreateCellBorders(XYZ centralPoint, double cellSizeF)
        {


            List<CellCorners> cellCorners = new List<CellCorners>();

            cellCorners = GetCellCorners(centralPoint, cellSizeF);

            foreach (CellCorners cC in cellCorners)
            {
                CreateCell(cC);
            }

        }

        void CreateCell(CellCorners cC)
        {
            CreateModelLine(cC.C1, cC.C2);
            CreateModelLine(cC.C2, cC.C3);
            CreateModelLine(cC.C3, cC.C4);
            CreateModelLine(cC.C4, cC.C1);
        }


        List<CellCorners> GetCellCorners(XYZ centerPoint, double cellSizeF)
        {
            List<CellCorners> cellCorners = new List<CellCorners>()
            {
                new CellCorners {},
            };

            cellCorners[0].C1 = new XYZ(centerPoint.X - cellSizeF / 2, centerPoint.Y - cellSizeF / 2, centerPoint.Z + cellSizeF / 2);
            cellCorners[0].C2 = new XYZ(centerPoint.X - cellSizeF / 2, centerPoint.Y + cellSizeF / 2, centerPoint.Z + cellSizeF / 2);
            cellCorners[0].C3 = new XYZ(centerPoint.X + cellSizeF / 2, centerPoint.Y + cellSizeF / 2, centerPoint.Z + cellSizeF / 2);
            cellCorners[0].C4 = new XYZ(centerPoint.X + cellSizeF / 2, centerPoint.Y - cellSizeF / 2, centerPoint.Z + cellSizeF / 2);

            cellCorners[0].C5 = new XYZ(centerPoint.X - cellSizeF / 2, centerPoint.Y - cellSizeF / 2, centerPoint.Z - cellSizeF / 2);
            cellCorners[0].C6 = new XYZ(centerPoint.X - cellSizeF / 2, centerPoint.Y + cellSizeF / 2, centerPoint.Z - cellSizeF / 2);
            cellCorners[0].C7 = new XYZ(centerPoint.X + cellSizeF / 2, centerPoint.Y + cellSizeF / 2, centerPoint.Z - cellSizeF / 2);
            cellCorners[0].C8 = new XYZ(centerPoint.X + cellSizeF / 2, centerPoint.Y - cellSizeF / 2, centerPoint.Z - cellSizeF / 2);



            return cellCorners;
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

    }
}
