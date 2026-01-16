# GERMA - Geothermal Energy Resource Mapping and Analysis

GERMA is a software tool developed as part of a master's thesis at the Technical University of Berlin under the supervision of GASAG Solution Plus within the Urban [Ground Heat project](https://www.ieg.fraunhofer.de/de/projekte/urbangroundheat.html) [[1](#references)]* (FKZ: 03EN3066).

<img width="600" alt="image" src="https://github.com/user-attachments/assets/d0323588-8cfc-452a-9582-eceee14d1aac" />
<br>

*Example view of the GERMA tool user interface*

## Public Data

GERMA enables automated geothermal potential analysis for borehole heat exchangers (BHE) for any area in Berlin. The tool utilizes publicly available geological and geothermal parameters provided through the geoportal [[2](#references)] of the City of Berlin - Senate Department for Urban Development, Building and Housing (SenStadt).

All relevant geodata is automatically transferred to an internal database at regular intervals to minimize latency during data transmission. The transferred geometries are indexed using an R-tree structure to reduce database query times. Technically, this is implemented using a PostgreSQL database in combination with the PostGIS extension for managing geodata.

## Determination of Usable Areas

To perform demand-based analysis in real-time, GERMA first determines the usable area of a selected region. Users can either select existing land parcels or interactively draw custom areas as polygons.

The usable area is defined as the area available for geothermal drilling for borehole heat exchangers, taking into account specified distances from property boundaries, buildings, and tree crowns. Subsequently, areas smaller than 15.88 m² are filtered out. This corresponds to the minimum area of an equilateral triangle with 6 m side length and represents a theoretical minimum probe arrangement with three BHEs. The remaining areas thus represent the zones where borehole heat exchanger fields can be realized.

Underground garages are taken into account, as their locations are recorded in the city of Berlin's ALKIS building data. However, whether drilling equipment can access inner courtyards through sufficiently wide driveways must be assessed by the user and is not automatically verified by the GERMA tool.

<img width="600"  alt="image" src="https://github.com/user-attachments/assets/f964cf6a-3735-4197-9a31-66f476eb96b5" />
<br>

*Calculation of usable areas - example with 2m distance to buildings and 3m distance to property boundaries. Graphic from [[3](#references)]*

## Modeling of Probe Fields

The determined usable areas are then used to model a theoretical borehole heat exchanger field (BHE field). For this purpose, an algorithm developed in the associated [master's thesis](https://tkroeger.com/websites/masterarbeit.pdf) for optimizing probe positions is applied to the respective areas [[3](#references)].

The algorithm iteratively generates a probe field in the form of a multipolygon structure with the most compact area coverage possible. This results in an orderly, technically feasible arrangement of borehole heat exchangers, on the basis of which the thermal extraction capacity can subsequently be determined.

<img width="400"  alt="image" src="https://github.com/user-attachments/assets/0a1c1744-fff7-4afb-b752-28b9b1a44991" />
<br>

*Step-by-step visualization of the area-minimizing point positioning algorithm for multipolygon structures. Graphic from [[3](#references)]*

<img src="https://github.com/user-attachments/assets/f946886a-6d98-4817-aa0b-a5dde60ad579" alt="BHE_plaement" width="600">
<br>

*Animated visualisation*

## Determination of Heat Extraction

To determine the specific heat extraction that can be provided by the modeled probe field, reference probe fields for various thermal conductivities were designed based on VDI 4640 Part 2 [[4](#references)] and using the Earth Energy Designer (EED) program [[5](#references)]. Based on these reference fields, approximation formulas were developed. These formulas calculate the specific extraction capacity per probe meter, taking into account the mutual thermal influence within a compact borehole heat exchanger field. For evaluating a probe field with complete regeneration, the calculated specific extraction value of a single probe (according to VDI 4640) is extrapolated to the entire probe field. The simulations can also consider thermally enhanced grouting material (thermal conductivity 2.0 W/(m·K)) and different flow conditions.

### Simulation Parameters

The following initial parameters were defined for simulating the geothermal potential of borehole heat exchanger fields:

| Parameter | Value |
|-----------|-------|
| Probe type | Da 32 Double-U, PE100 RC |
| Ground temperature | constant 11°C |
| Heat carrier | Monoethylene glycol (25%) |
| Flow behavior | laminar |
| Probe spacing | 6 m |
| Simulation period | 25 years |
| Design | 2400 full load hours |
| Minimum lower temperature boundary condition of heat carrier at probe inlet | -3°C |

Groundwater flow in the respective investigation areas was not considered. The assumed drilling depth corresponds to the legally permissible drilling depth at the respective location, which is determined by limiting clay layers such as the Holstein layer or the Rupel clay layer [[6](#references)].

The usable drilling depth was determined based on the "[3D Geological Model of Berlin – Germany](https://dataservices.gfz-potsdam.de/panmetaworks/showshort.php?id=c50925c8-241b-11eb-9603-497c92695674)" [[7](#references)] created by the Helmholtz Centre for Georesearch (GFZ). The respective thermal conductivities at the corresponding depths were taken from the Berlin Geoportal [[8](#references)]. For depths greater than 100 m, where no values are specified, a thermal conductivity of 2.4 W/(m·K) is assumed, as water-saturated unconsolidated rocks commonly occur at these depths, for which an average thermal conductivity of this magnitude is plausible [[9](#references)]. The thermal conductivities were proportionally calculated depending on their thickness.

<img width="600" alt="image" src="https://github.com/user-attachments/assets/9a47d73d-16c9-4f0f-89a5-0b5aa58a350a" />
<br>

*Specific extraction performance for different probe field sizes with various subsurface thermal conductivities. Data points based on VDI 4640 Part 2 [[4](#references)] and EED [[5](#references)] simulations*

## Determination of Demand Coverage

To compare the heat demand of buildings on the considered property with the geothermal heat that can be provided by the probe field, the individual coverage of heat demand is calculated for each modeled probe field.

For determining the expected annual heating work, data from "[Energymap](https://energymap-berlin.de/)" [[14](#references)] was used, which enables building-specific, AI-based estimation of annual heat demand. The Energymap data is integrated into GERMA via an API. By specifying a seasonal performance factor (SPF) of the heat pump, the proportion of heat demand that can be covered by the BHE field can then be calculated.

## Codebase

The complete source code of GERMA is publicly available on GitHub at "[TimKroeger13/GERMA: Geothermal Energy Resource Mapping and Analysis](https://github.com/TimKroeger13/GERMA)" and is published under the MIT License. The schema of the required database tables is also documented in the repository. Currently, a live version of the GERMA application runs on internal servers of GASAG Solution Plus and is therefore not publicly accessible.

<img  alt="image" src="https://github.com/user-attachments/assets/c7adb244-c059-4f80-be5e-8c334bdf462a" />
<br>

*Example of modeled BHE fields for multiple buildings*

## References

1. Fraunhofer IEG. UrbanGroundHeat – Wärmewende in urbanen Bestandsquartieren [Internet]. Available from: https://www.ieg.fraunhofer.de/de/projekte/urbangroundheat.html. Accessed: 2024-06-20.

2. Senatsverwaltung für Stadtentwicklung, Bauen und Wohnen. Geoportal Berlin [Internet]. Available from: https://gdi.berlin.de/viewer/main/. Accessed: 2025-11-20.

3. Kröger T. Web-Based Geothermal Energy Potential Mapping and Analysis for Berlin: Estimating the shallow geothermal energy potential of Berlin [Master's Thesis]. Berlin: Technical University of Berlin; 2023. Available from: https://tkroeger.com/websites/masterarbeit.pdf. Accessed: 2025-11-24.

4. Verein Deutscher Ingenieure. Thermische Nutzung des Untergrunds – Erdgekoppelte Wärmepumpenanlagen (VDI 4640). Berlin: Beuth Verlag; 2019.

5. Building Physics. EED – Earth Energy Designer, Version 4.24 [Software]. Available from: https://buildingphysics.com/eed-2/. Accessed: 2024-06-08.

6. Senatsverwaltung für Mobilität, Verkehr, Klimaschutz und Umwelt. Erdwärmenutzung in Berlin: Merkblatt für Erdwärmesonden und Erdwärmekollektoren mit einer Heizleistung bis 30 kW außerhalb von Wasserschutzgebieten. Berlin: Senatsverwaltung für Mobilität, Verkehr, Klimaschutz und Umwelt; 2025.

7. Frick M, Bott [Sippel] J, Scheck-Wenderoth M, Cacace M, Haacke N, Schneider M. 3D geological model of Berlin – Germany [dataset]. 2020. Available from: https://doi.org/10.5880/GFZ.4.5.2020.005

8. Senatsverwaltung für Stadtentwicklung, Bauen und Wohnen. Geoportal Berlin [Internet]. Available from: https://gdi.berlin.de/viewer/main/. Accessed: 2025-11-20.

9. Verein Deutscher Ingenieure. Thermische Nutzung des Untergrunds – Grundlagen, Genehmigungen, Umweltaspekte (VDI 4640). Berlin: Beuth Verlag; 2010.

## License

MIT License

---

# Notes for the Setup

### Scaffolding Request
<h2>Database scaffolding:</h2>

````
dotnet ef dbcontext scaffold "Host=localhost:5433;Database=germa;Username=germa;Password=germa" Npgsql.EntityFrameworkCore.PostgreSQL --no-onconfiguring --output-dir DataModel/Database --force --context DataContext --namespace GERMAG.DataModel.Database --startup-project Server --project Shared
````
<h2>Http Staus Codes:</h2>

### Error Codes

| Code         | Definition |
|-------------|-----|
| 429 | To many Requests |
| 512 | Requested location is outside the covered area|
| 513 | Not Enough space is given to place BHE in the given requested area |
| 514 | The selected locations are not connected by a landparcel |
| 515 | Could not connect to Server |
| 516 | To much requests for the short-report. Rate is limited |
| 517 | To much requests for the full-report. Rate is limited |
| 518 | To much requests for the User assignment. Rate is limited |

