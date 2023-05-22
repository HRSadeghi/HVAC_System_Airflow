// Copyright 2023 Hamidreza Sadeghi. All rights reserved.

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
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

            // Get selected element
            var selectedElement = document.GetElement(elementId);

            try
            {
                // Find all air terminals of a UVAC system 
                var ats = Search.SearchForAirTerminals(document, selectedElement);
                // Calculate total airflow of all terminals
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
