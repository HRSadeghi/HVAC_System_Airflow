# HVAC System Airflow

Revit is a building information modelling (BIM) software developed by Autodesk, used by architects, engineers, and construction professionals. It allows users to design and document building structures in a 3D model-based environment. 

HVAC (Heating, Ventilation, and Air Conditioning) systems regulate building temperature and air quality. In Revit, HVAC systems are modelled using various elements such as ducts, air terminals, and mechanical equipment.

In this project, we have provided a plug-in for Revit that can calculate the total airflow in an HVAC system.

## Finding all air terminals
 To find the total airflow, it is enough to find all the air terminals in an HVAC and sum their Flow values.

To do this, the <code class='language-cs'>[Search](https://github.com/HRSadeghi/HVAC_System_Airflow/blob/master/HVAC_System_Airflow/Search/Search.cs)</code> class is implemented, which allows us to find all the air terminals by getting an element from an HVAC system.

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