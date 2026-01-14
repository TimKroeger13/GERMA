//GlobalURLPreLink
const GlobalURLPreLink = "https://tkroeger.com:8443/api/report/";
const GlobalURLVectorDataSteam = "https://tkroeger.com:8443/api/Geodatastream/";
const GlobalURLConnection = "https://tkroeger.com:8443/api/Connection/";
//const GlobalURLPreLink = "https://localhost:9999/api/report/";
//const GlobalURLVectorDataSteam = "https://localhost:9999/api/Geodatastream/";
//const GlobalURLConnection = "https://localhost:9999/api/Connection/";

//const GlobalURLPreLink = "https://tkroeger.com:8443/api/report/";
//const GlobalURLVectorDataSteam = "https://tkroeger.com:8443/api/Geodatastream/";
//const GlobalURLConnection = "https://tkroeger.com:8443/api/Connection/";

async function GetRequest(XcorList, YcorList) {
    var Srid = 4326; //test

    const params = new URLSearchParams();
    XcorList.forEach(x => params.append('xCor', x));
    YcorList.forEach(y => params.append('yCor', y));
    params.append('srid', Srid);
    params.append('userId', window.currentUserId);


    const url = GlobalURLPreLink + "reportdata?" + params.toString();

    try {
        const response = await fetch(url);

        if (!response.ok) {
            console.error('Server error:', response.status);

            try {
                const errorData = await response.json();
                console.error('Error details:', errorData);
                alert(`Server error: ${response.status} - ${errorData.message}`);
            } catch (parseError) {
                console.error('Error parsing JSON:', parseError);
                alert(`Server error: ${response.status} - Something went terribly, terribly wrong`);
            }

            return null;
        }

        const GetJsonString = await response.json();
        return GetJsonString;

    } catch (error) {
        console.error('Error:', error.message);
        alert('Network error or failed request. Please try again.');
        return null;
    }
}

async function GetRequestFullReport(reportType) {

    document.getElementById("btnMinusArea").disabled = false;
    document.getElementById("btnAddSuperiorArea").disabled = false;
    document.getElementById('exportDropdown').classList.remove('disabled');

    var Srid = 4326;

    let CustomProbeDistancevalue = parseFloat(document.getElementById("numInput").value);
    let CustomLandParcelDistancevalue = parseFloat(document.getElementById("FlurInput").value);
    let CustomTreeBuffer = parseFloat(document.getElementById("TreeInput").value);
    let CustomEwsNumber = parseFloat(document.getElementById("EwsNumberInput").value);
    let disableBuildings = document.getElementById('toggleBuilding').checked;
    let DisableTrees = document.getElementById('toggleTrees').checked;
    let EnableMaixmalEWSFieldSize = document.getElementById('toggleEWSFieldSize').checked;
    let MaxEWSDrillingDepthNumber = parseFloat(document.getElementById("MaxEWSDrillingDepthInput").value);
    let COP = parseFloat(document.getElementById("COPInput").value);

    //DisableTrees toggleTrees

    if (CustomProbeDistancevalue < 6) CustomProbeDistancevalue = 6;
    document.getElementById("numInput").value = CustomProbeDistancevalue;

    const params = new URLSearchParams();
    Current_lng_coordiante.forEach(x => params.append('xCor', x));
    Current_lat_coordiante.forEach(y => params.append('yCor', y));
    params.append('srid', Srid);
    params.append('ProbeDistance', CustomProbeDistancevalue);
    params.append('UseBuildings', !disableBuildings);
    params.append('LandParcelDistance', CustomLandParcelDistancevalue);
    params.append('UseTrees', !DisableTrees);
    params.append('CalculateMaximalFieldSize', EnableMaixmalEWSFieldSize);
    params.append('TreeBuffer', CustomTreeBuffer);
    params.append('MinimalBhePerInduvidualArea', CustomEwsNumber);
    params.append('MaximalDrillingDepth', MaxEWSDrillingDepthNumber);
    params.append('userId', window.currentUserId);
    params.append('Cop', COP);

    const UserDemandInput = document.getElementById('waermebedarfInput');

    if (UserDemandInput) {
        const userInputValue = UserDemandInput.value;
        params.append('EnergyDemand', Math.round(userInputValue * 1000));
    }


    const UserRegenerationInput = document.getElementById('RegenerationSelect');

    var CustomRegenerationInput = "None";
    if (UserRegenerationInput) {
        CustomRegenerationInput = UserRegenerationInput.value;
        params.append('Regeneration', CustomRegenerationInput)
    }

    const UserFlowType = document.getElementById('FlowTypeSelect')

    var CustomFlowTypeInput = "false";
    if (UserFlowType) {
        CustomFlowTypeInput = UserFlowType.value;
        params.append('FlowIsTurbolent', CustomFlowTypeInput)
    }

    //TreeBuffer

    if (reportType == 'probe') {
        url = GlobalURLPreLink + "fullreport?" + params.toString() + "&probeRes=true";

    } else {

        url = GlobalURLPreLink + "fullreport?" + params.toString() + "&probeRes=false";

    }

    try {
        const response = await fetch(url);

        if (!response.ok) {
            console.error('Server error:', response.status);

            try {
                const errorData = await response.json();
                console.error('Error details:', errorData);
                alert(`Server error: ${response.status} - ${errorData.message}`);
            } catch (parseError) {
                console.error('Error parsing JSON:', parseError);
                alert(`Server error: ${response.status} - Something went terribly, terribly wrong`);
            }

            return null;
        }

        const GetJsonString = await response.json();
        return GetJsonString;

    } catch (error) {
        console.error('Error:', error.message);
        alert('Network error or failed request. Please try again.');
        return null;
    }
}

