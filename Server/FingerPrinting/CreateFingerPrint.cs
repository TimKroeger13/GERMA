using GERMAG.DataModel.Database;
using NetTopologySuite.Geometries;

public interface ICreateFingerPrint
{
    Task CreateUniqueFingerPrint(string userid, FingerPrintTypes fpt, NetTopologySuite.Geometries.Geometry? UserPolygon);
}

public class CreateFingerPrint(DataContext context, IUserIdvalidation userIdvalidation) : ICreateFingerPrint
{
    public async Task CreateUniqueFingerPrint(string userid, FingerPrintTypes fpt, NetTopologySuite.Geometries.Geometry? UserPolygon)
    {
        var CurrentTime = DateTime.Now;

        if (!userIdvalidation.IsValidUserId(userid))
        {
            return;
        }

        context.FingerPrints.Add(new FingerPrint
        {
            Userid = userid,
            Time = CurrentTime,
            FingerPrintTypes = fpt,
            Geom = UserPolygon
        });

        await context.SaveChangesAsync();
    }
}