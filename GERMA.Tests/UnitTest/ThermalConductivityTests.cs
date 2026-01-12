using Xunit;
using GERMAG.Server.GeometryCalculations;

namespace GERMA.Tests;

public class ThermalConductivityTest
{
    [Fact]
    public void ReturnsMeasuredConductivityIfDepthIsBelowTheThreshold()
    {
        //Arrange
        var Conductivity = new ThermalConductivity();
        double? GivenConductivity = 2.5;
        double? depth = 50;

        //Act
        var result = Conductivity.GetThermalConductivity(GivenConductivity, depth);

        //Assert
        Assert.Equal(GivenConductivity, result);

    }
    [Fact]
    public void UsesBlendingFormula_IfDepthAboveThreshold()
    {
        // Arrange
        var conductivity = new ThermalConductivity();
        double? measured = 2.5;
        double? depth = 150; // Above 100

        // Act
        var result = conductivity.GetThermalConductivity(measured, depth);

        // Assert
        Assert.InRange(result!.Value, 2.4, 2.5);
    }
    [Fact]
    public void NullInput()
    {
        // Arrange
        var conductivity = new ThermalConductivity();
        double? measured = null;
        double? depth = null; // Above 100

        // Act
        var result = conductivity.GetThermalConductivity(measured, depth);

        // Assert
        Assert.Null(result);
    }
}