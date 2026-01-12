using GERMAG.Server.GeometryCalculations;
using GERMAG.Server.ReportCreation;
using GERMAG.Shared;
using GERMAG.Shared.PointProperties;

public interface ICoreReport
{
    Task<IEnumerable<Report>> MainReportCalculation(LandParcel landParcelElement, RequestContext request_context, bool UseLandParcelBool);
}

public class CoreReport(ICreateReportAsync createReport, IRestrictionFromLandParcel restrictionFromLandParcel, IGeoThermalProbesCalcualtion geoThermalProbesCalcualtion, ICrossInfluence crossInfluence, IFilterBheSubGroups filterBheSubGroups, IEnergyDemand energyDemand, IGetTemplateBHE getTemplateBHE, IGetTemplateBasedProbeSpecificData getTemplateBasedProbeSpecificData) : ICoreReport
{
    public async Task<IEnumerable<Report>> MainReportCalculation(LandParcel landParcelElement, RequestContext request_context, bool UseLandParcelBool)
    {
        if (landParcelElement.Error == true)
        {
            return new[] { new Report
        {
            Error = "Error 512: Bisher wird nur das Gebiet Berlin von GERMA unterstützt.\nFür Gebiete in Brandenburg kontaktieren sie die O-GT-T!"
        }};
        }

        //EnergyDemand

        EnergyMapDemandData? EnergyMapEnergydemand = null;

        if (request_context.EnergyDemand != 0 && request_context.EnergyDemand != null)
        {
            EnergyMapEnergydemand = new EnergyMapDemandData
            {
                ActInsolation = request_context.EnergyDemand,
                BetterInsolation = 0,
                OptimalInsolation = 0,
            };
        }
        else
        {
            EnergyMapEnergydemand = await energyDemand.GetEneryDemand(landParcelElement);
        }

        //Default demand:

        if (EnergyMapEnergydemand.ActInsolation <= 0) {
            EnergyMapEnergydemand.ActInsolation = (int)Math.Round(landParcelElement.Geometry!.Area * 40);
        }

        //Calculate usable and restricted areas

        Restricion RestrictionFile = await restrictionFromLandParcel.CalculateRestrictions(landParcelElement, request_context);
        if (RestrictionFile.Usable_Area == null) { return new[] { new Report { Error = "Error 513: Nicht genug Platz vorhanden um EWS zu platzieren" } }; }

        IEnumerable<Report> polygonBasedReport = await createReport.CreateGeothermalReportAsync(landParcelElement, RestrictionFile, UseLandParcelBool);

        //Overwritevalues

        //Math.Min(Int32.Parse(FinalReport[0].TotalMaxDepth), request_context.MaximalDrillingDepth)

        var FinalReport = polygonBasedReport.ToList();
        FinalReport[0].Geometry_Usable = RestrictionFile.Geometry_Usable_geoJson;
        FinalReport[0].Geometry_Restiction = RestrictionFile.Geometry_Restiction_geoJson;
        FinalReport[0].Usable_Area = RestrictionFile.Usable_Area;
        FinalReport[0].Restiction_Area = RestrictionFile.Restiction_Area;
        FinalReport[0].TotalMaxDepth = ((int)Math.Min(double.Parse(FinalReport[0].TotalMaxDepth!), (double)request_context.MaximalDrillingDepth!)).ToString();
        FinalReport[0].Land_parcel_number = landParcelElement.LandParcels;

        List<ProbePoint?> FullPointProbe;

        TemplateBHE? TempalteBHE = await getTemplateBHE.CreateTemplateBHE(landParcelElement, request_context, EnergyMapEnergydemand);

        //Calcualte indivual BHEs

        try
        {
            FullPointProbe = await geoThermalProbesCalcualtion.CalculateGeoThermalProbes(RestrictionFile, request_context, TempalteBHE);
        }
        catch (Exception e)
        {
            FinalReport[0].Error = e.Message;
            return FinalReport;
        }

        //FilterBheSubGroups
        FullPointProbe = await filterBheSubGroups.FilterSubGroups(FullPointProbe, request_context);

        //Give BHE values 

        // var old_FullPointProbe = await getProbeSpecificData.GetPointProbeData(landParcelElement, FullPointProbe, request_context);

        FullPointProbe = getTemplateBasedProbeSpecificData.GetPointProbeDataAfterTemplate(landParcelElement, FullPointProbe, request_context, TempalteBHE ?? throw new Exception("CE: Templeate BHE not given"));

        if (FullPointProbe.Count == 0) { return new[] { new Report { Error = "Error 513: EWS konnten nicht plaziert werden" } }; }
        if (FullPointProbe[0] == null) { return new[] { new Report { Error = "Error 513: EWS konnten nicht plaziert werden" } }; }

        List<ProbePoint?> TruncatedPointProbe = new();

        foreach (var probePoint in FullPointProbe)
        {
            if (probePoint != null)
            {
                TruncatedPointProbe.Add(new ProbePoint
                {
                    GeometryJson = probePoint.GeometryJson,
                    Properties = probePoint.Properties
                });
            }
            else { TruncatedPointProbe.Add(null); }
        }

        FinalReport[0].ProbePoint = TruncatedPointProbe;

        FinalReport[0].Crossinfluence_Factor = await crossInfluence.GetCrossInfluenceFactor(FinalReport[0].ProbePoint?.Count);
        FinalReport[0].Extraction_KW_2400_Full_Load_Hours = FinalReport[0]?.ProbePoint?.Select(pp => pp?.Properties?.Extraction_KW_2400_Full_Load_Hours).Sum() ?? 0; //await crossInfluence.GetCrossInfluence(TruncatedPointProbe);     
        FinalReport[0].Extraction_KW = FinalReport[0]?.ProbePoint?.Select(pp => pp?.Properties?.Extraction_KW).Sum() ?? 0;

        FinalReport[0].Limited_100m_Extraction_KW_2400_Full_Load_Hours = FinalReport[0]?.ProbePoint?.Select(pp => pp?.Properties?.Limited_100m_Extraction_KW_2400_Full_Load_Hours).Sum() ?? 0;
        FinalReport[0].Limited_100m_Extraction_KW = FinalReport[0]?.ProbePoint?.Select(pp => pp?.Properties?.Limited_100m_Extraction_KW).Sum() ?? 0;

        FinalReport[0].ActInsolation = EnergyMapEnergydemand.ActInsolation;
        FinalReport[0].ActInsolation_Coverage_100 = Math.Round((double)((FinalReport[0].Limited_100m_Extraction_KW_2400_Full_Load_Hours! / ((request_context.Cop ?? OfficalParameters.FixCOP) - 1)) + FinalReport[0].Limited_100m_Extraction_KW_2400_Full_Load_Hours!) / (double)EnergyMapEnergydemand.ActInsolation! * 100, 2);
        FinalReport[0].ActInsolation_Coverage_Maxdepth = Math.Round((double)((FinalReport[0].Extraction_KW_2400_Full_Load_Hours! / ((request_context.Cop ?? OfficalParameters.FixCOP) - 1)) + FinalReport[0].Extraction_KW_2400_Full_Load_Hours!) / (double)EnergyMapEnergydemand.ActInsolation! * 100, 2);

        FinalReport[0].FlowIsTurbolent = request_context.FlowIsTurbolent;
        FinalReport[0].Regeneration = request_context.Regeneration.ToString();
        FinalReport[0].COP = request_context.Cop ?? OfficalParameters.FixCOP;

        //FinalReport[0].OptimalInsolation = EnergyMapEnergydemand.ActInsolation;
        //FinalReport[0].OptimalInsolation_Coverage_100 = Math.Min(Math.Round((double)FinalReport[0].Limited_100m_Extraction_KW_Without_regeneration_2400_Full_Load_Hours! / (double)EnergyMapEnergydemand.OptimalInsolation! *100, 2) , 100);
        //FinalReport[0].OptimalInsolation_Coverage_Maxdepth = Math.Min(Math.Round((double)FinalReport[0].Extraction_KW_Without_regeneration_2400_Full_Load_Hours! / (double)EnergyMapEnergydemand.OptimalInsolation! *100, 2) , 100);

        return FinalReport;
    }
}