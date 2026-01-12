proj4.defs("EPSG:4326", "+proj=longlat +datum=WGS84 +no_defs");
proj4.defs("EPSG:25833", "+proj=utm +zone=33 +ellps=GRS80 +units=m +no_defs");

var Current_lat_coordiante = null;
var Current_lng_coordiante = null;

var Multiple_lat = [];
var Multiple_lng = [];

var EditGeometry = null;

var AllGeometryLists = [];
var EditGeometryListTemp = [];
var EditGeometryTemp = null;

var Minus_AllGeometryLists = [];
var Minus_EditGeometryListTemp = [];
var Minus_EditGeometryTemp = null;

var AllSuperiorGeometryLists = [];
var EditSuperiorGeometryListTemp = [];
var EditSuperiorGeometryTemp = null;

var T_ProbePointsGeometry;
var T_UsableGeometry;
var T_ResrictionGeometry;

var LastShownGeometry = null;
var LastShownCustomGeometry = null;
var LastReportData = null;

var LastReportGeometry_UsabeGeometry = null;
var LastReportGeometry_ResrictionGeometry = null;
var LastReportGeometry_ProbePointsGeometry = null;

async function onMapClick(e, callback) {

    await clearAllForms();

    document.getElementById('exportDropdown').classList.add('disabled');

    LastReportData = null;

    var clickCoordinates = e.latlng;

    Multiple_lng = [clickCoordinates.lng];
    Multiple_lat = [clickCoordinates.lat];

    await InitalPointQuery(clickCoordinates.lng, clickCoordinates.lat)

}

async function onMapClickWithCtrl(e) {
    
    document.getElementById('exportDropdown').classList.add('disabled');

    LastReportData = null;

    var clickCoordinates = e.latlng;

    Multiple_lng.push(clickCoordinates.lng);
    Multiple_lat.push(clickCoordinates.lat);

    await InitalPointQuery(Multiple_lng, Multiple_lat)
}

async function onMapClickAddArea(e) {

    LastReportData = null;

    var clickCoordinates = e.latlng;

    await CreateEditArea(clickCoordinates.lng, clickCoordinates.lat)
}

async function onMapClickWithSuperiorArea(e) {

    LastReportData = null;

    var clickCoordinates = e.latlng;

    await CreateEditSuperiorArea(clickCoordinates.lng, clickCoordinates.lat)
}

async function onMapClickAddMinusArea(e) {

    LastReportData = null;

    var clickCoordinates = e.latlng;

    await SubtractEditArea(clickCoordinates.lng, clickCoordinates.lat)
}

async function onRightClick(e) {

    $("#myModal").modal({ backdrop: false });
    $('.modal').modal('hide');

    clearSelected();

}

async function ShowDetailedReport(reportType, InputGeometry) {

    if(Minus_EditGeometryTemp != null || Minus_AllGeometryLists.length>0 || EditSuperiorGeometryTemp != null ){
        await dissolveArea();
        reportType = 'custom';
        InputGeometry = LastShownGeometry;
    }

    //if(InputGeometry==null){InputGeometry = LastShownGeometry;} //Last click area

    if (AllGeometryLists.length != 0 || EditGeometryListTemp.length != 0) {
        await dissolveArea()
        reportType = 'custom';
        InputGeometry = AllGeometryLists;
    }

    if(InputGeometry==null){InputGeometry = LastShownCustomGeometry;} //Last custom selected Area

    if (InputGeometry != null) {AllGeometryLists = InputGeometry; } //switsh to choosen geometry when given

    if(Current_lng_coordiante==null && Current_lat_coordiante==null && LastShownCustomGeometry == null){
        return;
    }

    changeCursor('progress');

    if (reportType == 'custom') {
        var ReportRequest_Json = await GetRequestEditGeometry(AllGeometryLists,Minus_AllGeometryLists,AllSuperiorGeometryLists);
    }
    else {
        var ReportRequest_Json = await GetRequestFullReport(reportType);
    }

    if (ReportRequest_Json[0].error != null) {
        alert(ReportRequest_Json[0].error)
        return true
    }

    LastReportData = ReportRequest_Json[0];

    const geometry_Usable_json = JSON.parse(ReportRequest_Json[0].geometry_Usable);
    const geometry_Resriction_json = JSON.parse(ReportRequest_Json[0].geometry_Restiction);

    if (geometry_Usable_json.coordinates.length === 0) {
        UsabeGeometry = null
    } else {
        var UsabeGeometry = await BackTransformationOfGeometry(ReportRequest_Json[0].geometry_Usable);
    }

    if (geometry_Resriction_json.coordinates.length === 0) {
        ResrictionGeometry = null
    } else {
        var ResrictionGeometry = await BackTransformationOfGeometry(ReportRequest_Json[0].geometry_Restiction);
    }

    if (ReportRequest_Json[0].probePoint.length === 0) {
        ProbePointsGeometry = null
    } else {
        var ProbePointsGeometry = await BackTransformationOfProbepoints(ReportRequest_Json[0].probePoint);
    }

    const flattenedUsableCoordinates = [UsabeGeometry.coordinates];
    T_UsableGeometry = turf.multiPolygon(flattenedUsableCoordinates, { description: "UsableArea" });

    const wrappedRestrictionCoordinates = [ResrictionGeometry.coordinates];
    T_ResrictionGeometry = turf.multiPolygon(wrappedRestrictionCoordinates, { description: "RestrictionZone" });

    var totalRating = [];
    if (ReportRequest_Json[0].probePoint.length != 0) {
        for (let k = 0; k < ProbePointsGeometry.length; k++) {
            var Singlerating = ProbePointsGeometry[k].properties.rating;
            totalRating.push(Singlerating);
        }
    }
    ReportRequest_Json[0].totalRating = totalRating

    //Create Gethermalreport
    if (reportType == 'probe') {
        var GeothermalReport = await CreateReportHTML(ReportRequest_Json[0], true, true);
    } else {
        var GeothermalReport = await CreateReportHTML(ReportRequest_Json[0], true, false);
    }

    //Set Inital HTMl Repot Parameter

    await SetReport(GeothermalReport);

    //opens modal window

    if (reportType == 'probe') {
        await openModal(GeothermalReport, true, ReportRequest_Json[0]);
        renderBoxplot(ReportRequest_Json[0].totalRating);
    } else {
        await openModal(GeothermalReport, false);
    }

    await removeLandParcels()
    await CreateLandParcel(UsabeGeometry, '#00ff00', '#00ff00', 2, 0, 0.2);  //2,0,0.2
    await CreateLandParcel(ResrictionGeometry, '#ff6600', '#ff6600', 2, 1, 0.2);

    //Change Modal intput to Serevr Response

    document.getElementById('RegenerationSelect').value = ReportRequest_Json[0].regeneration;
    document.getElementById('FlowTypeSelect').value = ReportRequest_Json[0].flowIsTurbolent;

    //Add Points to map

    const turfPoints = ProbePointsGeometry.map(p => {
        const coords = p.coordinates[0]; // [lng, lat]
        return turf.point(coords, p.properties); // Adds properties to each point
    });

    T_ProbePointsGeometry = turf.featureCollection(turfPoints);

    await CreateAllPoints(T_ProbePointsGeometry);

    //Saving Last Report Geometry
    LastReportGeometry_UsabeGeometry = UsabeGeometry;
    LastReportGeometry_ResrictionGeometry = ResrictionGeometry;
    LastReportGeometry_ProbePointsGeometry = T_ProbePointsGeometry;


    changeCursor('default');

    return true;
}

