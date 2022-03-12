using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace DS.RVT.DMU
{
    public class WallUpdater : IUpdater
    {
        static AddInId m_appId;
        static UpdaterId m_updaterId;


        // constructor takes the AddInId for the add-in associated with this updater
        public WallUpdater(AddInId id)
        {
            m_appId = id;
            m_updaterId = new UpdaterId(m_appId, new Guid("aa8e4ffb-8241-4bce-9470-94df224829c5"));
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();

            foreach (ElementId addedElemId in data.GetAddedElementIds())
            {
                Element el = doc.GetElement(addedElemId);
                GetElementParameterInformation(el);
            }
        }

        public string GetAdditionalInformation()
        {
            return "updater example";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FloorsRoofsStructuralWalls;
        }

        public UpdaterId GetUpdaterId()
        {
            return m_updaterId;
        }

        public string GetUpdaterName()
        {
            return "updater example";
        }


        void GetElementParameterInformation(Element element)
        {
            // iterate element's parameters
            foreach (Parameter param in element.Parameters)
            {
                if (param.Definition.Name == "Комментарии")
                {
                    param.Set(element.Id.ToString());
                }
            }

        }

    }
}
