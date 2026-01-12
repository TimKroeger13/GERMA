async function downloadLatexProject(ReportFlag = "Internal", ExportType = "LATEX") {
//ReportFlag = Internal / BIM 
//Export LATEX // PDF
  fileName = "Unknown";

  if(ReportFlag == 'BIM'){
  	ExportType = document.getElementById('OutputTypeSelect').value;
  }
  if(ReportFlag == 'Internal'){
  	ExportType = document.getElementById('OutputTypeSelectInternal').value;
  }

  try {

	if(ReportFlag == 'BIM'){
		var FormalName = document.getElementById("Form_wirtschaftseinheit").value;
		var Number = document.getElementById("Form_lfdNr").value;
		if(FormalName != ""){fileName = Number + "_" + replaceSpace(FormalName);}
	}
	if(ReportFlag == 'Internal'){
		var FormalName = document.getElementById("Form_Internal_wirtschaftseinheit").value;
		if(FormalName != ""){fileName = replaceSpace(FormalName);}
	}

	showLoadingOverlay("Generating LaTeX document...");
	hideInputFormulaBim();
	hideInputFormulaInternal();

	updateLoadingProgress(0, 9);
	map.removeLayer(esriSat);
	baseLayerLightColor.addTo(map);
	wmsLayerFlurstuecke.addTo(map);
    treeLayerGroup.addTo(map);
	
	var collectetData = await getAllReportData(ReportFlag);

	if(collectetData == null){
		hideLoadingOverlay();
		return;
	}

    // Generate LaTeX code
    const latexCode = GenerateLatexCode(collectetData, ReportFlag);

	const zip = new JSZip();
    zip.file(fileName + ".tex", latexCode);

	if(ReportFlag == 'BIM'){
		const excelTextFile = CreateExcelTextFile(collectetData);
		zip.file(fileName + ".txt", excelTextFile);
	}

    // Show loading overlay
  
    // Perform necessary actions
    zoomToGeoJsonList();

	updateLoadingProgress(14, 20);

    // Create a new ZIP file

    const picFolder = zip.folder("pic");

	var logoArrayBufferCopy = null;

    // Fetch the logo file (with error handling)
    try {
      const logoResponse = await fetch('pic/GasagLogoText.png');
      
      if (!logoResponse.ok) {
        throw new Error(`HTTP error! status: ${logoResponse.status}`);
      }

      const logoArrayBuffer = await logoResponse.arrayBuffer();
	  logoArrayBufferCopy = logoArrayBuffer;
      picFolder.file("GasagLogoText.png", logoArrayBuffer);
    } catch (error) {
      console.error("Failed to fetch the logo:", error);
    }

    // Hide controls and capture map
    hideLeafletControls();
	updateLoadingProgress(15, 20);
    await new Promise((res) => setTimeout(res, 1000)); // Small delay for Map rendering
	updateLoadingProgress(16, 20);
	await new Promise((res) => setTimeout(res, 1000)); // Small delay for Map rendering
	updateLoadingProgress(17, 20);
	await new Promise((res) => setTimeout(res, 1000)); // Small delay for Map rendering
	updateLoadingProgress(18, 20);
	await new Promise((res) => setTimeout(res, 1000)); // Small delay for Map rendering
	updateLoadingProgress(19, 20);
    const { buffer, imageWidth, imageHeight } = await captureLeafletMap();
    showLeafletControls();

    // Add captured map image to ZIP
    if (buffer) {
      picFolder.file("MapCapture.png", buffer);
    }

	updateLoadingProgress(20, 20);

	if(ExportType == "PDF"){
		await generateAndOpenPdf(latexCode, buffer, logoArrayBufferCopy,fileName);
		if(ReportFlag == 'BIM'){
			var excelCode = CreateExcelTextFile(collectetData);
		}
	}
	if(ExportType == "LATEX"){
		await ExportZIPFile(zip,fileName);
		if(ReportFlag == 'BIM'){
			var excelCodeTempOut = CreateExcelTextFile(collectetData);
			enableExcelCodeOverlay(excelCodeTempOut);
		}
	}

	if(excelCode != "" && excelCode != null){
		enableExcelCodeOverlay(excelCode);
	}

	hideLoadingOverlay();

  } catch (error) {
    console.error("An error occurred:", error);
    hideLoadingOverlay(); // Ensure the overlay is hidden even on error
  }

}

async function generateAndOpenPdf(latexCode, mapImageBuffer, logoImageBuffer,fileName) {
  const formData = new FormData();
  formData.append("latex", latexCode);
  formData.append("MapCapture.png", new Blob([mapImageBuffer]), "MapCapture.png");
  formData.append("GasagLogoText.png", new Blob([logoImageBuffer]), "GasagLogoText.png");

  try {
    const response = await fetch("https://germa-tool.gs-local.gasag.de/latexcompiler/compile", {
      method: "POST",
      body: formData
    });

    if (!response.ok) {
      const errorText = await response.text();
      alert("PDF generation failed: " + response.status + ": " + errorText);
      return;
    }

    const pdfBlob = await response.blob();
    const blobUrl = URL.createObjectURL(pdfBlob);

    // Create a download link and trigger download
    const a = document.createElement("a");
    a.href = blobUrl;
    a.download = fileName + ".pdf";
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);

    // Clean up the blob URL after a minute
    setTimeout(() => URL.revokeObjectURL(blobUrl), 60 * 1000);

  } catch (err) {
    alert("Network/Fetch error: " + err.message);
    console.error("Network/Fetch error details:", err);
  }
}