async function GetRequestEditGeometry(NewAreaGeometry,MinusAreaGeometryList,AllSuperiorGeometryLists) {

    document.getElementById("btnMinusArea").disabled = false;
    document.getElementById("btnAddSuperiorArea").disabled = false;
    document.getElementById('exportDropdown').classList.remove('disabled');

    var InputGeometry = null;
    var SubtractionArea = null;
    var SuperiorArea = null;

    if (NewAreaGeometry == null && MinusAreaGeometryList.length == 0) { return; }

    if(NewAreaGeometry != null){
        InputGeometry = NewAreaGeometry;
    }

    //Added new Area case
    if(NewAreaGeometry.length > 0 && MinusAreaGeometryList.length == 0) {
        InputGeometry = NewAreaGeometry;
    }

    //Minus Area given case (Creates featureCollection)
    if(MinusAreaGeometryList.length > 0) {
        SubtractionArea = turf.featureCollection(MinusAreaGeometryList.map(item => turf.feature(item.geometry)));
    }

    if(AllSuperiorGeometryLists.length > 0) {
        SuperiorArea = turf.featureCollection(AllSuperiorGeometryLists.map(item => turf.feature(item.geometry)));
    }

    let CustomProbeDistancevalue = parseFloat(document.getElementById("numInput").value);
    let CustomLandParcelDistancevalue = parseFloat(document.getElementById("FlurInput").value);
    let disableBuildings = document.getElementById('toggleBuilding').checked;
    let DisableTrees = document.getElementById('toggleTrees').checked;
    let CustomTreeBuffer = parseFloat(document.getElementById("TreeInput").value);
    let CustomEwsNumber = parseFloat(document.getElementById("EwsNumberInput").value);
    let EnableMaixmalEWSFieldSize = document.getElementById('toggleEWSFieldSize').checked;
    let MaxEWSDrillingDepthNumber = parseFloat(document.getElementById("MaxEWSDrillingDepthInput").value);
    let COP = parseFloat(document.getElementById("COPInput").value);

    if (CustomProbeDistancevalue < 6) CustomProbeDistancevalue = 6;
    document.getElementById("numInput").value = CustomProbeDistancevalue;

    const UserDemandInput = document.getElementById('waermebedarfInput');

    var CustomEnergyDemand = 0;
    if (UserDemandInput) {
        const userInputValue = UserDemandInput.value;
        CustomEnergyDemand = Math.round(userInputValue * 1000);
    }

    const UserRegenerationInput = document.getElementById('RegenerationSelect');

    var CustomRegenerationInput = "None";
    if (UserRegenerationInput) {
        CustomRegenerationInput = UserRegenerationInput.value;
    }

    const UserFlowType = document.getElementById('FlowTypeSelect')

    var CustomFlowTypeInput = false;
    if (UserFlowType) {
        CustomFlowTypeInput = UserFlowType.value === 'true';
    }

    const payload = {
        srid: 4326,
        geojson: JSON.stringify(InputGeometry),
        geojsonMinus: JSON.stringify(SubtractionArea),
        GeojsonSuperior: JSON.stringify(SuperiorArea),
        ProbeDistance: CustomProbeDistancevalue,
        UseBuildings: !disableBuildings,
        LandParcelDistance: CustomLandParcelDistancevalue,
        UseTrees: !DisableTrees,
        CalculateMaximalFieldSize: EnableMaixmalEWSFieldSize,
        TreeBuffer: CustomTreeBuffer,
        MinimalBhePerInduvidualArea: CustomEwsNumber,
        EnergyDemand: CustomEnergyDemand,
        Regeneration: CustomRegenerationInput,
        FlowIsTurbolent: CustomFlowTypeInput,
        MaximalDrillingDepth: MaxEWSDrillingDepthNumber,
        userId: window.currentUserId,
        Cop: COP
    };

    const url = GlobalURLPreLink + "geojsonreport";

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload) // Send the payload with SRID and GeoJSON
        });

        if (!response.ok) {
            console.error('Server error:', response.status);

            try {
                const errorData = await response.json();
                console.error('Error details:', errorData);
                alert(`Server error: ${response.status} - ${errorData.message}`);
            } catch (parseError) {
                console.error('Error parsing JSON:', parseError);
                alert(`Server error: ${response.status} - Something went terribly, terribly wrong`);
            }

            return null;
        }

        const GetJsonString = await response.json();
        return GetJsonString;

    } catch (error) {
        console.error('Error:', error.message);
        alert('Network error or failed request. Please try again.');
        return null;
    }
}

