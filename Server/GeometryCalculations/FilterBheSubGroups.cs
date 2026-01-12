using GERMAG.Shared;
using GERMAG.Shared.PointProperties;

public interface IFilterBheSubGroups
{
    Task<List<ProbePoint?>> FilterSubGroups(List<ProbePoint?> probepoint_list, RequestContext request_context);
}

public class FilterBheSubGroups : IFilterBheSubGroups
{

    public async Task<List<ProbePoint?>> FilterSubGroups(List<ProbePoint?> probepoint_list, RequestContext request_context)
    {
        return await Task.Run(() =>
        {
            List<int?> FieldNumberList = probepoint_list.Select(ppl => ppl?.Properties?.SubAreaFieldNumber).ToList();

            /*
            var LargeSubgroupsNumbers = FieldNumberList.Where(nl => nl != null).Where(nl => nl.HasValue).GroupBy(nl => nl!.Value).Where(gn => gn.Count() >= request_context.MinimalBhePerInduvidualArea).Select(gn => gn.Key).ToList();
                        
            var FilterdFieldNumberList = probepoint_list.Where(ppl => ppl != null &&
            ppl.Properties?.SubAreaFieldNumber != null
            && LargeSubgroupsNumbers.Contains(ppl.Properties.SubAreaFieldNumber.Value)).ToList();*/

            var FilterdFieldNumberList = probepoint_list.Where(ppl => ppl != null &&
            ppl.Properties?.SubAreaFieldNumber != null).ToList();

            return FilterdFieldNumberList;
        });
    }
}