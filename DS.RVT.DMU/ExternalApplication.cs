using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RVT.DMU
{
    public class ExternalApplication : Autodesk.Revit.UI.IExternalApplication
    {
        public Result OnStartup(Autodesk.Revit.UI.UIControlledApplication application)
        {
            TaskDialog.Show("Revit", "DMU mode activated!");

            try
            {
                // Register event. 
                application.ControlledApplication.DocumentChanged += new EventHandler<
    Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(application_DocumentChanged);
            }
            catch (Exception)
            {
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void application_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            Document Doc = e.GetDocument();

            //Instantiate a new class instance for element iteration and filtering
            ElementClassFilter filter = new ElementClassFilter(typeof(Pipe));

            ICollection<ElementId> colElID = e.GetModifiedElementIds(filter);

            string IDS = "";
            foreach (ElementId elID in colElID)
            {
                IDS += "\n" + elID.ToString();
            }

            TaskDialog.Show("Revit", IDS);
            
        }

        public Result OnShutdown(Autodesk.Revit.UI.UIControlledApplication application)
        {
            // remove the event.
            application.ControlledApplication.DocumentChanged -= application_DocumentChanged;
            return Result.Succeeded;
        }
    }




}