//multiple buttons

async function InitalPointQuery(lng, lat) {

    AllGeometryLists = [];

    if (!Array.isArray(lng) && !Array.isArray(lng)) {

        lng = [lng]
        lat = [lat]

    }

    Current_lat_coordiante = lat;
    Current_lng_coordiante = lng;

    //Get Json from Server
    var ReportRequest_Json = await GetRequest(lng, lat);

    if (ReportRequest_Json[0].error != null) {
        Multiple_lat.pop();
        Multiple_lng.pop();
        alert(ReportRequest_Json[0].error);
        return true
    }

    //Transform Geometry Back
    var LandParcelGeometry = await BackTransformationOfGeometry(ReportRequest_Json[0].geometry);
    LastShownGeometry = turf.multiPolygon(LandParcelGeometry.coordinates);

    //Create Gethermalreport
    var GeothermalReport = await CreateReportHTML(ReportRequest_Json[0], false, false);
    await SetReport(GeothermalReport);

    //opens modal window
    await openModal(GeothermalReport, false);

    //Plots geometry on map
    await removeLandParcels();
    await CreateLandParcel(LandParcelGeometry, '#3388ff', '#3388ff', 2, 1, 0.2);

    return true

}

async function getGeojsonFromAddress(address) {

    var geoJsonRequest = await httpGet("https://nominatim.openstreetmap.org/search?q=" + address + "&format=geojson");

    var parsedGeoJson = JSON.parse(geoJsonRequest);

    if (parsedGeoJson.features.length == 0) {
        return;
    }

    x = parsedGeoJson.features[0].geometry.coordinates[1];
    y = parsedGeoJson.features[0].geometry.coordinates[0];

    Current_lat_coordiante = x;
    Current_lng_coordiante = y;

    var mapFocus = document.getElementById('map');
    mapFocus.focus();

    var geometryFound = await InitalPointQuery(y, x)

    if (geometryFound) {
        await flyToPoint(x, y)
    }
}

async function openModal(html, ReportIsDetailed, jsonData) {
    $("#myModal").modal({ backdrop: false });
    $('.modal').modal('hide');

    $(".modal-dialog").draggable({
        handle: ".modal-header"
    });

    $(".modal-body").html(html);

    $("#myModal").off('shown.bs.modal');

    /*
    if (ReportIsDetailed) {
        await loadGoogleCharts();
        $("#myModal").on('shown.bs.modal', function (e) {
            drawChart(jsonData);
        });
    }
        */

    $("#myModal").on('hide.bs.modal', function (e) {
        $(".modal iframe").attr('src', "");
    });

    $("#myModal").modal({ backdrop: false });

    $('#myModal').modal({
        backdrop: 'false',  // Allow interactions with elements behind the modal
        keyboard: false,  // Keep the modal open with Escape key disabled
        focus: false      // Prevent automatic focus trapping
    });

    // Ensure focus doesn't get trapped by the modal
    $(document).off('focusin.bs.modal');


}


async function loadGoogleCharts() {
    return new Promise((resolve, reject) => {
        var script = document.createElement('script');
        script.src = 'https://www.gstatic.com/charts/loader.js?version={version}';
        document.head.appendChild(script);

        script.onload = function () {
            google.charts.load('current', { 'packages': ['corechart'] });
            google.charts.setOnLoadCallback(resolve);
        };

        script.onerror = reject;
    });
}

function classifyData(JasonData) {
    const counts = {};

    for (let i = 0; i < JasonData.probePoint.length; i++) {
        const geoPoten = JasonData.probePoint[i].properties.geoPoten;
        if (counts[geoPoten]) {
            counts[geoPoten]++;
        } else {
            counts[geoPoten] = 1;
        }
    }

    return counts;
}

