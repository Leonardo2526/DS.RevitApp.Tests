﻿using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Geometry.Faces;
using Rhino.DocObjects;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitCmd.EnergyTest.SpaceBoundary
{
    public class CompoundFaceStructure(BoundaryFace sourceBoundaryFace) : List<BoundaryFace>
    {
        private readonly BoundaryFace _sourceBoundaryFace = sourceBoundaryFace;
        private BoundaryFace _minBoundaryFace;

        public double Width => _minBoundaryFace is null ? 
            0 :
            _minBoundaryFace.Face.DistanceAtCenter(_sourceBoundaryFace.Face);

        public BoundaryFace MinBoundaryFace  => 
            _minBoundaryFace = this.LastOrDefault();

        public XYZ ComputeCenter()
        {
            var minFaceCenter = MinBoundaryFace.Face.ComputeCenter();
            var sourceFaceCenterProj = _sourceBoundaryFace.Face.Project(minFaceCenter);
            var offsetVector = sourceFaceCenterProj.XYZPoint - minFaceCenter;
            return minFaceCenter + offsetVector.Multiply(0.5);
        }


        public Face ComputeResultFace()
        {
            var minFaceBasis = MinBoundaryFace.Face.GetCenterBasis();
            var minFaceCenter = minFaceBasis.Origin.ToXYZ();
            var structureCenter = ComputeCenter();
            var offsetVector = structureCenter - minFaceCenter;
            var compareValue = minFaceBasis.Z
                .IsParallelTo(offsetVector.Normalize().ToVector3d(), 1.DegToRad());
            var movedFace = FaceUtils.Offset(_minBoundaryFace.Face, offsetVector.GetLength(), true);

            return movedFace;
        }

        public IEnumerable<CompoundStructure> ToCompoundStructures(Document doc)
        {
            foreach (var bf in this)
            {
                var elem = doc.GetElement(bf.ElementId);
                var wall = elem as Wall;
                yield return wall.WallType.GetCompoundStructure();
            }
        }
    }
}
