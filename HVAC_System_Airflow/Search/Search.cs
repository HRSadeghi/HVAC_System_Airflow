using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace HVAC_System_Airflow.Search
{
    static class Search
    {
        /// <summary>
        /// This list temporarily stores all element IDs that have been seen so far.
        /// </summary>
        private static List<int> elements_ids;
        /// <summary>
        /// This list temporarily stores all the air terminals that have been seen so far.
        /// </summary>
        private static List<Element> AirTerminals = new List<Element>();


        /// <summary>
        /// This method finds all air terminals that can be reached through an element.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="element">An element through which all air terminals of a UAV system are found.</param>
        /// <returns>
        /// <c>AirTerminals</c>: <c>List</c> --> <c>Element</c>
        /// </returns>
        /// <exception cref="Exception"></exception>
        public static List<Element> SearchForAirTerminals(Autodesk.Revit.DB.Document document, Element element)
        {
            AirTerminals = new List<Element>();
            elements_ids = new List<int>();


            var conectors = GetConnectors(element);
            if (conectors != null)
            {
                //TaskDialog.Show("Simple Command", $"Simple element: {elementId}", TaskDialogCommonButtons.Ok);
                foreach (var connector in conectors)
                {
                    _searchForAirTerminals(document, (Connector)connector);

                }
            }
            else
                throw new Exception();
            return AirTerminals;

        }


        /// <summary>
        /// This method finds the total airflow by taking all the air terminals of a UVAC system.
        /// </summary>
        /// <param name="AirTerminals"></param>
        /// <returns>
        /// <c>totalAirFlow</c>: <c>double</c>
        /// </returns>
        public static double CalculateTotalAirFlow(List<Element> AirTerminals)
        {
            double totalAirFlow = 0;
            foreach (var element in AirTerminals)
            {
                var airFlow = element.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.RBS_DUCT_FLOW_PARAM)?.AsDouble() ?? 0;
                totalAirFlow += airFlow;
            }

            totalAirFlow *= 28.31684; // convert to l/s
            totalAirFlow = Math.Round(totalAirFlow, 3);
            return totalAirFlow;
        }

        /// <summary>
        /// This method finds all air terminals that can be reached through one of the connectors of an element.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="connector">One of the connectors of an element</param>
        private static void _searchForAirTerminals(Autodesk.Revit.DB.Document document, Connector connector)
        {
            List<Connector> connected_connectors = new List<Connector>();

            try
            {
                var mepSystem = connector.MEPSystem;

                if (null != mepSystem)
                {
                    if (connector.IsConnected == true)
                    {
                        var connectorSet = connector.AllRefs;
                        var csi = connectorSet.ForwardIterator();
                        while (csi.MoveNext())
                        {
                            var connected = csi.Current as Connector;
                            if (null != connected)
                            {
                                // look for physical connections
                                if (connected.ConnectorType == ConnectorType.End ||
                                    connected.ConnectorType == ConnectorType.Curve ||
                                    connected.ConnectorType == ConnectorType.Physical)
                                {
                                    if (connector.Owner.Id != connected.Owner.Id)
                                    {
                                        if (!elements_ids.Contains(connected.Owner.Id.IntegerValue))
                                        {
                                            elements_ids.Add(connected.Owner.Id.IntegerValue);


                                            var el = document.GetElement(connected.Owner.Id);
                                            if (el.Category.Name == "Air Terminals")
                                            {
                                                AirTerminals.Add(el);
                                            }

                                            var new_conectors = GetConnectors(el);
                                            foreach (var c in new_conectors)
                                            {
                                                _searchForAirTerminals(document, (Connector)c);

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
            {
                try
                {
                    var HostElementId = GetPropValue(connector.Owner, "HostElementId") as ElementId;

                    var selectedElement = document.GetElement(HostElementId);
                    SearchForAirTerminals(document, selectedElement);
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(ex2.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        /// <summary>
        /// This method returns all the connectors of an element in the form of a set of connectors.
        /// </summary>
        /// <param name="element"></param>
        /// <returns><c>connectors</c>: <c>ConnectorSet</c></returns>
        private static ConnectorSet GetConnectors(Element element)
        {
            ConnectorSet connectors = null;

            if (element is FamilyInstance)
            {
                MEPModel m = ((FamilyInstance)element).MEPModel;

                if (null != m
                  && null != m.ConnectorManager)
                {
                    connectors = m.ConnectorManager.Connectors;
                }
            }
            else if (element is Wire)
            {
                connectors = ((Wire)element)
                  .ConnectorManager.Connectors;
            }
            else if (element is MEPCurve)
            {
                connectors = ((MEPCurve)element)
                      .ConnectorManager.Connectors;
            }
            return connectors;
        }

        /// <summary>
        /// This method checks whether an object has a certain property or not.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns>
        /// <c>result</c>: <c>bool</c>
        /// </returns>
        private static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }

        /// <summary>
        /// This method receives an object and the name of a property related to it as a string and returns the asked property.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns>
        /// <c>result</c>: <c>object</c>
        /// </returns>
        private static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }
    }
}