async function PrintPDF() {
    var pdf = new jsPDF('p', 'pt', 'letter');
    var MainReportsource = await GetReport();

    var Title = '<h1 style="margin-bottom: 30px;">Geothermal Report - germa</h1>';

    var source = '<div style="font-size: 14px;">' + Title + '<br>' + MainReportsource + '</div>';

    specialElementHandlers = {
        '#bypassme': function (element, renderer) {
            return true;
        }
    };

    margins = {
        top: 40,
        bottom: 60,
        left: 40,
        width: 522
    };

    pdf.fromHTML(
        source,
        margins.left,
        margins.top, {
        'width': margins.width,
        'elementHandlers': specialElementHandlers
    },
        function (dispose) {
            var pdfDataUri = pdf.output('datauristring');
            var newWindow = window.open();
            newWindow.document.write('<style>body, html { margin: 0; padding: 0; }</style>');
            newWindow.document.write('<iframe width="100%" height="100%" style="margin: 0; padding: 0; border: none;" src="' + pdfDataUri + '"></iframe>');
            newWindow.document.title = 'Geothermal-Report';
        }, margins);
}

//Close Modal, when adress bar ist clicked
document.addEventListener('DOMContentLoaded', function () {
    var addressInput = document.getElementById('address-input');
    if (addressInput) {
        addressInput.addEventListener('focus', function () {
            if ($('#myModal').hasClass('in')) {
                $('.modal').modal('hide');
            }
            addressInput.focus();
            removeLandParcels();
        });
    }
});

function httpGet(theUrl) {
    return new Promise(function (resolve, reject) {
        var xmlHttp = new XMLHttpRequest();
        xmlHttp.open("GET", theUrl, true); // true for asynchronous request

        xmlHttp.onload = function () {
            if (xmlHttp.status >= 200 && xmlHttp.status < 300) {
                resolve(xmlHttp.responseText);
            } else {
                reject(new Error(`HTTP request failed with status ${xmlHttp.status}`));
            }
        };

        xmlHttp.onerror = function () {
            reject(new Error('HTTP request failed'));
        };

        xmlHttp.send();
    });
}


