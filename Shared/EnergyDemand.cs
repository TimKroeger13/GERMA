using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public class EnergyMapDemandData
{
    public int? ActInsolation { get; set; } // cons_c1r1 //Prognostizierter Heizwärmeverbrauch; Heutiges Klima, witterungsbereinigt; Aktueller Sanierungszustand
    public int? BetterInsolation { get; set; } //cons_c1r2 //Prognostizierter Heizwärmeverbrauch; Heutiges Klima, witterungsbereinigt; herkömmlich energetisch saniert
    public int? OptimalInsolation { get; set; } //cons_c1r3 //Prognostizierter Heizwärmeverbrauch; Heutiges Klima, witterungsbereinigt; optimal energetisch saniert

}