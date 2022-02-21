using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation;
using DS.PathSearch.GridMap;
using FrancoGustavo;
using System.Collections.Generic;
using Location = DS.PathSearch.Location;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PathFinder
    {
        public List<XYZ> PathCoords { get; set; }

        public List<PathFinderNode> AStarPath(XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints)
        {
            InputData data = new InputData(startPoint, endPoint, unpassablePoints);
            data.ConvertToPlane();

            MapCreator map = new MapCreator();
            map.Start = new Location(InputData.Ax, InputData.Ay, InputData.Az);
            map.Goal = new Location(InputData.Bx, InputData.By, InputData.Bz);

            map.Matrix = new int[InputData.Xcount, InputData.Ycount, InputData.Zcount];

            foreach (StepPoint unpass in InputData.UnpassStepPoints)
                map.Matrix[unpass.X, unpass.Y, unpass.Z] = 1;

            List<PathFinderNode> pathNodes = FGAlgorythm.GetPathByMap(map, new PathRequiment());

            return pathNodes;
        }
    }
}