async function CreateReportHTML(reportData, ReportIsDetailed, DisplayGrafics) {

    var String_geo_poten_restrict = ``;
    if (reportData.geo_poten_restrict.length > 0) {
        for (let i = 0; i < reportData.geo_poten_restrict.length; i++) {
            String_geo_poten_restrict = String_geo_poten_restrict + `<p><strong>Restriktionsflächen:</strong> ${reportData.geo_poten_restrict[i]}</p>`
        }
    }

    var String_ProtectionAreas = ``;
    if (reportData.protecitonList.length > 0) {
        for (let i = 0; i < reportData.protecitonList.length; i++) {
            if (i == 0) {
                String_ProtectionAreas = String_ProtectionAreas + `<p><strong>Schutzgebiete:</strong> ${reportData.protecitonList[i]}`;
            } else {
                String_ProtectionAreas = String_ProtectionAreas + `, ${reportData.protecitonList[i]}`;
            }
        }
        String_ProtectionAreas = String_ProtectionAreas + `</p>`;
    }

    //Get Unique Elemets
    reportData.verordnung = [...new Set(reportData.verordnung)]
    reportData.veror_link = [...new Set(reportData.veror_link)]

    var String_Water_protec_areas = ``;
    if (reportData.verordnung.length > 0) {
        for (let i = 0; i < reportData.verordnung.length; i++) {
            String_Water_protec_areas = String_Water_protec_areas + `<p><strong>Wasserschutzgebiet:</strong><a href="${reportData.veror_link[i]}">${reportData.verordnung[i]}</a></p>`
        }
    }

    var holstein = ``;
    if (reportData.holstein != "") {
        holstein = `<p><strong>Holstein-Schicht:</strong> ${ChangeDotToComman(reportData.holstein)} m u. GOK</p>`;
    }

    var rupelton = ``;
    if (reportData.rupelton != "") {
        rupelton = `<p><strong>Rupelton-Teufe:</strong> ${ChangeDotToComman(reportData.rupelton)} m u. GOK</p>`;
    }

    var maxDepth = ``;
    if (reportData.totalMaxDepth != "") {
        maxDepth = `<p><strong>Maximale Bohrtiefen Begrenzung: </strong> ${reportData.totalMaxDepth} m u. GOK</p>`;
    }

    var backgroundColor = reportData.activeRestriction ? 'background-color: rgba(255, 87, 51, 0.2);' : '';

    if (reportData.geo_poten_100m_with_2400ha == '') {
        backgroundColor = 'background-color: rgba(255, 87, 51, 0.2);'
    }

    var html = `
    <div class="geothermal-report" style="${backgroundColor}">`

    html = html + `
    <p style="color:red; font-size:13px;"><b>Für den folgenden Grundlagenbericht zur Nutzung von Erdwärmesonden am ausgewählten Standort wird keine Gewähr auf Richtigkeit und/oder Vollständigkeit genommen.<br>
Die Nutzung und/oder Weitergabe der Angaben und Daten erfolgt auf eigenes Risiko.<br>
Grundsätzlich ist für geothermische Kundenanfragen stets die Fachplanung O-GT-T zuständig und durch den Vertrieb einzubeziehen.</b></p><br>
`

    if (ReportIsDetailed) {

        if (reportData.totalMaxDepth > 100) {

            html = html + `
            <h3><strong>Jahresentzugsarbeit - ${reportData.totalMaxDepth} m, 2400 h/a:</strong> ${ChangeDotToComman(Math.round(reportData.extraction_KW_2400_Full_Load_Hours / 10) / 100)} MWh</h3>`

            html = html + `
            <h3><strong>Entzugsleistung -  ${reportData.totalMaxDepth} m:</strong> ${ChangeDotToComman(Math.round(reportData.extraction_KW * 10) / 10)} KW</h3>`

            html = html + `<hr style="border: 1px dashed #000; margin: 20px 0;">`;

        }


        html = html + `
    <h3><strong>Jahresentzugsarbeit - ${Math.min(reportData.totalMaxDepth, 100)} m, 2400 h/a:</strong> ${ChangeDotToComman(Math.round(reportData.limited_100m_Extraction_KW_2400_Full_Load_Hours / 10) / 100)} MWh</h3>`

        html = html + `
            <h3><strong>Entzugsleistung - ${Math.min(reportData.totalMaxDepth, 100)} m:</strong> ${ChangeDotToComman(Math.round(reportData.limited_100m_Extraction_KW * 10) / 10)} KW</h3>`

        html = html + `<hr style="border: 1px dashed #000; margin: 20px 0;">`;

        html += `
        <div class="input-row">
            <label for="waermebedarfInput" class="input-label">Wärmebedarf:</label>
            <input 
                type="number" 
                id="waermebedarfInput" 
                class="input-field"
                value="${(Math.round(reportData.actInsolation / 10) / 100).toFixed(3)}" 
                min="0" 
                max="${Number.MAX_SAFE_INTEGER}" 
                step="1"
                oninput="adjustInputWidth(this)"
            >
            <span class="input-unit">MWh</span>
        </div>
    `;

        html += `
        <div class="selection-row">
        <label for="RegenerationSelect" class="input-label">Regeneration:</label>
        <select id="RegenerationSelect" class="selection-dropdown" onchange="RegenerationChangeEvent(this)">
            <option value="None">Keine</option>
            <option value="Half">50% Regeneration</option>
            <option value="Full">Volle Regeneration</option>
        </select>
    </div>
    `;

        html += `
        <div class="selection-row">
        <label for="FlowTypeSelect" class="input-label">Strömung:</label>
        <select id="FlowTypeSelect" class="selection-dropdown" onchange="FlowTypeChangeEvent(this)">
            <option value="false">Laminar</option>
            <option value="true">Turbulent</option>>
        </select>
    </div>
    `;

        html = html + `<h4><strong>Wärmeversorgung (${Math.min(reportData.totalMaxDepth, 100)} m | COP${reportData.cop}):</strong> ${reportData.actInsolation_Coverage_100}%</h4>`

        if (reportData.totalMaxDepth > 100) {

            html = html + `<h4><strong>Wärmeversorgung (${reportData.totalMaxDepth} m | COP${reportData.cop}):</strong> ${reportData.actInsolation_Coverage_Maxdepth}%</h4>`

        }

    }

    /*if (ReportIsDetailed) {
        html = html + `
            <h3><strong>Rating:</strong> ${Math.round(median(reportData.totalRating) * 100) / 100} von 10</h3>
            <br>`
    }*/

    if (ReportIsDetailed) {
        if (DisplayGrafics) {
            html = html + `<div id='boxplotDiv' style='width: 550px; height: 400px;'></div>`
        }
    }

    html = html + `
        <p><strong>Gemeinde:</strong> ${reportData.land_parcels_gemeinde}</p>
        <p><strong>Flurstück:</strong> ${reportData.land_parcel_number}</p>
        ${holstein}
        ${rupelton}
        ${maxDepth}`


    if (ReportIsDetailed) {

        //Expected groundwater hight
        //html = html + `
        //<p><strong>ZeHGW:</strong> ${reportData.zeHGW} meter</p>`

        //Usable Area
        html = html + `
    <p><strong>Nutzbare Fläche:</strong> ${Math.round(reportData.usable_Area * 100) / 100} m&sup2</p>`
        //Number Of Probes
        html = html + `
    <p><strong>EWS Anzahl:</strong> ${reportData.probePoint.length}</p>`

    }

    html = html + `
    ${String_geo_poten_restrict}
    ${String_Water_protec_areas}
    ${String_ProtectionAreas}`

    //reportData.activeRestriction

    //if(DisplayGrafics) {
    //    html = html + `<div id="piechart" style="width: 500px; height: 300px;"></div>`
    //}

    html = html + `
        <br>
        <p><strong>Entzugsleistungen 2400 h/a (W/m):</strong></p>
        <ul>
            <li><strong>100 m:</strong> ${ChangeDotToComman(reportData.geo_poten_100m_with_2400ha)}</li>
            <li><strong>80 m:</strong> ${ChangeDotToComman(reportData.geo_poten_80m_with_2400ha)}</li>
            <li><strong>60 m:</strong> ${ChangeDotToComman(reportData.geo_poten_60m_with_2400ha)}</li>
            <li><strong>40 m:</strong> ${ChangeDotToComman(reportData.geo_poten_40m_with_2400ha)}</li>
        </ul>
        <p><strong>Entzugsleistungen 1800 h/a (W/m):</strong></p>
        <ul>
            <li><strong>100 m:</strong> ${ChangeDotToComman(reportData.geo_poten_100m_with_1800ha)}</li>
            <li><strong>80 m:</strong> ${ChangeDotToComman(reportData.geo_poten_80m_with_1800ha)}</li>
            <li><strong>60 m:</strong> ${ChangeDotToComman(reportData.geo_poten_60m_with_1800ha)}</li>
            <li><strong>40 m:</strong> ${ChangeDotToComman(reportData.geo_poten_40m_with_1800ha)}</li>
        </ul>
        <p><strong>Spezifische Wärmeleitfähigkeit (Wm<sup>-1</sup>K<sup>-1</sup>):</strong></p>
        <ul>
            <li><strong>100 m:</strong> ${ChangeDotToComman(reportData.thermal_con_100)}</li>
            <li><strong>80 m:</strong> ${ChangeDotToComman(reportData.thermal_con_80)}</li>
            <li><strong>60 m:</strong> ${ChangeDotToComman(reportData.thermal_con_60)}</li>
            <li><strong>40 m:</strong> ${ChangeDotToComman(reportData.thermal_con_40)}</li>
        </ul>
        <p><strong>Grundwassertemperatur unter Geländeoberfläche (°C):</strong></p>
        <ul>
            <li><strong>20 bis 100 m:</strong> ${ChangeDotToComman(reportData.mean_water_temp_20to100)}</li>
            <li><strong>60 m:</strong> ${ChangeDotToComman(reportData.mean_water_temp_60)}</li>
            <li><strong>40 m:</strong> ${ChangeDotToComman(reportData.mean_water_temp_40)}</li>
            <li><strong>20 m:</strong> ${ChangeDotToComman(reportData.mean_water_temp_20)}</li>
        </ul>`;

    html = html + `
        
</div>`

    setTimeout(() => {
        const inputElement = document.getElementById('waermebedarfInput');
        if (inputElement) {
            adjustInputWidth(inputElement);
        }
    }, 0);

    return html

}

