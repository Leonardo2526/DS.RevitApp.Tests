﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.BinaryOperations;
using DS.RevitCmd.EnergyTest.SpaceBoundary;
using MoreLinq;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Filtering;
using OLMP.RevitAPI.Tools.Walls;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitCmd.EnergyTest.CompoundStructures
{
    internal class CompoundFaceStructureTest
    {
        private readonly UIDocument _uiDoc;
        private readonly IEnumerable<RevitLinkInstance> _allLoadedlinks;
        private readonly ISpecifyElementFilter _elementFilter;
        private readonly Document _doc;

        public CompoundFaceStructureTest(UIDocument uiDoc,
            ISpecifyElementFilter elementFilter)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _elementFilter = elementFilter;
        }

        public ILogger Logger { get; set; }

        public ITransactionFactory TransactionFactory { get; set; }

        public IEnumerable<CompoundFaceStructure> CreateFaceStructures(Wall wall, Face face)
        {
            var boundaryFace = new BoundaryFace(wall.Id, face);
            var f = GetInteractor();
            var result = new CompoundFaceStructureFactory(_doc, f).Create(boundaryFace);
            return result;
        }

        public IEnumerable<BoundaryFace> ComputeResultFaces(IEnumerable<CompoundFaceStructure> boundaryFaces)
        {
            foreach (var bf in boundaryFaces)
            {
                var f = bf.ComputeResultFace();
                yield return f;
            }
        }


        private Func<BoundaryFace, IEnumerable<BoundaryFace>> GetInteractor()
        {
            var wallInteraction = new WallInteraction(_doc, _allLoadedlinks, _elementFilter);


            IEnumerable<BoundaryFace> func(BoundaryFace sourceBoundaryFace)
            {
                var bWall = _doc.GetElement(sourceBoundaryFace.ElementId) as Wall;

                var fitElements = wallInteraction
               .Initialize(bWall)
               .FindSideFits();

                var fitResults = wallInteraction.FitResults;
                var fitResultsValues = fitResults.SelectMany(kv => kv.Value);
                //fitFaces.ForEach(ShowFace);

                //get only faces that have intersection with source face.
                var sourceFace = sourceBoundaryFace.Face;
                var intersectionFaces = new List<BoundaryFace>();
                foreach (var fitResultValue in fitResultsValues)
                {
                    var intersectionResults = sourceFace
                        .ExecuteBinaryOperationMany(fitResultValue.Item2, BinaryOperationType.Intersection);
                    var boundaryFaces = intersectionResults
                        .Select(f => new BoundaryFace(fitResultValue.Item1.Id, f));
                    intersectionFaces.AddRange(boundaryFaces);
                }

                return intersectionFaces;
            }

            return func;
        }

        public void PrintResults(IEnumerable<CompoundFaceStructure> faceStructures)
        {
            Logger?.Information($"Structures created: {faceStructures.Count()}");

            var faceStructuresList = faceStructures.ToList();
            for (int i = 0; i < faceStructuresList.Count; i++)
            {
                CompoundFaceStructure faceStructure = faceStructuresList[i];
                Logger?.Information($"Structure : {i + 1}");
                foreach (var boundaryFace in faceStructure)
                {
                    Logger?.Information($"face layer id: {boundaryFace.ElementId}");
                }
            }

        }

        public void ShowResults(IEnumerable<CompoundFaceStructure> faceStructures)
        {
           foreach (var faceStructure in faceStructures)
            {
                var faces = faceStructure.Select(s => s.Face);
                faces.ForEach(f => ShowFace(f));
            }
        }

        public void ShowResultFaces(IEnumerable<BoundaryFace> boundaryFaces)
            => boundaryFaces.ForEach(bf => ShowFace(bf.Face));

        private void ShowFace(Face face)
         => TransactionFactory.Create(() => face.ShowEdges(_doc), "ShowFace");
    }
}