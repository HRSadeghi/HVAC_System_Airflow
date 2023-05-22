using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HVAC_System_Airflow.Search
{
    [Transaction(TransactionMode.Manual)]
    public class Run : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDocument = commandData.Application.ActiveUIDocument;
            var document = uiDocument.Document;

            var elementId = uiDocument.Selection.PickObject(ObjectType.Element, "Please select an element to start with");
            if (elementId == null) return Result.Cancelled;


            var selectedElement = document.GetElement(elementId);

            try
            {
                var ats = Search.SearchForAirTerminals(document, selectedElement);
                var total_airflow = Search.CalculateTotalAirFlow(ats);
                TaskDialog.Show("TotalAirflow", $"No of AirTerminals: {ats.Count} \nTota Airflow: {total_airflow} L/S", TaskDialogCommonButtons.Ok);
            }
            catch 
            {
                TaskDialog.Show("Inappropriate element", "An inappropriate element has been selected. Please select an element of an HVAC system.", TaskDialogCommonButtons.Ok);
            }

            return Result.Succeeded;
        }


    }
}