function renderBoxplot(totalRating) {
    var y0 = totalRating;
    var trace1 = {
        y: y0,
        type: 'box',
        name: 'Punkte',
        jitter: 0.3,
        pointpos: -1.8,
        marker: {
            size: 8
        },
        boxpoints: 'all'
    };
    var data = [trace1];
    var layout = {
        title: {
            text: 'Bewertung',
            font: {
                size: 40
            }
        },
        yaxis: {
            tickfont: {
                size: 30
            }
        },
        xaxis: {
            title: {
                font: {
                    size: 25
                }
            },
            tickfont: {
                size: 25
            }
        },
        showlegend: false,
        dragmode: false
    };
    Plotly.newPlot('boxplotDiv', data, layout, { staticPlot: false });
}

function ChangeDotToComman(unformattedString) {

    var formatedString = String(unformattedString).replace(/\./g, ",");
    return (formatedString);
}

//Quick search by hitting enter
function handleKeyPress(event) {
    if (event.keyCode === 13) {
        searchAddress();
    }
}

// Back transformation

async function BackTransformationOfGeometry(geometry) {

    var JsonGeometry = JSON.parse(geometry);

    if (Array.isArray(JsonGeometry) && JsonGeometry.every(g => g.type === "Point" && Array.isArray(g.coordinates))) {
        return JsonGeometry.map(point => {
            const transformed = transformCoordinates([point.coordinates])[0];
            return {
                type: "Point",
                coordinates: transformed
            };
        });
    }

    if (JsonGeometry.type === "Point" && Array.isArray(JsonGeometry.coordinates)) {
        JsonGeometry.coordinates = transformCoordinates([JsonGeometry.coordinates])[0];
        return JsonGeometry;
    }

    if (JsonGeometry.type == "Polygon") {

        if (Array.isArray(JsonGeometry.coordinates) && JsonGeometry.coordinates.length > 0) {

            if (JsonGeometry.coordinates.length == 1) {

                var flattenedCoordinates = transformCoordinates(JsonGeometry.coordinates[0]);

                JsonGeometry.coordinates[0] = flattenedCoordinates;

            } else {
                for (let i = 0; i < JsonGeometry.coordinates.length; i++) {

                    if (JsonGeometry.coordinates[0][0].length == 2) {
                        var flattenedCoordinates = transformCoordinates(JsonGeometry.coordinates[i]);
                        JsonGeometry.coordinates[i] = flattenedCoordinates;
                    } else {
                        var flattenedCoordinates = transformCoordinates(JsonGeometry.coordinates[i][0]);
                        JsonGeometry.coordinates[i][0] = flattenedCoordinates;
                    }

                }
            }

            return JsonGeometry;

        } else {
            console.error("Error: Invalid coordinates format in LandParcelGeometry");
        }
    }

    if (JsonGeometry.type == "MultiPolygon") {
        if (Array.isArray(JsonGeometry.coordinates) && JsonGeometry.coordinates.length > 0) {
            for (let i = 0; i < JsonGeometry.coordinates.length; i++) {
                for (let j = 0; j < JsonGeometry.coordinates[i].length; j++) {
                    var flattenedCoordinates = transformCoordinates(JsonGeometry.coordinates[i][j]);
                    JsonGeometry.coordinates[i][j] = flattenedCoordinates;
                }
            }
            return JsonGeometry;
        } else {
            console.error("Error: Invalid coordinates format in MultiPolygon");
        }
    }

}

async function BackTransformationOfProbepoints(probePoints) {

    var cor = []

    for (let i = 0; i < probePoints.length; i++) {
        var probePoint = probePoints[i];

        var geometry = JSON.parse(probePoint.geometryJson);

        probePoint.coordinates = transformCoordinates([geometry.coordinates]);

        probePoints[i] = probePoint;

        //var transformedCoordinates = transformCoordinates(geometry.coordinates[0]);
        //cor[i] = transformedCoordinates;

    }
    //probePoint.geometryJson = cor;
    return probePoints;
}

function transformCoordinates(coordinates) {
    var transformedCoordinates = [];

    for (var i = 0; i < coordinates.length; i++) {
        var pair = coordinates[i];

        if (Array.isArray(pair) && pair.length === 2) {
            transformedCoordinates.push(proj4("EPSG:25833", "EPSG:4326", pair));
        } else {
            console.error("Error: Geometry could not ne transformed");
            return [];
        }
    }

    return transformedCoordinates;
}

function median(numbers) {
    if (numbers.length === 0) return 0;
    numbers.sort((a, b) => a - b);

    const mid = Math.floor(numbers.length / 2);

    if (numbers.length % 2 !== 0) {
        return numbers[mid];
    }

    return (numbers[mid - 1] + numbers[mid]) / 2;
}