async function ExportZIPFile(zip,fileName){
	// Generate the ZIP and trigger download
    const content = await zip.generateAsync({ type: "blob" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(content);
    link.download = fileName + ".zip";
    document.body.appendChild(link);
    link.click();
    setTimeout(() => {
      link.remove();
      URL.revokeObjectURL(link.href);
    }, 100);

    // Hide loading overlay
    hideLoadingOverlay();
}

async function getAllReportData(ReportFlag){

	var AllreportData = {};

	//Data from the formula
	var formulaData = null;

	//Try to set Heat demand
	//if(document.getElementById("Form_waermeverbrauch").value == 0){
	//	document.getElementById("Form_waermeverbrauch").value = document.getElementById('waermebedarfInput').value*1000;
	//}

	if(ReportFlag == 'BIM'){
		formulaData = {
			waermeverbrauch: document.getElementById("Form_waermeverbrauch").value/1000,
			maxdrillingdepth: document.getElementById("Form_maxBohrtiefe").value
		};
		//Check if Heat demand was set
		if(document.getElementById("Form_waermeverbrauch").value == 0){
			alert(`Wärmevebrauch was not set`);
			return;
		}
	}
	if(ReportFlag == 'Internal'){
		formulaData = {
			waermeverbrauch: document.getElementById('waermebedarfInput').value,
			maxdrillingdepth: document.getElementById("MaxEWSDrillingDepthInput").value
		};
	}

	updateLoadingProgress(1, 20);

	var maximaleTechnicalDillingDepthToday = Math.min(100,formulaData.maxdrillingdepth);


	//Setting of values
	if(ReportFlag == 'BIM'){
		//document.getElementById("numInput").value = 6;
		//document.getElementById("FlurInput").value = 2;
		//document.getElementById("TreeInput").value = 0;
		//document.getElementById("EwsNumberInput").value = 0;
		//document.getElementById('toggleBuilding').checked = false;
		//document.getElementById('toggleTrees').checked =false;
		document.getElementById("COPInput").value = 4;
		document.getElementById('waermebedarfInput').value = formulaData.waermeverbrauch;
		document.getElementById('FlowTypeSelect').value = 'true' 	 //false true
	}


	//0reg - needed - 100m
    document.getElementById('toggleEWSFieldSize').checked = false; //coverage
    document.getElementById("MaxEWSDrillingDepthInput").value = maximaleTechnicalDillingDepthToday;
	document.getElementById('RegenerationSelect').value = "None"; //None Half Full

	await ShowDetailedReport();
	AllreportData["0regNeed_100"] = LastReportData;

	updateLoadingProgress(2, 20);

	//50reg - needed - 100m
    document.getElementById('toggleEWSFieldSize').checked = false; //coverage
    document.getElementById("MaxEWSDrillingDepthInput").value = maximaleTechnicalDillingDepthToday;
	document.getElementById('RegenerationSelect').value = "Half"; //None Half Full

	await ShowDetailedReport();
	AllreportData["50regNeed_100"] = LastReportData;

	updateLoadingProgress(3, 20);

	//10reg - needed - 100m
    document.getElementById('toggleEWSFieldSize').checked = false; //coverage
    document.getElementById("MaxEWSDrillingDepthInput").value = maximaleTechnicalDillingDepthToday;
	document.getElementById('RegenerationSelect').value = "Full"; //None Half Full

	await ShowDetailedReport();
	AllreportData["100regNeed_100"] = LastReportData;

	updateLoadingProgress(4, 20);


	//0reg - 100% coverage - 100m
    document.getElementById('toggleEWSFieldSize').checked = true; //coverage
    document.getElementById("MaxEWSDrillingDepthInput").value = maximaleTechnicalDillingDepthToday;
	document.getElementById('RegenerationSelect').value = "None"; //None Half Full

	await ShowDetailedReport();
	AllreportData["0regFull_100"] = LastReportData;

	updateLoadingProgress(5, 20);

	//50reg - 100% coverage - 100m
    document.getElementById('toggleEWSFieldSize').checked = true; //coverage
    document.getElementById("MaxEWSDrillingDepthInput").value = maximaleTechnicalDillingDepthToday;
	document.getElementById('RegenerationSelect').value = "Half"; //None Half Full

	await ShowDetailedReport();
	AllreportData["50regFull_100"] = LastReportData;

	updateLoadingProgress(6, 20);

	//100reg - 100% coverage - 100m
    document.getElementById('toggleEWSFieldSize').checked = true; //coverage
    document.getElementById("MaxEWSDrillingDepthInput").value = maximaleTechnicalDillingDepthToday;
	document.getElementById('RegenerationSelect').value = "Full"; //None Half Full

	await ShowDetailedReport();
	AllreportData["100regFull_100"] = LastReportData;

	updateLoadingProgress(7, 20);

	//###################################################################

	//0reg - needed
    document.getElementById('toggleEWSFieldSize').checked = false; //coverage
    document.getElementById("MaxEWSDrillingDepthInput").value = formulaData.maxdrillingdepth;
	document.getElementById('RegenerationSelect').value = "None"; //None Half Full

	await ShowDetailedReport();
	AllreportData["0regNeed"] = LastReportData;

	updateLoadingProgress(8, 20);

	//50reg - needed
    document.getElementById('toggleEWSFieldSize').checked = false; //coverage
    document.getElementById("MaxEWSDrillingDepthInput").value = formulaData.maxdrillingdepth;
	document.getElementById('RegenerationSelect').value = "Half"; //None Half Full

	await ShowDetailedReport();
	AllreportData["50regNeed"] = LastReportData;

	updateLoadingProgress(9, 20);

	//10reg - needed
    document.getElementById('toggleEWSFieldSize').checked = false; //coverage
    document.getElementById("MaxEWSDrillingDepthInput").value = formulaData.maxdrillingdepth;
	document.getElementById('RegenerationSelect').value = "Full"; //None Half Full

	await ShowDetailedReport();
	AllreportData["100regNeed"] = LastReportData;

	updateLoadingProgress(10, 20);

	//0reg - 100% coverage
    document.getElementById('toggleEWSFieldSize').checked = true; //coverage
    document.getElementById("MaxEWSDrillingDepthInput").value = formulaData.maxdrillingdepth;
	document.getElementById('RegenerationSelect').value = "None"; //None Half Full

	await ShowDetailedReport();
	AllreportData["0regFull"] = LastReportData;

	updateLoadingProgress(11, 20);

	//50reg - 100% coverage
    document.getElementById('toggleEWSFieldSize').checked = true; //coverage
    document.getElementById("MaxEWSDrillingDepthInput").value = formulaData.maxdrillingdepth;
	document.getElementById('RegenerationSelect').value = "Half"; //None Half Full

	await ShowDetailedReport();
	AllreportData["50regFull"] = LastReportData;

	updateLoadingProgress(12, 20);

	//100reg - 100% coverage
    document.getElementById('toggleEWSFieldSize').checked = true; //coverage
    document.getElementById("MaxEWSDrillingDepthInput").value = formulaData.maxdrillingdepth;
	document.getElementById('RegenerationSelect').value = "Full"; //None Half Full

	await ShowDetailedReport();
	AllreportData["100regFull"] = LastReportData;

	updateLoadingProgress(13, 20);

	//############################################################################################

	return AllreportData;

}




//LastReportData

function GenerateLatexCode(collectetData,ReportFlag) {

	var formulaData = null;

	if(ReportFlag == 'BIM'){
		formulaData = {
			lfdNr: document.getElementById("Form_lfdNr").value,
			wirtschaftseinheit: document.getElementById("Form_wirtschaftseinheit").value.replace(/_/g, '\\_'),
			bezirk: document.getElementById("Form_bezirk").value,
			postleitzahl: document.getElementById("Form_postleitzahl").value,
			grundstuecksflaeche: document.getElementById("Form_grundstuecksflaeche").value,
			flurstuecksnummer: document.getElementById("Form_flurstuecksnummer").value,
			waermeverbrauch: document.getElementById("Form_waermeverbrauch").value/1000,
			energietraeger: document.getElementById("Form_energietraeger").value,
			maxdrillingdepth: document.getElementById("Form_maxBohrtiefe").value,
			Form_remark1: document.getElementById("Form_remark1").value,
			Form_remark2: document.getElementById("Form_remark2").value
		};
	}
	if(ReportFlag == 'Internal'){
		formulaData = {
			wirtschaftseinheit: document.getElementById("Form_Internal_wirtschaftseinheit").value.replace(/_/g, '\\_'),
			bezirk: document.getElementById("Form_Internal_bezirk").value,
			postleitzahl: document.getElementById("Form_Internal_postleitzahl").value,
			grundstuecksflaeche: Math.round(collectetData["0regFull"].usable_Area),
			flurstuecksnummer: document.getElementById("Form_Internal_flurstuecksnummer").value,
			waermeverbrauch: document.getElementById('waermebedarfInput').value,
			maxdrillingdepth: document.getElementById("MaxEWSDrillingDepthInput").value,
			Form_remark1: document.getElementById("Form_Internal_remark1").value,
			Form_remark2: document.getElementById("Form_Internal_remark2").value
		};
	}

	//MaxDrillingDelth
	var maximaleTechnicalDillingDepthToday = Math.min(100,collectetData["0regFull"].probePoint[0].properties.maxDepth);

	var showHeatTable = "\\showHeatTabletrue"
	if(collectetData["0regFull"].probePoint[0].properties.maxDepth <= 100){
		showHeatTable = "\\showHeatTablefalse"
	}

	var CoverageAmount_100m_0reg = collectetData["0regNeed_100"].probePoint.length;
	if(collectetData["0regFull_100"].actInsolation_Coverage_Maxdepth < 100){CoverageAmount_100m_0reg = "\\small{Nicht möglich}";}
	var CoverageAmount_100m_50reg = collectetData["50regNeed_100"].probePoint.length;
	if(collectetData["50regFull_100"].actInsolation_Coverage_Maxdepth < 100){CoverageAmount_100m_50reg = "\\small{Nicht möglich}";}
	var CoverageAmount_100m_100reg = collectetData["100regNeed_100"].probePoint.length;
	if(collectetData["100regFull_100"].actInsolation_Coverage_Maxdepth < 100){CoverageAmount_100m_100reg = "\\small{Nicht möglich}";}

	var CoverageAmount_0reg = collectetData["0regNeed"].probePoint.length;
	if(collectetData["0regFull"].actInsolation_Coverage_Maxdepth < 100){CoverageAmount_0reg = "\\small{Nicht möglich}";}
	var CoverageAmount_50reg = collectetData["50regNeed"].probePoint.length;
	if(collectetData["50regFull"].actInsolation_Coverage_Maxdepth < 100){CoverageAmount_50reg = "\\small{Nicht möglich}";}
	var CoverageAmount_100reg = collectetData["100regNeed"].probePoint.length;
	if(collectetData["100regFull"].actInsolation_Coverage_Maxdepth < 100){CoverageAmount_100reg = "\\small{Nicht möglich}";}

	//geological condition

	var GlobalThermalConductivity = collectetData["0regFull"].probePoint[0].properties.thermalCon;

	var geologicalCondition = "Unknown";

	if(GlobalThermalConductivity >= 2){
		geologicalCondition = "Geeignet";
	}
	if(GlobalThermalConductivity >= 1.5 && GlobalThermalConductivity < 2){
		geologicalCondition = "Eingeschränkt";
	}
	if(GlobalThermalConductivity < 1.5){
		geologicalCondition = "Ungeeignet";
	}

	var totalRestrictionFile = [];

	//RestrictionAras

	if(containsWord(collectetData["0regFull"].geo_poten_restrict, "Holstein")){totalRestrictionFile.push("Tiefenbegrenzung");}
	if(containsWord(collectetData["0regFull"].geo_poten_restrict, "Wasserschutzgebieten")){totalRestrictionFile.push("Wasserschutzgebiet");}
	if(containsWord(collectetData["0regFull"].geo_poten_restrict, "Rupeltonhochlagen")){totalRestrictionFile.push("Rupeltonhochlage");}
	if(containsWord(collectetData["0regFull"].geo_poten_restrict, "artesisch")){totalRestrictionFile.push("Artesisch");}
	if(containsWord(collectetData["0regFull"].protecitonList , "Gartendenkmal")){totalRestrictionFile.push("Gartendenkmal");}
	if(containsWord(collectetData["0regFull"].protecitonList , "Biotop")){totalRestrictionFile.push("Biotop");}
	if(containsWord(collectetData["0regFull"].protecitonList , "Naturschutzgebiet")){totalRestrictionFile.push("Naturschutzgebiet");}
	if(containsWord(collectetData["0regFull"].protecitonList , "Naturdenkmal")){totalRestrictionFile.push("Naturdenkmal");}
	
	ParsedTotalRestrictionFile = "";

	if(totalRestrictionFile.length > 0){
		totalRestrictionFile.forEach(element => {
			ParsedTotalRestrictionFile = ParsedTotalRestrictionFile + "\\textbf{" + element + "},\\\\"
		});
		ParsedTotalRestrictionFile = ParsedTotalRestrictionFile.slice(0, -3);
	}else{
		ParsedTotalRestrictionFile = "\\textbf{Keine}";
	}

	////Kosten

	//Abdeckung bis 100%
	var costCoverage = Math.min(collectetData["100regFull"].actInsolation_Coverage_Maxdepth,100);

	//remarks

	var RemarkString = "";

	if(formulaData.Form_remark1 == "" && formulaData.Form_remark2 == ""){
		RemarkString = "\\item Keine Anmerkung";
	}
	if(formulaData.Form_remark1 != ""){
		RemarkString = RemarkString + "\\item " + formulaData.Form_remark1 + " ";
	}
	if(formulaData.Form_remark2 != ""){
		RemarkString = RemarkString + "\\item " + formulaData.Form_remark2 + " ";
	}

	//Flowtypes
	var FlowTypeRawValue = "";
	var FlowTypeSwitsh = document.getElementById('FlowTypeSelect').value;

	if(FlowTypeSwitsh == 'true'){
		FlowTypeRawValue = "Turbolent";
	}
	if(FlowTypeSwitsh == 'false'){
		FlowTypeRawValue = "Laminar";
	}

	//ReportFlag adeptions
	var UsableAreaString = "";
	var HadderString = "";
	var ThermalTextString = "";
	var AdressName = "";
	var EnergyProviderType = "";
	var ImageHight = 0;
	var FlowType = "";

	if(ReportFlag == 'BIM'){
		UsableAreaString = "Grundstücksfläche";
		HadderString = "Geothermie Potentialanalyse - Sondervermögen Immobilien des Landes Berlin (SILB), c/o BIM Berliner Immobilienmanagement GmbH";
		ThermalTextString = "Geologische Voraussetzungen";
		AdressName= "Wirtschaftseinheit";
		ImageHight = 10;
		EnergyProviderType = `
					\\hline
					\\parbox[t]{6.5cm}{\\textbf{Vorwiegender Energieträger:}} & \\parbox[t]{4.7cm}{${formulaData.energietraeger}} \\\\`;
	}
	if(ReportFlag == 'Internal'){
		UsableAreaString = "Für EWS nutzbare Fläche";
		HadderString = "Geothermie Potentialanalyse - Interner Bericht";
		ThermalTextString = "Wärmeleitfähigkeit ($W/mK$)";
		geologicalCondition = collectetData["100regFull"].probePoint[0].properties.thermalCon;
		AdressName = "Adresse/name";
		ImageHight = 9.5;
		FlowType = `
				\\rowcolor{accent!10} & & \\small{Strömung} & \\small\\textbf{${FlowTypeRawValue}} \\\\
				\\noalign{\\hrule height 1.5pt}`;
	}

	var latexCodeTempalte = `
\\documentclass[11pt,a4paper]{article}
\\usepackage[german]{babel}        % language support
\\usepackage[utf8]{inputenc}       % UTF-8 encoding
\\usepackage[T1]{fontenc}
\\usepackage[table]{xcolor}        % critical for \\rowcolor
\\usepackage{geometry}             % page margins
\\usepackage{graphicx}             % images
\\usepackage{tcolorbox}            % colored boxes
\\usepackage{array}                % for tabular and \\parbox
\\usepackage{fancyhdr} 
\\usepackage{tabularx}
\\usepackage{makecell}
\\usepackage{eurosym}
\\usepackage{ragged2e}
\\usepackage{dashrule} 
\\usepackage{pgfornament}


\\setlength{\\parindent}{0pt}
\\setlength{\\headheight}{14pt}     % prevents fancyhdr warning

% Moderne Farbdefinitionen
\\definecolor{darkblue}{HTML}{07608F}
\\definecolor{backrodunBlue}{HTML}{264D73}
\\definecolor{secondary}{RGB}{102,153,255}
\\definecolor{accent}{HTML}{D27B2C}
\\definecolor{darkgray}{RGB}{73,80,87}

% Table colors for Erdwärmesonden table
\\definecolor{headercolor}{HTML}{07608F}
\\definecolor{rowcolor1}{RGB}{230,240,255}
\\definecolor{rowcolor2}{HTML}{F2F2F2}

\\definecolor{noReg}{RGB}{220,235,255}       % light blue for no regeneration
\\definecolor{reg}{RGB}{255,230,230}         % light red/pink for regeneration
\\definecolor{gasagGreen}{HTML}{6EA43C}

% Seitenränder
\\geometry{left=2.5cm, right=2.5cm, top=3cm, bottom=3cm}

% Grafiken Pfad
\\graphicspath{{pic/}}

% Header/Footer Setup
\\pagestyle{fancy}
\\fancyhf{}
\\fancyhead[l]{%
	\\parbox[b]{0.7\\textwidth}{\\justifying\\raggedright\\footnotesize%
		\\scriptsize\\textcolor{darkgray}{\\textbf{${HadderString}}}%
	}%
}
\\fancyhead[R]{%
	\\parbox[b]{0.3\\textwidth}{\\raggedleft\\footnotesize%
		\\raisebox{0mm}{\\includegraphics[height=3.5mm]{GasagLogoText.png}}%
	}%
}
\\fancyfoot[L]{\\scriptsize\\makecell[l]{GASAG Solution Plus GmbH \\\\EUREF-Campus 23–24 10829 Berlin }}     % left footer
\\fancyfoot[C]{%
	\\hspace{0cm}
	\\scriptsize
	\\begin{tabular}{l}
		service-solution@gasag.de \\\\
		www.gasag-solution.de
	\\end{tabular}%
}

     % centered footer
\\fancyfoot[R]{\\thepage}  % right footer = page number
\\renewcommand{\\headrulewidth}{0.5pt}
\\renewcommand{\\headrule}{\\color{darkgray}\\hrule width\\headwidth height\\headrulewidth\\relax}
\\renewcommand{\\footrule}{\\color{darkgray}\\hrule width\\headwidth height\\headrulewidth\\relax}

% Titel-Styling
\\usepackage{titlesec}
\\titleformat{\\section}
{\\Large\\bfseries\\color{darkblue}}
{\\thesection}{1em}{}
[\\textcolor{darkblue}{\\titlerule[0.5pt]}]

\\titleformat{\\subsection}
{\\large\\bfseries\\color{secondary}}
{\\thesubsection}{1em}{}

% Table Styling
\\newcolumntype{L}[1]{>{\\raggedright\\arraybackslash}p{#1}}
\\newcolumntype{C}[1]{>{\\centering\\arraybackslash}p{#1}}
\\newcolumntype{R}[1]{>{\\raggedleft\\arraybackslash}p{#1}}

\\newif\\ifshowHeatTable
${showHeatTable}  % or \\showHeatTablefalse || \\showHeatTabletrue

\\newif\\ifshowCustomText
\\showCustomTexttrue  % or \\showCustomTextfalse || \\showCustomTexttrue

\\begin{document}
	
	% ===========================
	% Header Box
	% ===========================
	\\begin{center}
		\\begin{tcolorbox}[
			width=13cm,
			colback=white,
			colframe=black,
			boxrule=1.5pt,
			arc=2mm,
			left=3mm,
			right=3mm,
			top=2mm,
			bottom=2mm,
			sharp corners=all,
			valign=center
			]
			
			% === Title line ===
			\\parbox{\\textwidth}{\\large\\textbf{Potentialanalyse oberflächennahe Geothermie}}
			\\hrule
			\\vspace{2mm}
			
			% === Main info + logo ===
			\\begin{minipage}[t]{0.72\\textwidth}
				\\vspace{0pt}
				\\begin{tabular}{l|l}
					\\parbox[t]{6.5cm}{\\textbf{${AdressName}:}} & \\parbox[t]{4.7cm}{${formulaData.wirtschaftseinheit}} \\\\
					\\hline
					\\parbox[t]{6.5cm}{\\textbf{Bezirk:}} & \\parbox[t]{4.7cm}{${formulaData.bezirk}} \\\\
					\\hline
					\\parbox[t]{6.5cm}{\\textbf{Postleitzahl:}} & \\parbox[t]{4.7cm}{${formulaData.postleitzahl}} \\\\
					\\hline
					\\parbox[t]{6.5cm}{\\textbf{${UsableAreaString}:}} & \\parbox[t]{4.7cm}{${formulaData.grundstuecksflaeche}\\,m\\textsuperscript{2}} \\\\
					\\hline
					\\parbox[t]{6.5cm}{\\textbf{Flurstücks-Nummer:}} & \\parbox[t]{4.7cm}{${formulaData.flurstuecksnummer}} \\\\
					\\hline
					\\parbox[t]{6.5cm}{\\textbf{Wärmeverbrauch:}} & \\parbox[t]{4.7cm}{${formatGermanStringNumber(formulaData.waermeverbrauch)} MWh/a} \\\\${EnergyProviderType}
				\\end{tabular}
			\\end{minipage}%

			
		
			
			\\hrule
			
			% === Footer block ===
			\\begin{tabular}{l|l}
				\\parbox[t]{6.5cm}{\\textbf{Erstellt am:}} & \\parbox[t]{8cm}{\\today} \\\\
			\\end{tabular}
			
		\\end{tcolorbox}
	\\end{center}
	
	\\vfill
	
	% ===========================
	% Lagebild
	% ===========================
	\\begin{center}
		\\Large\\textbf{Lage- und Sondenplan} \\\\[2mm]
		\\includegraphics[width=\\textwidth,height=${ImageHight}cm,keepaspectratio]{MapCapture.png} \\\\[1mm]
		\\small
		\\textit{Der Lageplan zeigt eine beispielhafte Sondenfeld-Konstellation für die\\\\ Wirtschaftseinheit für die maximal auslegbare Anzahl an Erdwärmesonden.}
	\\end{center}
	
	\\vfill
	

	
	
	\\begin{comment}
		\\begin{tcolorbox}[
			colback=accent!10,
			colframe=accent,
			boxrule=1pt,
			arc=2mm,
			title={\\textbf{Wichtiger Hinweis}},
			fonttitle=\\bfseries,
			coltitle=white,
			colbacktitle=accent
			]
			Die vorliegende Einschätzung zur geothermischen Eignung des Standorts stellt eine erste, grobe Prüfung dar und dient lediglich der orientierenden Bewertung. Sie ersetzt \\textbf{keine detaillierte Machbarkeitsstudie}. Für belastbare Aussagen zur geothermischen Nutzbarkeit sind weiterführende Analysen erforderlich.
		\\end{tcolorbox}
	\\end{comment}
	
	% ===========================
	% Erdwärmesonden Table
	% ===========================
	
	%\\makecell[l]{Anzahl der Erdwärmesonden\\\\für das Bohrfeld}
	%Anzahl der Erdwärmesonden für das Bohrfeld
	
	
	
	\\begin{center}
		
		{\\Large{Parameter der simulierten Erdwärmesonden:}}
		
		\\vspace{4mm}
		
		\\renewcommand{\\arraystretch}{2} % increase row height
		
		% Wrap the table in a makebox that forces width = \\textwidth
		\\makebox[\\textwidth][c]{%
			\\begin{tabular}{!{\\vrule width 1.5pt}l!{\\vrule width 1.5pt}l!{\\vrule width 1.5pt}l!{\\vrule width 1.5pt}l!{\\vrule width 1.5pt}}
				\\noalign{\\hrule height 1.5pt}
				\\rowcolor{accent}
				\\textcolor{white}{\\textbf{Parameter}} & \\textcolor{white}{\\textbf{Wert}} &
				\\textcolor{white}{\\textbf{Parameter}} & \\textcolor{white}{\\textbf{Wert}} \\\\
				\\noalign{\\hrule height 1.5pt}
				\\rowcolor{accent!10} \\small\\makecell[l]{Anzahl der maximal\\\\platzierbaren Erdwärmesonden} & \\textbf{${collectetData["0regFull"].probePoint.length}} &
				\\small\\makecell[{{l}}]{\\rule{0pt}{2.5ex}Heute rechtlich \\& \\\\technisch mögliche Bohrtiefe\\rule[-1ex]{0pt}{0pt}}  & \\textbf{${maximaleTechnicalDillingDepthToday}m} \\\\
				\\noalign{\\hrule height 1.5pt}
				\\rowcolor{rowcolor2}\\small{Sondenabstand}& \\textbf{${document.getElementById("numInput").value}m} &
				\\small\\makecell[{{l}}]{\\rule{0pt}{2.5ex}Zukünftig rechtlich \\& \\\\technisch mögliche Bohrtiefe\\rule[-1ex]{0pt}{0pt}}  & \\textbf{${collectetData["0regFull"].probePoint[0].properties.maxDepth}m} \\\\
				\\noalign{\\hrule height 1.5pt}
				\\rowcolor{accent!10} \\small{Rechtliche Einschränkungen} & \\small\\makecell[{{l}}]{\\rule{0pt}{2.5ex}${ParsedTotalRestrictionFile}\\rule[-1ex]{0pt}{0pt}} &
				\\small{${ThermalTextString}} & \\textbf{${geologicalCondition}} \\\\
				\\noalign{\\hrule height 1.5pt}${FlowType}
			\\end{tabular}
		} % end makebox
	\\end{center}
	
	
	
	 \\newpage

	
	\\begin{center}
		{\\Large{Heute zulässige Wärmebereitstellung für \\textbf{${maximaleTechnicalDillingDepthToday}m} \\\\Bohrtiefe bei unterschiedlichen Regenerationsanteil:}}
		
		\\vspace{4mm}
		
		\\renewcommand{\\arraystretch}{1.5} % row height
		\\setlength{\\tabcolsep}{6pt}       % column separation
		
		{\\large
				\\begin{tabular}{!{\\vrule width 1.5pt}l
						!{\\vrule width 1.5pt}>{\\columncolor{accent!10}}l
						!{\\vrule width 1.5pt}>{\\columncolor{gasagGreen!10}}l
						!{\\vrule width 1.5pt}>{\\columncolor{gasagGreen!30}}l
						!{\\vrule width 1.5pt}}
					\\noalign{\\hrule height 1.5pt}
					\\rowcolor{accent}
					\\textcolor{white}{\\textbf{Parameter}} 
					& \\textcolor{white}{\\small\\textbf{\\makecell{\\rule{0pt}{5mm}0\\% \\\\ Regeneration}}}
					& \\textcolor{white}{\\small\\textbf{\\makecell{\\rule{0pt}{5mm}50\\% \\\\ Regeneration}}}
					& \\textcolor{white}{\\small\\textbf{\\makecell{\\rule{0pt}{5mm}100\\% \\\\ Regeneration}}} \\\\
					\\noalign{\\hrule height 1.5pt}
					\\small{Entzugsleistung (kW)} & ${Math.round(collectetData["0regFull_100"].extraction_KW)} & ${Math.round(collectetData["50regFull_100"].extraction_KW)} & ${Math.round(collectetData["100regFull_100"].extraction_KW)} \\\\
					\\noalign{\\hrule height 1.5pt}
					\\small{Jahresentzugsarbeit (MWh, 2400h)} & ${Math.round(collectetData["0regFull_100"].extraction_KW_2400_Full_Load_Hours/1000)} & ${Math.round(collectetData["50regFull_100"].extraction_KW_2400_Full_Load_Hours/1000)} & ${Math.round(collectetData["100regFull_100"].extraction_KW_2400_Full_Load_Hours/1000)} \\\\
					\\noalign{\\hrule height 1.5pt}
					\\small{Anteil am aktuellem Vebrauch (JAZ ${collectetData["0regFull"].cop})} & ${replaceDots(Math.round(collectetData["0regFull_100"].actInsolation_Coverage_Maxdepth*10)/10)}\\% & ${replaceDots(Math.round(collectetData["50regFull_100"].actInsolation_Coverage_Maxdepth*10)/10)}\\% & ${replaceDots(Math.round(collectetData["100regFull_100"].actInsolation_Coverage_Maxdepth*10)/10)}\\% \\\\
					\\noalign{\\hrule height 1.5pt}
					\\small\\makecell[{{l}}]{\\rule{0pt}{2.5ex}Anzahl der Erdwärmesonden bei 100\\%\\\\ Abdeckung des Wärmebedarfs\\rule[-1ex]{0pt}{0pt}} 
					& ${CoverageAmount_100m_0reg} &  ${CoverageAmount_100m_50reg} & ${CoverageAmount_100m_100reg} \\\\
					\\noalign{\\hrule height 1.5pt}
				\\end{tabular}
		}
	\\end{center}
	
	\\vspace{0mm}

    \\ifshowHeatTable
	
	\\begin{center}
		{\\Large{Wärmebereitstellung für perspektivisch zulässige \\textbf{${collectetData["0regFull"].probePoint[0].properties.maxDepth}m} \\\\Bohrtiefe bei unterschiedlichen Regenerationsanteil:}}
		
		\\vspace{4mm}
		
		\\renewcommand{\\arraystretch}{1.5} % row height
		\\setlength{\\tabcolsep}{6pt}       % column separation
		
		{\\large
				\\begin{tabular}{!{\\vrule width 1.5pt}l
						!{\\vrule width 1.5pt}>{\\columncolor{accent!10}}l
						!{\\vrule width 1.5pt}>{\\columncolor{gasagGreen!10}}l
						!{\\vrule width 1.5pt}>{\\columncolor{gasagGreen!30}}l
						!{\\vrule width 1.5pt}}
					\\noalign{\\hrule height 1.5pt}
					\\rowcolor{accent}
					\\textcolor{white}{\\textbf{Parameter}} 
					& \\textcolor{white}{\\small\\textbf{\\makecell{\\rule{0pt}{5mm}0\\% \\\\ Regeneration}}}
					& \\textcolor{white}{\\small\\textbf{\\makecell{\\rule{0pt}{5mm}50\\% \\\\ Regeneration}}}
					& \\textcolor{white}{\\small\\textbf{\\makecell{\\rule{0pt}{5mm}100\\% \\\\ Regeneration}}} \\\\
					\\noalign{\\hrule height 1.5pt}
					\\small{Entzugsleistung (kW)} & ${Math.round(collectetData["0regFull"].extraction_KW)} & ${Math.round(collectetData["50regFull"].extraction_KW)} & ${Math.round(collectetData["100regFull"].extraction_KW)} \\\\
					\\noalign{\\hrule height 1.5pt}
					\\small{Jahresentzugsarbeit (MWh, 2400h)} & ${Math.round(collectetData["0regFull"].extraction_KW_2400_Full_Load_Hours/1000)} & ${Math.round(collectetData["50regFull"].extraction_KW_2400_Full_Load_Hours/1000)} & ${Math.round(collectetData["100regFull"].extraction_KW_2400_Full_Load_Hours/1000)} \\\\
					\\noalign{\\hrule height 1.5pt}
					\\small{Anteil am aktuellem Vebrauch (JAZ ${collectetData["0regFull"].cop})} & ${replaceDots(Math.round(collectetData["0regFull"].actInsolation_Coverage_Maxdepth*10)/10)}\\% & ${replaceDots(Math.round(collectetData["50regFull"].actInsolation_Coverage_Maxdepth*10)/10)}\\% & ${replaceDots(Math.round(collectetData["100regFull"].actInsolation_Coverage_Maxdepth*10)/10)}\\% \\\\
					\\noalign{\\hrule height 1.5pt}
					\\small\\makecell[{{l}}]{\\rule{0pt}{2.5ex}Anzahl der Erdwärmesonden bei \\\\ vollständiger Abdeckung des Wärmebedarfs\\rule[-1ex]{0pt}{0pt}} 
					& ${CoverageAmount_0reg} & ${CoverageAmount_50reg} & ${CoverageAmount_100reg} \\\\
					\\noalign{\\hrule height 1.5pt}
				\\end{tabular}
			}
	\\end{center}

    \\fi
	
	\\vspace{-4mm}
	
	
		% ===========================
	% Kosten Box
	% ===========================
	
	\\begin{center}
		\\rule{0.3\\linewidth}{1pt}
	\\end{center}
	
%\\vspace{-2mm}
%\\begin{center}
%	\\pgfornament[width=0.5\\linewidth]{88}
%\\end{center}
%\\vspace{-2mm}

\\vspace{-4mm}
	
	
	\\begin{tcolorbox}[
		colback=accent!10,
		colframe=accent,
		boxrule=1pt,
		arc=2mm,
		title={\\textbf{Kostenschätzung - Effektive Investitionskosten netto}},
		fonttitle=\\bfseries,
		coltitle=white,
		colbacktitle=accent
		]
		\\small
		\\textcolor{black!70}{{Für die Annahme eines kompakten Erdwärmesondefeldes bei \\textbf{${collectetData["0regFull"].probePoint[0].properties.maxDepth}m Bohrtiefe}, \\textbf{voller Regeneration} und \\textbf{${replaceDots(costCoverage)}\\% Abdeckung des heutigen Wärmebedarfes} vor Förderung:}}
		
		\\begin{center}
			{\\bfseries\\LARGE ${Math.round(calculateCost(collectetData["100regNeed"].extraction_KW_2400_Full_Load_Hours / 1000,collectetData["100regNeed"].probePoint.length,collectetData["0regFull"].probePoint[0].properties.maxDepth)).toLocaleString('de-DE')} \\euro}
		\\end{center}
		\\vspace{0mm}
	\\end{tcolorbox}
	
	\\ifshowCustomText

	\\vspace{5mm}
	
	\\Large{\\textbf{Textliche Erläuterung:}}
	
	\\begin{itemize}
	${RemarkString}
	\\end{itemize}
	
    \\fi

\\end{document}
`;

	return latexCodeTempalte;
}

function CreateExcelTextFile(collectetData){

	//////Number Gathering//////

	const formulaData = {
		lfdNr: document.getElementById("Form_lfdNr").value,
		wirtschaftseinheit: document.getElementById("Form_wirtschaftseinheit").value,
		bezirk: document.getElementById("Form_bezirk").value,
		postleitzahl: document.getElementById("Form_postleitzahl").value,
		grundstuecksflaeche: document.getElementById("Form_grundstuecksflaeche").value,
		flurstuecksnummer: document.getElementById("Form_flurstuecksnummer").value,
		waermeverbrauch: document.getElementById("Form_waermeverbrauch").value/1000,
		energietraeger: document.getElementById("Form_energietraeger").value,
		maxdrillingdepth: document.getElementById("Form_maxBohrtiefe").value,
		Form_remark1: document.getElementById("Form_remark1").value,
		Form_remark2: document.getElementById("Form_remark2").value
  	};

	//geological condition

	var GlobalThermalConductivity = collectetData["0regFull"].probePoint[0].properties.thermalCon;

	var geologicalCondition = "Unknown";

	if(GlobalThermalConductivity >= 2){geologicalCondition = "Geeignet";}
	if(GlobalThermalConductivity >= 1.5 && GlobalThermalConductivity < 2){geologicalCondition = "Eingeschränkt";}
	if(GlobalThermalConductivity < 1.5){geologicalCondition = "Ungeeignet";}

	var totalRestrictionFile = [];

	//RestrictionAras
	if(containsWord(collectetData["0regFull"].geo_poten_restrict, "Holstein")){totalRestrictionFile.push("Tiefenbegrenzung durch erhöhte Salzkonzentrationen im Grundwasser unterhalb der Holstein-Schichten");}
	if(containsWord(collectetData["0regFull"].geo_poten_restrict, "Wasserschutzgebieten")){totalRestrictionFile.push("Wasserschutzgebiet");}
	if(containsWord(collectetData["0regFull"].geo_poten_restrict, "Rupeltonhochlagen")){totalRestrictionFile.push("Rupeltonhochlage");}
	if(containsWord(collectetData["0regFull"].geo_poten_restrict, "artesisch")){totalRestrictionFile.push("Artesisch");}
	if(containsWord(collectetData["0regFull"].protecitonList , "Gartendenkmal")){totalRestrictionFile.push("Gartendenkmal");}
	if(containsWord(collectetData["0regFull"].protecitonList , "Biotop")){totalRestrictionFile.push("Biotop");}
	if(containsWord(collectetData["0regFull"].protecitonList , "Naturschutzgebiet")){totalRestrictionFile.push("Naturschutzgebiet");}
	if(containsWord(collectetData["0regFull"].protecitonList , "Naturdenkmal")){totalRestrictionFile.push("Naturdenkmal");}
	
	ParsedTotalRestrictionFile = "";

	if(totalRestrictionFile.length > 0){
		totalRestrictionFile.forEach(element => {
			ParsedTotalRestrictionFile = ParsedTotalRestrictionFile + element +  ", "
		});
		ParsedTotalRestrictionFile = ParsedTotalRestrictionFile.slice(0, -2);
	}
	else{
		ParsedTotalRestrictionFile = "Keine";
	}

	var maximaleTechnicalDillingDepthToday = Math.min(100,collectetData["0regFull"].probePoint[0].properties.maxDepth);


	//remarks

	RemarkString = "";

	if(formulaData.Form_remark1 == "" && formulaData.Form_remark2 == ""){
		RemarkString = "";
	}
	if(formulaData.Form_remark1 != ""){
		RemarkString = RemarkString  + formulaData.Form_remark1 + " ";
	}
	if(formulaData.Form_remark2 != ""){
		RemarkString = RemarkString + formulaData.Form_remark2 + " ";
	}


	//Creation

	OutPutFile = [];
	OutPutFile.push(geologicalCondition); //Geologische Voraussetzungen
	OutPutFile.push("Ja"); //Hydrogeologie vorhanden
	OutPutFile.push(ParsedTotalRestrictionFile); //Rechtliche Einschränkungen
	OutPutFile.push("Ja"); //Bohrgenehmigung erforderlich
	OutPutFile.push(maximaleTechnicalDillingDepthToday); //Max. zulässige Bohrtiefe (m)
	OutPutFile.push(""); //Technisch machbare Bohrtiefe (m)
	OutPutFile.push(Math.round(collectetData["0regFull"].usable_Area)); //Verfügbare Bohrfläche (m²): Fläche
	OutPutFile.push(collectetData["0regFull"].probePoint.length); //Max. mögliche Bohrungen (Stk.)
	OutPutFile.push(collectetData["0regFull"].probePoint[0].properties.maxDepth); //Geplante Bohrtiefe je Bohrung (m)
	OutPutFile.push(Math.round(collectetData["0regFull"].extraction_KW_2400_Full_Load_Hours /1000)); //Theoretische Jahresenergie Entzug (kWh/a) ohne Regeneration: 
	OutPutFile.push(Math.round(collectetData["50regFull"].extraction_KW_2400_Full_Load_Hours /1000)); //Theoretische Jahresenergie Entzug (kWh/a) 50 Regeneration: 
	OutPutFile.push(Math.round(collectetData["100regFull"].extraction_KW_2400_Full_Load_Hours /1000)); //Theoretische Jahresenergie Entzug (kWh/a) 100 Regeneration: 
	OutPutFile.push("4"); //JAZ-Vorgabe (COP):
	OutPutFile.push(Math.round(collectetData["0regFull"].extraction_KW*4/3)); //Wärmepumpenleistung System (kW) ohne Regeneration: Entzug + COP?
	OutPutFile.push(Math.round(collectetData["50regFull"].extraction_KW*4/3)); //Wärmepumpenleistung System (kW) mit 50% Regeneration:
	OutPutFile.push(Math.round(collectetData["100regFull"].extraction_KW*4/3)); //Wärmepumpenleistung System (kW) mit 100% Regeneration:
	OutPutFile.push(replaceDots(collectetData["0regFull"].actInsolation_Coverage_Maxdepth/100)); //Anteil WP Geothermie an aktuellem Verbrauch ohne Regeneration: 
	OutPutFile.push(replaceDots(collectetData["50regFull"].actInsolation_Coverage_Maxdepth/100)); //Anteil WP Geothermie an aktuellem Verbrauch 50% Regeneration:
	OutPutFile.push(replaceDots(collectetData["100regFull"].actInsolation_Coverage_Maxdepth/100)); //Anteil WP Geothermie an aktuellem Verbrauch 100% Regeneration:
	OutPutFile.push("Ja"); //Visualisierung vorhanden
	OutPutFile.push(Math.round(calculateCost(collectetData["100regNeed"].extraction_KW_2400_Full_Load_Hours / 1000,collectetData["100regNeed"].probePoint.length,collectetData["0regFull"].probePoint[0].properties.maxDepth)).toLocaleString('de-DE')); //Gesamtkosten (EUR, geschätzt):
	OutPutFile.push("95"); //Kosten Bohrung Bohrmeter (EUR/m):
	OutPutFile.push(""); //Förderfähigkeit gegeben: 
	OutPutFile.push(""); //Alternativen geprüft:
	OutPutFile.push(""); //Empfohlene Lösung: 
	OutPutFile.push(RemarkString); //Bemerkungen / Hinweise:
	OutPutFile.push("Bearbeitet"); //Bearbeitungsmarker

	return OutPutFile.join('\t');
}








function enableExcelCodeOverlay(text) {
  const overlay = document.getElementById('excelCodeOverlay');
  const codeText = document.getElementById('excelCodeText');
  const copiedMsg = document.getElementById('copiedMsg');
  codeText.textContent = text || '';
  overlay.style.display = 'flex';
  copiedMsg.style.display = 'none';
}

// Versteckt den Overlay
function disableExcelCodeOverlay() {
  const overlay = document.getElementById('excelCodeOverlay');
  overlay.style.display = 'none';
}

// Kopiert den Text in die Zwischenablage, zeigt "Kopiert!" kurz an
function copyExcelCodeToClipboard() {
  const codeText = document.getElementById('excelCodeText');
  const copiedMsg = document.getElementById('copiedMsg');
  const text = codeText.textContent;
  if (!text.trim()) return;
  // Kopieren
  navigator.clipboard.writeText(text).then(() => {
    copiedMsg.style.display = 'block';
    setTimeout(() => {
      copiedMsg.style.display = 'none';
    }, 1200);
  });
}


async function clearAllForms() {
    // Clear BIM form
    const bimForm = document.getElementById('bimForm');
    if (bimForm) {
        // Clear all input fields
        bimForm.querySelectorAll('input').forEach(input => {
            if (input.type === 'checkbox' || input.type === 'radio') {
                input.checked = false;
            } else {
                input.value = '';
            }
        });

        // Clear all textareas
        bimForm.querySelectorAll('textarea').forEach(textarea => {
            textarea.value = '';
        });

        // Reset selects to their first option
        bimForm.querySelectorAll('select').forEach(select => {
            select.selectedIndex = 0;
        });
    }

	var MaxEWSDrillingDepthInputElement = document.getElementById('Form_maxBohrtiefe');
	MaxEWSDrillingDepthInputElement.value = 400;

    // Clear Internal form
    const internalForm = document.getElementById('InternalForm');
    if (internalForm) {
        internalForm.querySelectorAll('input').forEach(input => {
            if (input.type === 'checkbox' || input.type === 'radio') {
                input.checked = false;
            } else {
                input.value = '';
            }
        });

        internalForm.querySelectorAll('textarea').forEach(textarea => {
            textarea.value = '';
        });

        internalForm.querySelectorAll('select').forEach(select => {
            select.selectedIndex = 0;
        });
    }
}












function replaceDots(num) {
  return num.toString().replace(/\./g, ',');
}

function replaceSpace(name) {
	name = name.toString().replace(/\//g, '_');
	name = name.toString().replace(/-/g, '_');
	return name;
}

function containsWord(str, word) {
  return new RegExp('\\b' + word + '\\b').test(str);
}

function calculateCost(extracMwh,BheAmount,BheLength){
	HeatPumpCost = (252.8*(extracMwh/2.4))+20857;
	CostByMeter = BheAmount * BheLength * 95;
	return HeatPumpCost+CostByMeter;
}

function formatGermanStringNumber(str) {
  const num = Number(str);
  if (isNaN(num)) {
    return 'Invalid number';
  }
  return num.toLocaleString('de-DE');
}