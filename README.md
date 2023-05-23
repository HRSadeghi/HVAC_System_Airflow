# HVAC System Airflow

Revit is a building information modelling (BIM) software developed by Autodesk, used by architects, engineers, and construction professionals. It allows users to design and document building structures in a 3D model-based environment. 

HVAC (Heating, Ventilation, and Air Conditioning) systems regulate building temperature and air quality. In Revit, HVAC systems are modelled using various elements such as ducts, air terminals, and mechanical equipment.

In this project, we have provided a plug-in for Revit that can calculate the total airflow in an HVAC system.

## Finding all air terminals
 To find the total airflow, it is enough to find all the air terminals in an HVAC and sum their Flow values.

To do this, the <code class='language-cs'>[Search](https://github.com/HRSadeghi/HVAC_System_Airflow/blob/master/HVAC_System_Airflow/Search/Search.cs)</code> class is implemented, which allows us to find all the air terminals by getting an element from an HVAC system. In this class, the [```SearchForAirTerminals```](https://github.com/HRSadeghi/HVAC_System_Airflow/blob/abe3462466eea3232c2caa7d4a5c2acea7d72a95/HVAC_System_Airflow/Search/Search.cs#LL44C37-L44C58) method receives an ```element``` from a system along with the ```document``` and returns a list of air terminals. This method is given below:

```cs
public static List<Element> SearchForAirTerminals(Autodesk.Revit.DB.Document document, Element element)
{
    AirTerminals = new List<Element>();
    elements_ids = new List<int>();


    var conectors = GetConnectors(element);
    if (conectors != null)
    {
        foreach (var connector in conectors)
        {
            _searchForAirTerminals(document, (Connector)connector);

        }
    }
    else
        throw new Exception();
    return AirTerminals;

}
```

The [```SearchForAirTerminals```](https://github.com/HRSadeghi/HVAC_System_Airflow/blob/abe3462466eea3232c2caa7d4a5c2acea7d72a95/HVAC_System_Airflow/Search/Search.cs#LL44C37-L44C58) method first finds all the connectors of an element. Then, each connector is given individually to the [```_searchForAirTerminals```](https://github.com/HRSadeghi/HVAC_System_Airflow/blob/abe3462466eea3232c2caa7d4a5c2acea7d72a95/HVAC_System_Airflow/Search/Search.cs#LL92C12-L92C12) method to find all the air terminals that can be reached through this connector. This method works recursively and is as follows:

```cs
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
    .
    .
    .


}
```


## Calculating total airflow
To calculate the total airflow, it is enough to find the Flow parameter from all the air terminals and then sum them up. The  [```CalculateTotalAirFlow```](https://github.com/HRSadeghi/HVAC_System_Airflow/blob/abe3462466eea3232c2caa7d4a5c2acea7d72a95/HVAC_System_Airflow/Search/Search.cs#L73) method finds the total airflow by receiving a list of air terminals and then finding their Flow parameter. The unit of airflow obtained is not L/S, which by multiplying by 28.31684, is converted to this unit. This method is given below.




```cs
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
```