async function clearSelected() {

    changeCursor('default');

    document.getElementById('btnSubmit').disabled = true;
    $("#myModal").modal({ backdrop: false });
    $('.modal').modal('hide');

    var mode = await GetMode();

    Current_lat_coordiante = null;
    Current_lng_coordiante = null;
    Multiple_lat = [];
    Multiple_lng = [];
    LastReportData = null;
    LastShownGeometry = null;
    LastShownCustomGeometry = null;

    document.getElementById('address-bar').style.backgroundColor = '#2D2D2D';
    document.getElementById('functions').style.backgroundColor = '#2D2D2D';
    document.getElementById('branding-text').textContent = 'GERMAG';
    document.getElementById('btnNewArea').style.backgroundColor = 'white';
    document.getElementById('btnMinusArea').style.backgroundColor = 'white';
    document.getElementById('btnAddSuperiorArea').style.backgroundColor = 'white';
    document.getElementById("btnMinusArea").disabled = true;
    document.getElementById("btnAddSuperiorArea").disabled = true;
    document.getElementById('exportDropdown').classList.add('disabled')

    AllGeometryLists = [];
    AllPolygons = [];
    EditGeometryListTemp = [];
    EditGeometryTemp = null;

    Minus_AllGeometryLists = [];
    Minus_EditGeometryListTemp = [];
    Minus_EditGeometryTemp = null;

    AllSuperiorGeometryLists = [];
    EditSuperiorGeometryListTemp = [];
    EditSuperiorGeometryTemp = null;

    T_ProbePointsGeometry = null;
    T_UsableGeometry = null;
    T_ResrictionGeometry = null;

    LastReportGeometry_UsabeGeometry = null;
    LastReportGeometry_ResrictionGeometry = null;
    LastReportGeometry_ProbePointsGeometry = null;

    const UserDemandInput = document.getElementById('waermebedarfInput');

    if (UserDemandInput) {
        UserDemandInput.value = null;
    }

    await removeLandParcels();
    await SetMode(0);

}

async function modeNewArea() {

    await dissolveArea();

    changeCursor('crosshair');

    await SetMode(1);
    document.getElementById('address-bar').style.backgroundColor = '#AF4600';
    document.getElementById('functions').style.backgroundColor = '#AF4600';
    document.getElementById('branding-text').textContent = 'Edit Mode';
    document.getElementById('btnNewArea').style.backgroundColor = '#ff6700';
    document.getElementById('btnMinusArea').style.backgroundColor = 'white';

    document.getElementById("btnMinusArea").disabled = true;
    document.getElementById("btnAddSuperiorArea").disabled = true;
    document.getElementById('exportDropdown').classList.add('disabled')

    Minus_AllGeometryLists = [];
    Minus_EditGeometryListTemp = [];
    Minus_EditGeometryTemp = null;

    AllSuperiorGeometryLists = [];
    EditSuperiorGeometryListTemp = [];
    EditSuperiorGeometryTemp = null;

}


async function modeSuperiorArea() {

    await dissolveArea();

    await SetMode(3);

    changeCursor('crosshair');
    document.getElementById('address-bar').style.backgroundColor = '#AF4600';
    document.getElementById('functions').style.backgroundColor = '#AF4600';
    document.getElementById('branding-text').textContent = 'Edit Mode';
    document.getElementById('btnAddSuperiorArea').style.backgroundColor = '#ff6700';
    document.getElementById('btnMinusArea').style.backgroundColor = 'white';

    document.getElementById('exportDropdown').classList.add('disabled')

}

async function modeMinusArea() {

    await dissolveArea();

    await SetMode(2);

    changeCursor('crosshair');

    document.getElementById('address-bar').style.backgroundColor = '#AF4600';
    document.getElementById('functions').style.backgroundColor = '#AF4600';
    document.getElementById('branding-text').textContent = 'Edit Mode';
    document.getElementById('btnMinusArea').style.backgroundColor = '#ff6700';
    document.getElementById('btnNewArea').style.backgroundColor = 'white';
    document.getElementById('btnAddSuperiorArea').style.backgroundColor = 'white';

    AllGeometryLists = [];
    EditGeometryListTemp = [];
    EditGeometryTemp = null;

}

async function dissolveArea() {

    if (SelectedMode == 1) {

        //Inital configuration

        if (!Array.isArray(AllGeometryLists)) {
            AllGeometryLists = [AllGeometryLists];
        }

        //Union new Geometry

        if (EditGeometryListTemp.length >= 3) {
            AllGeometryLists.push(EditGeometryTemp);
            EditGeometryListTemp = [];
            EditGeometryTemp = null;
        }

        //Append the last click geometry when it is given

        if(LastShownGeometry != null){
            AllGeometryLists.push(LastShownGeometry);
            LastShownGeometry = null;
        }

        if (AllGeometryLists != 0) {
            let combined = AllGeometryLists[0];
            for (let i = 1; i < AllGeometryLists.length; i++) {
                combined = turf.union(combined, AllGeometryLists[i]);
            }
            AllGeometryLists = combined;
        }

        LastShownCustomGeometry = AllGeometryLists; //Save for bad days

        await RenderAllGeometryLists();
        await AddGeoJson(EditGeometryTemp); //<- Extend for all geometrys A list AddGeoJson

    }

    if (SelectedMode == 2) {

        if (!Array.isArray(Minus_AllGeometryLists)) {
            Minus_AllGeometryLists = [Minus_AllGeometryLists];
        }

        if (Minus_EditGeometryListTemp.length >= 3) {
            Minus_AllGeometryLists.push(Minus_EditGeometryTemp);
            Minus_EditGeometryListTemp = [];
            Minus_EditGeometryTemp = null;
        }

        /*
        if (Minus_AllGeometryLists != 0) {
            let combined = Minus_AllGeometryLists[0];
            for (let i = 1; i < Minus_AllGeometryLists.length; i++) {
                combined = turf.union(combined, Minus_AllGeometryLists[i]);
            }
            Minus_AllGeometryLists = combined;
        }*/

        await RenderAllGeometryLists();
        await AddGeoJson(Minus_EditGeometryTemp, "#ff6600"); //<- Extend for all geometrys A list AddGeoJson
    }


    if (SelectedMode == 3) {

        if (!Array.isArray(AllSuperiorGeometryLists)) {
            AllSuperiorGeometryLists = [AllSuperiorGeometryLists];
        }

        if (EditSuperiorGeometryListTemp.length >= 3) {
            AllSuperiorGeometryLists.push(EditSuperiorGeometryTemp);
            EditSuperiorGeometryListTemp = [];
            EditSuperiorGeometryTemp = null;
        }

        await RenderAllGeometryLists();
        await AddGeoJson(EditSuperiorGeometryTemp, "#00A502"); //<- Extend for all geometrys A list AddGeoJson
    }
}

