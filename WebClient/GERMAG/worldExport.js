const { Document, Packer, Paragraph, TextRun, ImageRun, Table, TableRow, TableCell, WidthType, LineBreak, AlignmentType } = window.docx;


async function fetchImageBuffer(url) {
    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error("Image fetch failed");
        const buffer = await response.arrayBuffer();
        return buffer;
    } catch (err) {
        console.error("Image fetch error:", err);
        return null;
    }
}

async function buildWord() {

    showLoadingOverlay("Generating Word document...");
    updateLoadingProgress(1, 2);
    zoomToGeoJsonList();
    //await waitForVisibleWmsLayersToLoad();

    console.log("Sleep");

    const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

    
    await sleep(4000);

    updateLoadingProgress(2, 2);

    const imageBuffer = await fetchImageBuffer("pic/Logo.jpg");

    console.log("Creating World");

    hideLeafletControls();
    await new Promise(res => setTimeout(res, 1));

    const { buffer, imageWidth, imageHeight } = await captureLeafletMap();

    //downloadImage(buffer) //here

    showLeafletControls();

    const aspectRatio = imageHeight / imageWidth;

    const today = new Date();
    const formattedDate = today.toLocaleDateString('de-DE');

    const pageWidthPoints = 600;

    const mapImage = new ImageRun({
        data: buffer,
        transformation: {
            width: pageWidthPoints,
            height: Math.round(pageWidthPoints * aspectRatio),
        },
    });

    const mapParagraph = new Paragraph({
        children: [mapImage],
        spacing: {
            before: 300,
        },
    });


    // Title TextRun
    const titleText = new TextRun({
        text: "Germa - Automatische Grundlagenermittlung",
        bold: true,
        size: 48, // 24pt (half-points)
        font: "Arial",
    });

    const titleParagraph = new Paragraph({
        children: [titleText],
    });

    // ImageRun if image is available
    const image = imageBuffer
        ? new ImageRun({
            data: imageBuffer,
            transformation: {
                width: 150,
                height: 150,
            },
        })
        : new TextRun("Image failed to load.");


    //ParagraphList
    const children = [];

    // Table for layout: title left, image right
    const layoutTable = new Table({
        width: {
            size: 100,
            type: WidthType.PERCENTAGE,
        },
        borders: {
            top: { style: "none", size: 0, color: "FFFFFF" },
            bottom: { style: "none", size: 0, color: "FFFFFF" },
            left: { style: "none", size: 0, color: "FFFFFF" },
            right: { style: "none", size: 0, color: "FFFFFF" },
            insideHorizontal: { style: "none", size: 0, color: "FFFFFF" },
            insideVertical: { style: "none", size: 0, color: "FFFFFF" },
        },
        rows: [
            new TableRow({
                children: [
                    new TableCell({
                        children: [titleParagraph],
                        verticalAlign: "center", // 🔥 center title in cell
                        width: { size: 70, type: WidthType.PERCENTAGE },
                        borders: { top: { style: "none" }, bottom: { style: "none" }, left: { style: "none" }, right: { style: "none" } },
                    }),
                    new TableCell({
                        children: [new Paragraph({ children: [image] })],
                        width: { size: 30, type: WidthType.PERCENTAGE },
                        borders: { top: { style: "none" }, bottom: { style: "none" }, left: { style: "none" }, right: { style: "none" } },
                    }),
                ],
            }),
        ],
    });
    children.push(layoutTable);

    // Below the title+image: Example paragraph
    const IntroductionParagraph = new Paragraph({
        spacing: { before: 400 }, // 20pt space before (this adds the vertical space to the next paragraph)
        children: [
            new TextRun({
                text: "Für den folgenden Grundlagenbericht zur Nutzung von Erdwärmesonden am ausgewählten Standort wird keine Gewähr auf Richtigkeit und/oder Vollständigkeit genommen. ",
                color: "FF0000", // Red color (hex code)
            }),
            new TextRun({ break: 1 }), // line break
            new TextRun({
                text: "Die Nutzung und/oder Weitergabe der Angaben und Daten erfolgt auf eigenes Risiko.",
                color: "FF0000", // Red color (hex code)
            }),
            new TextRun({ break: 1 }), // line break
            new TextRun({
                text: "Grundsätzlich ist für geothermische Kundenanfragen stets die Fachplanung O-GT-T zuständig und durch den Vertrieb einzubeziehen.",
                color: "FF0000", // Red color (hex code)
            }),
        ],
    });
    children.push(IntroductionParagraph);

    const locationImageParagraph = new Paragraph({
        spacing: { before: 200 }, // Adds a bit of space before this paragraph
        children: [
            new TextRun({
                text: "Lagebild zur Einordnung: ",
                bold: true,
            }),
        ],
    });
    children.push(locationImageParagraph);
    children.push(mapParagraph);












    const geoHeader = [
        new Paragraph({
            alignment: "center",
            spacing: { after: 300 },
            children: [
                new ImageRun({
                    data: imageBuffer,
                    transformation: { width: 150, height: 150 },
                }),
            ],
        }),
        new Paragraph({
            alignment: "center",
            spacing: { after: 600 },
            children: [
                new TextRun({
                    text: "MACHBARKEITSPRÜFUNG GEOTHERMIE",
                    bold: true,
                    size: 48,
                }),
            ],
        }),
    ];


    const infoTable = new Table({
        width: {
            size: 100,
            type: WidthType.PERCENTAGE,
        },
        borders: {
            top: { style: "none", size: 0, color: "FFFFFF" },
            bottom: { style: "none", size: 0, color: "FFFFFF" },
            left: { style: "none", size: 0, color: "FFFFFF" },
            right: { style: "none", size: 0, color: "FFFFFF" },
            insideHorizontal: { style: "none", size: 0, color: "FFFFFF" },
            insideVertical: { style: "none", size: 0, color: "FFFFFF" },
        },
        rows: [
            // PROJECT
            new TableRow({
                children: [
                    new TableCell({
                        width: { size: 33, type: WidthType.PERCENTAGE },
                        children: [new Paragraph({
                            spacing: {
                                before: 400,
                                after: 400,
                            },
                            children: [new TextRun({ text: "Projekt", bold: true, size: 28 })],
                        })],
                    }),
                    new TableCell({
                        width: { size: 67, type: WidthType.PERCENTAGE },
                        children: [new Paragraph({
                            spacing: {
                                before: 400,
                                after: 400,
                            },
                            children: [new TextRun({ text: "xxx", size: 28 })],
                        })],
                    }),
                ],
            }),
            // ADDRESS
            new TableRow({
                children: [
                    new TableCell({
                        width: { size: 33, type: WidthType.PERCENTAGE },
                        children: [new Paragraph({
                            spacing: {
                                before: 400,
                                after: 400,
                            },
                            children: [new TextRun({ text: "Projektadresse", bold: true, size: 28 })],
                        })],
                    }),
                    new TableCell({
                        width: { size: 67, type: WidthType.PERCENTAGE },
                        children: [new Paragraph({
                            spacing: {
                                before: 400,
                                after: 400,
                            },
                            children: [new TextRun({ text: "xxx", size: 28 })],
                        })],
                    }),
                ],
            }),
            // CONTENT
            new TableRow({
                children: [
                    new TableCell({
                        width: { size: 33, type: WidthType.PERCENTAGE },
                        children: [new Paragraph({
                            spacing: {
                                before: 400,
                                after: 400,
                            },
                            children: [new TextRun({ text: "Berichtsinhalt", bold: true, size: 28 })],
                        })],
                    }),
                    new TableCell({
                        width: { size: 67, type: WidthType.PERCENTAGE },
                        children: [new Paragraph({
                            spacing: {
                                before: 400,
                                after: 400,
                            },
                            children: [new TextRun({ text: "Machbarkeitsstudie Geothermie", size: 28 })],
                        })],
                    }),
                ],
            }),
            // CONTACT
            new TableRow({
                children: [
                    new TableCell({
                        width: { size: 33, type: WidthType.PERCENTAGE },
                        children: [new Paragraph({
                            spacing: {
                                before: 400,
                                after: 400,
                            },
                            children: [new TextRun({ text: "Bearbeitung", bold: true, size: 28 })],
                        })],
                    }),
                    new TableCell({
                        width: { size: 67, type: WidthType.PERCENTAGE },
                        children: [new Paragraph({
                            spacing: {
                                before: 400,
                                after: 400,
                            },
                            children: [
                                new TextRun({
                                    text:
                                        "XXX",
                                    size: 28,
                                }),
                            ],
                        })],
                    }),
                ],
            }),
            // DATE
            new TableRow({
                children: [
                    new TableCell({
                        width: { size: 33, type: WidthType.PERCENTAGE },
                        children: [new Paragraph({
                            children: [new TextRun({ text: "Erstellt am", bold: true, size: 28 })],
                        })],
                    }),
                    new TableCell({
                        width: { size: 67, type: WidthType.PERCENTAGE },
                        children: [new Paragraph({
                            children: [new TextRun({ text: formattedDate, size: 28 })],
                        })],
                    }),
                ],
            }),
        ],
    });


    children.push(...geoHeader);
    children.push(infoTable);










    const SummaryTitel = new Paragraph({
        children: [
            new TextRun({
                text: "Zusammenfassung",
                bold: true,
                size: 28,
            }),
        ],
        spacing: {
            after: 300,
        },
        pageBreakBefore: true,
    });
    children.push(SummaryTitel);


    const Summary = new Paragraph({
        children: [
            new TextRun({
                text: "Für das Vorhaben DEKAB-Fahrplan des Projektstandortes Granitzstr. 42, 13189 Berlin wurde die Machbarkeitsstudie zur geothermischen Nutzung des oberflächennahen Untergrunds durchgeführt. In dieser Untersuchung wurden sowohl Brunnendubletten mit Grundwasser-Wärmepumpe als offenes Geothermiesystem als auch Erdwärmesonden mit Sole-Wasser-Wärmepumpe als geschlossenes Geothermiesystem betrachtet.",
                size: 28,
            }),
            new TextRun({ break: 2 }),
            new TextRun({ text: "xxx", bold: true }),
        ],
        spacing: {
            before: 400,
            after: 400,
        },
        alignment: AlignmentType.JUSTIFIED,
    })
    children.push(SummaryTitel);


    const TitleParagraph1 = new Paragraph({
        children: [
            new TextRun({
                text: "Angaben zur betrachteten Fläche:",
                bold: true,
            }),
        ],
        spacing: { after: 200, before: 200 },
    });
    children.push(TitleParagraph1);
    

    const ParameterTable1 = new Table({
        rows: [
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Gemeinde: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: LastReportData.land_parcels_gemeinde, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Flurstück Nr: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: LastReportData.land_parcel_number, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Wärmebedarf (MWh/a): ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${(Math.round(LastReportData.actInsolation / 10) / 100).toFixed(2).replace('.', ',')} MWh`, bold: true })] })],
                    }),
                ],
            }),
        ],
        alignment: AlignmentType.LEFT, // or CENTER if you want it centered
    });
    children.push(ParameterTable1);
        


    var RegenerationName = null;
    var Strömungsname = null;

    if(LastReportData.flowIsTurbolent == false){Strömungsname = "Laminar";}
    if(LastReportData.flowIsTurbolent == true){Strömungsname = "Turbulent";}

    if(LastReportData.regeneration == "None"){RegenerationName = "Keine Regeneration";}
    if(LastReportData.regeneration == "Half"){RegenerationName = "50% Regeneration";}
    if(LastReportData.regeneration == "Full"){RegenerationName = "Volle Regeneration";}

    var Holsteinname = `Keine Begrenzung`;
    if(LastReportData.holstein != ""){
        Holsteinname = LastReportData.holstein + "m";
    }


    const TitleParagraph2 = new Paragraph({
        children: [
            new TextRun({
                text: "EWS-Parameter:",
                bold: true,
            }),
        ],
        spacing: { after: 200, before: 200 },
    });
    children.push(TitleParagraph2);

    const ParameterTable2 = new Table({
        rows: [
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Regeneration: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: RegenerationName, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Strömungsart: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: Strömungsname, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Holstein Tiefe: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${Holsteinname}`, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Rupelton-Teufe Tiefe: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${LastReportData.rupelton}m`, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Maximale nutzbare Bohrtiefe (Simulationsbasis): ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${LastReportData.totalMaxDepth}m`, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Nutzbare Fläche: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${Math.round(LastReportData.usable_Area * 100) / 100}m\u00B2`, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph(`EWS-Anzahl (${LastReportData.totalMaxDepth}m): `)],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${LastReportData.probePoint.length.toString().replace('.', ',')}`, bold: true })] })],
                    }),
                ],
            }),
        ],
        alignment: AlignmentType.LEFT, // or CENTER if you want it centered
    });
    children.push(ParameterTable2);

    let TitleParagraph3 = new Paragraph({
        children: [
            new TextRun({
                text: "Entzug bei 100m Bohrtiefe:",
                bold: true,
            }),
        ],
        spacing: { after: 200, before: 200 },
    });

    let ParameterTable3 = new Table({
        rows: [
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Entzugsleistung: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${Math.round(LastReportData.limited_100m_Extraction_KW * 10) / 10} KW`, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Jahresentzugsarbeit (2400 Volllaststunden): ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${Math.round(LastReportData.limited_100m_Extraction_KW_2400_Full_Load_Hours / 10) / 100} MWh`, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Wärmeversorgung (COP3): ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${LastReportData.actInsolation_Coverage_100}%`, bold: true })] })],
                    }),
                ],
            }),
        ],
        alignment: AlignmentType.LEFT, // or CENTER if you want it centered
    });

    children.push(TitleParagraph3);
    children.push(ParameterTable3);

    if(parseFloat(LastReportData.totalMaxDepth) <100){
        TitleParagraph3 = [];
        ParameterTable3 = [];
    }

    let TitleParagraph4 = new Paragraph({
        children: [
            new TextRun({
                text: `Entzug bei ${LastReportData.totalMaxDepth}m Bohrtiefe:`,
                bold: true,
            }),
        ],
        spacing: { after: 200, before: 200 },
    });

    let ParameterTable4 = new Table({
        rows: [
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Entzugsleistung: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${Math.round(LastReportData.extraction_KW * 10) / 10} KW`, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Jahresentzugsarbeit (2400 Volllaststunden): ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${Math.round(LastReportData.extraction_KW_2400_Full_Load_Hours / 10) / 100} MWh`, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Wärmeversorgung (COP3): ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${LastReportData.actInsolation_Coverage_Maxdepth}%`, bold: true })] })],
                    }),
                ],
            }),
        ],
        alignment: AlignmentType.LEFT, // or CENTER if you want it centered
    });

    children.push(TitleParagraph4);
    children.push(ParameterTable4);






    const TitleParagraph5 = new Paragraph({
        children: [
            new TextRun({
                text: `Wärmeleitfähigkeit: `,
                bold: true,
            }),
        ],
        spacing: { after: 200, before: 200 },
    });

    const ParameterTable5 = new Table({
        rows: [
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Wärmeleitfähigkeit 40m: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${LastReportData.thermal_con_40} W/mK`, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Wärmeleitfähigkeit 60m: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${LastReportData.thermal_con_60} W/mK`, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Wärmeleitfähigkeit 80m: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${LastReportData.thermal_con_80} W/mK`, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Wärmeleitfähigkeit 100m: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${LastReportData.thermal_con_100} W/mK`, bold: true })] })],
                    }),
                ],
            }),
        ],
        alignment: AlignmentType.LEFT, // or CENTER if you want it centered
    });

    children.push(TitleParagraph5);
    children.push(ParameterTable5);




    const TitleParagraph6 = new Paragraph({
        children: [
            new TextRun({
                text: `Grundwassertemperatur: `,
                bold: true,
            }),
        ],
        spacing: { after: 200, before: 200 },
    });

    const ParameterTable6 = new Table({
        rows: [
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Grundwassertemperatur 20m: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${LastReportData.mean_water_temp_20}°C`, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Grundwassertemperatur 40m: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${LastReportData.mean_water_temp_40}°C`, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Grundwassertemperatur 60m: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${LastReportData.mean_water_temp_60}°C`, bold: true })] })],
                    }),
                ],
            }),
            new TableRow({
                children: [
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph("Grundwassertemperatur 100m: ")],
                    }),
                    new TableCell({
                        margins: { top: 283, bottom: 283, left: 283, right: 283 },
                        children: [new Paragraph({ children: [new TextRun({ text: `${LastReportData.mean_water_temp_20to100}°C`, bold: true })] })],
                    }),
                ],
            }),
        ],
        alignment: AlignmentType.LEFT, // or CENTER if you want it centered
    });

    children.push(TitleParagraph6);
    children.push(ParameterTable6);
    


    //LastReportData.holstein
    /*    holstein = ``;
    if(reportData.holstein != ""){
        holstein = `<p><strong>Holstein-Schicht:</strong> ${reportData.holstein} meter</p>`
    }
        */



    //Not Fix Paragraphs

    if (LastReportData.holstein && LastReportData.holstein.trim() !== "") {
        const holsteinParagraph = new Paragraph({
            spacing: { before: 200 },
            children: [
                new TextRun({ text: `Restriktion durch Grundwasser Versalzung entdeck:`, bold: true, color: "FF0000" }),
                new TextRun({ break: 1 }),
                new TextRun({
                    text: `Die maximale Bohrtiefe kann nicht erreicht werden! Erdwärmenutzung ist nur mit Einschränkungen erlaubt in Bereichen mit erhöhten Salzkonzentrationen im Grundwasser unterhalb der Holstein-Schichten (Grundwasserleiter 3 und 4).
Die Holsteinsicht ist auf den gewählten Arial bei einer Tiefe von `}),
                new TextRun({ text: `${LastReportData.holstein} `, bold: true }),
                new TextRun({ text: `Metern zu finden.` }),
            ]
        });
        children.push(holsteinParagraph);
    }


    // Build the doc
    const doc = new Document({
        styles: {
            default: {
                document: {
                    run: {
                        font: "Arial",
                        size: 20, // 14pt
                    },
                },
            },
        },
        sections: [
            {
                children: [
                    layoutTable,
                    IntroductionParagraph,
                    locationImageParagraph,
                    mapParagraph,
                ],
            },
            // This will force a new page
            {
                children: [
                    ...geoHeader,
                    infoTable,
                    SummaryTitel,
                    Summary
                    // ... add other paragraphs you want on the next page here
                ],
            },
            {
                children: [
                    TitleParagraph1,
                    ParameterTable1,
                    TitleParagraph2,
                    ParameterTable2,
                    TitleParagraph3,
                    ParameterTable3,
                    TitleParagraph4,
                    ParameterTable4,
                    TitleParagraph5,
                    ParameterTable5,
                    TitleParagraph6,
                    ParameterTable6
                    // ... add other paragraphs you want on the next page here
                ],
            },
        ],
    });

    hideLoadingOverlay();

    return doc;
}

async function exportWorld() {
    if (LastReportData == null) {
        alert("No Report avaible");
        return;
    }
    const doc = await buildWord();

    Packer.toBlob(doc).then((blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = "GERMA_Report.docx";
        a.click();
        URL.revokeObjectURL(url);
    });
}



//Devloper Fucntions

function downloadImage(buffer, filename = "map.png") {
  const blob = new Blob([buffer], { type: "image/png" }); // or "image/jpeg"
  const link = document.createElement("a");
  link.href = URL.createObjectURL(blob);
  link.download = filename;
  link.click();
  URL.revokeObjectURL(link.href);
}