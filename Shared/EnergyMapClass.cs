 public class EnergyMapProperties
    {
        public string? Fid { get; set; }
        public string? Uuid { get; set; }
        public string? Geom { get; set; }
        public string? Geom25833 { get; set; }
        public int? Gfk { get; set; }
        public string? Bezgfk { get; set; }
        public int? Aog { get; set; }
        public object? Aug { get; set; }
        public object? Bat { get; set; }
        public object? Bezbat { get; set; }
        public object? Name { get; set; }
        public int? Baw { get; set; }
        public string? Bezbaw { get; set; }
        public string? Street_id { get; set; }
        public string? Street_name { get; set; }
        public string? House_number { get; set; }
        public string? Description { get; set; }
        public string? Postcode { get; set; }
        public string? Block_id { get; set; }
        public string? Plot_id { get; set; }
        public object? Heritage_id { get; set; }
        public object? Heritage_type { get; set; }
        public object? Height { get; set; }
        public string? Buildingtype_emb { get; set; }
        public string? Ground_area { get; set; }
        public int? Year_of_construction { get; set; }
        public string? Nrf_area { get; set; }
        public string? Spec_cons_c1r1 { get; set; }
        public string? Cons_c1r1 { get; set; }
        public string? Eeclass_c1r1 { get; set; }
        public string? Spec_cons_c1r2 { get; set; }
        public string? Cons_c1r2 { get; set; }
        public string? Eeclass_c1r2 { get; set; }
        public string? Spec_cons_c1r3 { get; set; }
        public string? Cons_c1r3 { get; set; }
        public string? Eeclass_c1r3 { get; set; }
        public string? Spec_cons_c2r1 { get; set; }
        public string? Cons_c2r1 { get; set; }
        public string? Eeclass_c2r1 { get; set; }
        public string? Spec_cons_c2r2 { get; set; }
        public string? Cons_c2r2 { get; set; }
        public string? Eeclass_c2r2 { get; set; }
        public string? Spec_cons_c2r3 { get; set; }
        public string? Cons_c2r3 { get; set; }
        public string? Eeclass_c2r3 { get; set; }
        public string? Spec_cons_c3r1 { get; set; }
        public string? Cons_c3r1 { get; set; }
        public string? Eeclass_c3r1 { get; set; }
        public string? Spec_cons_c3r2 { get; set; }
        public string? Cons_c3r2 { get; set; }
        public string? Eeclass_c3r2 { get; set; }
        public string? Spec_cons_c3r3 { get; set; }
        public string? Cons_c3r3 { get; set; }
        public string? Eeclass_c3r3 { get; set; }
        public string? Last_updated { get; set; }
    }

        public class EnergyMapRoot
    {
        public List<EnergyMapProperties?>? EnergyMapProperties { get; set; }
    }