async function RenderAllGeometryLists() {
    await removeLandParcels();

    if (SelectedMode == 1) {

        if (!Array.isArray(AllGeometryLists)) {
            await AddGeoJson(AllGeometryLists);
        } else {
            for (const geometry of AllGeometryLists) {
                await AddGeoJson(geometry);
            }
        }

        if(LastShownGeometry != null){
            await AddGeoJson(LastShownGeometry);
        }
    }

    if (SelectedMode == 2) {
        if (!Array.isArray(Minus_AllGeometryLists)) {
            await AddGeoJson(Minus_AllGeometryLists, "#ff6600");
        } else {
            for (const geometry of Minus_AllGeometryLists) {
                await AddGeoJson(geometry, "#ff6600");
            }
        }

        //restore old Selected Area in mode2

        await CreateLandParcel(LastReportGeometry_UsabeGeometry, '#00ff00', '#00ff00', 2, 0, 0.2);  //2,0,0.2
        await CreateLandParcel(LastReportGeometry_ResrictionGeometry, '#ff6600', '#ff6600', 2, 1, 0.2);
        await AddGeoJson(AllSuperiorGeometryLists, '#00A502');
        await CreateAllPoints(LastReportGeometry_ProbePointsGeometry);

    }

    if (SelectedMode == 3) {

        if (!Array.isArray(AllSuperiorGeometryLists)) {
            await AddGeoJson(AllSuperiorGeometryLists, "#00A502");
        } else {
            for (const geometry of AllSuperiorGeometryLists) {
                await AddGeoJson(geometry,"#00A502");
            }
        }

        await CreateLandParcel(LastReportGeometry_UsabeGeometry, '#00ff00', '#00ff00', 2, 0, 0.2);  //2,0,0.2
        await CreateLandParcel(LastReportGeometry_ResrictionGeometry, '#ff6600', '#ff6600', 2, 1, 0.2);
        await AddGeoJson(Minus_AllGeometryLists, '#ff6600');
        await CreateAllPoints(LastReportGeometry_ProbePointsGeometry);
    }
}

async function CreateEditArea(lng, lat) {

    EditGeometryListTemp.push([lng, lat]);

    EditGeometryTemp = createGeometry(EditGeometryListTemp);

    if (EditGeometryListTemp.length >= 3) {
        document.getElementById('btnSubmit').disabled = false;
    }

    await removeLandParcels();

    await RenderAllGeometryLists();

    await AddGeoJson(EditGeometryTemp); //<- Extend for all geometrys A list AddGeoJson
}

async function CreateEditSuperiorArea(lng, lat) {

    EditSuperiorGeometryListTemp.push([lng, lat]);

    EditSuperiorGeometryTemp = createGeometry(EditSuperiorGeometryListTemp);

    if (EditSuperiorGeometryListTemp.length >= 3) {
        document.getElementById('btnSubmit').disabled = false;
    }

    await removeLandParcels();

    await RenderAllGeometryLists();

    await AddGeoJson(EditSuperiorGeometryTemp,"#00A502"); //<- Extend for all geometrys A list AddGeoJson
}

async function SubtractEditArea(lng, lat) {

    Minus_EditGeometryListTemp.push([lng, lat]);

    Minus_EditGeometryTemp = createGeometry(Minus_EditGeometryListTemp);

    if (Minus_EditGeometryListTemp.length >= 3) {
        document.getElementById('btnSubmit').disabled = false;
    }

    await removeLandParcels();

    await RenderAllGeometryLists();

    await AddGeoJson(Minus_EditGeometryTemp, "#ff6600");//<- Extend for all geometrys A list AddGeoJson
}

function createGeometry(Orginalcoords) {

    let coords = Orginalcoords.slice();

    if (coords.length === 1) {
        // Create a point
        let point = turf.point(coords[0]);
        return point;
    } else if (coords.length === 2) {
        // Create a line
        let line = turf.lineString(coords);
        return line;
    } else if (coords.length >= 3) {
        // Ensure the polygon is closed
        if (coords[0][0] !== coords[coords.length - 1][0] || coords[0][1] !== coords[coords.length - 1][1]) {
            coords.push(coords[0]);
        }
        // Create a polygon
        let polygon = turf.polygon([coords]);
        return polygon;
    } else {
        console.error("Invalid coordinates array");
        return null;
    }
}