async function fetchAndRenderVectorData() {
    const bounds = map.getBounds();
    const zoom = map.getZoom(); //zoom >=16  //17

    if (!map.hasLayer(treeLayerGroup)) return;

    if (zoom < 17) {
        await RemoveTreeLayerFromMap();
        return;
    }

    const bbox = {
        minLat: bounds.getSouth(),
        minLng: bounds.getWest(),
        maxLat: bounds.getNorth(),
        maxLng: bounds.getEast()
    };

    const payload = {
        srid: 4326,
        minLat: bbox.minLat,
        minLng: bbox.minLng,
        maxLat: bbox.maxLat,
        maxLng: bbox.maxLng,
    };

    const url = GlobalURLVectorDataSteam + "Getvectordatasteam";

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload) // Send the payload with SRID and GeoJSON
        });

        if (!response.ok) {
            console.error('Server error:', response.status);

            try {
                const errorData = await response.json();
                console.error('Error details:', errorData);
                alert(`Server error: ${response.status} - ${errorData.message}`);
            } catch (parseError) {
                console.error('Error parsing JSON:', parseError);
                alert(`Server error: ${response.status} - Something went terribly, terribly wrong`);
            }

            return null;
        }

        const GetJsonString = await response.json();

        //Treevectors

        if (map.hasLayer(treeLayerGroup)) {

            var TransformedGetJsonStringTree = await BackTransformationOfGeometry(GetJsonString.treedata.geojsonTree);

            // Manually wrap the geometry into a Feature
            const TreeVectorFeature = {
                type: "Feature",
                geometry: TransformedGetJsonStringTree, // already a MultiPolygon
                properties: {
                    description: "TreeVector"
                }
            };

            await addTreeLayerToMap(TreeVectorFeature);

        }

    } catch (error) {
        console.error('Error:', error.message);
        alert('Network error or failed request. Please try again.');
        return null;
    }
}


async function InitalConnection(Userid) {
    const url = GlobalURLConnection + "connectionstart?userid=" + Userid;

    try {
        const response = await fetch(url);

        if (!response.ok) {
            console.error('Server error:', response.status);
            alert(`Server error: ${response.status}`);
            return false;
        }
        return true;

    } catch (error) {
        console.error('Error:', error.message);
        alert('Network error or failed request. Please try again.');
        return false;
    }
}

function EndConnection(userid) {
    const url = GlobalURLConnection + "connectionend?userid=" + userid;

    // Use sendBeacon instead of fetch
    const success = navigator.sendBeacon(url);

    if (!success) {
    } else {
    }

    return success;
}