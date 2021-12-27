﻿using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{


    class ModelSolid
    {
        readonly Document Doc;

        public ModelSolid(Document doc)
        {
            Doc = doc;
        }

        ElementUtils ElemUtils = new ElementUtils();

        public static Dictionary<Element, List<Solid>> SolidsInModel { get; set; }

        public Dictionary<Element, List<Solid>> GetSolids()
        {
            SolidsInModel = new Dictionary<Element, List<Solid>>();
            FilteredElementCollector collector = new FilteredElementCollector(Doc);

            ICollection<BuiltInCategory> elementCategoryFilters = new List<BuiltInCategory>
                {
                    BuiltInCategory.OST_DuctCurves,
                    BuiltInCategory.OST_PipeCurves,
                    BuiltInCategory.OST_Walls
                };

            ElementMulticategoryFilter elementMulticategoryFilter = new ElementMulticategoryFilter(elementCategoryFilters);

            Outline myOutLn = new Outline(PointsInfo.MinBoundPoint, PointsInfo.MaxBoundPoint);
            BoundingBoxIntersectsFilter boundingBoxFilter = new BoundingBoxIntersectsFilter(myOutLn);


            ICollection<ElementId> elementIds = new List<ElementId>
            {
                Main.CurrentElement.Id

            };
            ExclusionFilter exclusionFilter = new ExclusionFilter(elementIds);


            collector.WhereElementIsNotElementType();
            collector.WherePasses(boundingBoxFilter);
            collector.WherePasses(exclusionFilter);
            IList<Element> intersectedElementsBox = collector.WherePasses(elementMulticategoryFilter).ToElements();

            Dictionary<Element, List<Solid>> solidsDictionary = new Dictionary<Element, List<Solid>>();

            List<Solid> solids = new List<Solid>();
            foreach (Element elem in intersectedElementsBox)
            {
                solids = ElemUtils.GetSolids(elem);
                if (solids.Count !=0)
                {
                    SolidsInModel.Add(elem, solids);
                    solidsDictionary.Add(elem, solids);
                }             
            }

            return solidsDictionary;
        }


    }
}