async function popEditGeometryList() { //STRG+Z Control

    //New custom area
    if (SelectedMode == 1) {

        document.getElementById('btnSubmit').disabled = true;
        if (EditGeometryListTemp[0] == null) { return; }
        EditGeometryListTemp.pop();
        await removeLandParcels();
        if (EditGeometryListTemp[0] == null) {
            await RenderAllGeometryLists();
            return;
        }

        if (EditGeometryListTemp.length >= 3) {
            document.getElementById('btnSubmit').disabled = false;
        }
        SpareGeometry = createGeometry(EditGeometryListTemp);

        await RenderAllGeometryLists();
        await AddGeoJson(SpareGeometry);
        EditGeometryTemp = SpareGeometry;
    }
    //Add subtraction area
    if (SelectedMode == 2) {
        document.getElementById('btnSubmit').disabled = true;
        if (Minus_EditGeometryListTemp[0] == null) { return; }
        Minus_EditGeometryListTemp.pop();
        await removeLandParcels();
        if (Minus_EditGeometryListTemp[0] == null) {
            await RenderAllGeometryLists();
            return;
        }

        if (Minus_EditGeometryListTemp.length >= 3) {
            document.getElementById('btnSubmit').disabled = false;
        }
        SpareGeometry = createGeometry(Minus_EditGeometryListTemp);

        await RenderAllGeometryLists();
        await AddGeoJson(SpareGeometry, "#ff6600");
        Minus_EditGeometryTemp = SpareGeometry;
    }
    if (SelectedMode == 3) {
        document.getElementById('btnSubmit').disabled = true;
        if (EditSuperiorGeometryListTemp[0] == null) { return; }
        EditSuperiorGeometryListTemp.pop();
        await removeLandParcels();
        if (EditSuperiorGeometryListTemp[0] == null) {
            await RenderAllGeometryLists();
            return;
        }

        if (EditSuperiorGeometryListTemp.length >= 3) {
            document.getElementById('btnSubmit').disabled = false;
        }
        SpareGeometry = createGeometry(EditSuperiorGeometryListTemp);

        await RenderAllGeometryLists();
        await AddGeoJson(SpareGeometry, "#00A502");
        EditSuperiorGeometryTemp = SpareGeometry;
    }
}

function RegenerationChangeEvent() {
    ShowDetailedReport();
}

function FlowTypeChangeEvent() {
    ShowDetailedReport();
}

async function loadGroundWaterVectorData() {
    await addWaterQualityPointLayerToMap(groundwater_measuring_points_4326);
}
async function RemoveLayerGroundWater() {
    await RemoveGroundWaterVectorData(groundwater_measuring_points_4326);
}

async function loadDrillingVectorData() {
    await addDrillPointLayerToMap(drilling_points_4326);
}
async function RemoveDrillPointLayerFromMap() {
    await RemoveDrillingVectorData(drilling_points_4326);
}

function ExportToGeojson() {
    const allFeatures = [];

    if (T_UsableGeometry == null || T_ResrictionGeometry == null) { return; }

    /*
    if (T_UsableGeometry.type === 'FeatureCollection') {
        allFeatures.push(...T_UsableGeometry.features);
    } else if (T_UsableGeometry.type === 'Feature') {
        allFeatures.push(T_UsableGeometry);
    }


    if (T_ResrictionGeometry.type === 'FeatureCollection') {
        allFeatures.push(...T_ResrictionGeometry.features);
    } else if (T_ResrictionGeometry.type === 'Feature') {
        allFeatures.push(T_ResrictionGeometry);
    }*/

    // Add all probe points (already a FeatureCollection)
    allFeatures.push(...T_ProbePointsGeometry.features);

    const mergedCollection = turf.featureCollection(allFeatures);

    // Trigger download
    downloadGeoJSON(mergedCollection, 'GermaExport.geojson');
}

function downloadGeoJSON(geojson, filename) {
    const dataStr = "data:text/json;charset=utf-8," + encodeURIComponent(JSON.stringify(geojson));
    const downloadAnchorNode = document.createElement('a');
    downloadAnchorNode.setAttribute("href", dataStr);
    downloadAnchorNode.setAttribute("download", filename);
    document.body.appendChild(downloadAnchorNode); // required for Firefox
    downloadAnchorNode.click();
    downloadAnchorNode.remove();
}

//Modal INput field

function calculateInputWidth(value) {
    return (value.toFixed(3).length + 5); // +2 for padding space
}

function adjustInputWidth(input) {
    input.style.width = (input.value.length + 5) + 'ch';
}

//links

function OpenInformationSite() {
    window.open("Information.html", "_blank");
}

function OpenFeedbackSite() {
    window.open("https://forms.office.com/Pages/ResponsePage.aspx?id=L9UxkQadoUGEutS4pvysCDbIAPNqVStGlCyZplAfPvpUMFcyRkNNRThWNVpOR0lOUVlFR1JLSUlVNS4u", "_blank");
}

function OpenFeatureSite() {
    window.open("https://gasag.sharepoint.com/:l:/s/GSP_Unternehmensinterna/FIpW0xloCaNLsjQcqo5ne_EBpY_sWlzJUD1YmBt6vQY4og", "_blank");
}

function positionModal() {
    const modal = document.querySelector('#myModal .modal-dialog');
    const functionsContainer = document.querySelector('#functions');

    if (modal && functionsContainer) {
        const rect = functionsContainer.getBoundingClientRect();
        const offset = rect.bottom + 10; // 12px space below buttons
        modal.style.top = `${offset}px`;
    }
}
window.addEventListener('load', positionModal);
window.addEventListener('resize', positionModal);


async function OpenGoogleMaps() {
    // Get current center and zoom from Leaflet
    const center = map.getCenter();  // {lat, lng}
    const zoom = map.getZoom();

    // Build Google Maps URL
    const url = `https://www.google.com/maps/@${center.lat},${center.lng},${zoom}z`;

    // Open in new browser tab
    window.open(url, "_blank");
}


/*
async function ForceByPostApi() {
    await dissolveArea();
    await ShowDetailedReport('custom', LastShownGeometry);
}*/