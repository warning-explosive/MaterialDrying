// ReSharper disable InconsistentNaming
namespace MaterialDrying
{
    public class ExportData
    {
        public uint Step { get; set; }

        public int j_p { get; set; }
        public decimal X_p { get; set; }
        public decimal X_p_star { get; set; }
        public decimal W_p_star { get; set; }
        
        public decimal delta_X_I { get; set; }
        public decimal delta_X_II { get; set; }
        public decimal abs_delta_sum { get; set; }
        
        public decimal q_p_corrector { get; set; }
        
        public decimal T_I_avg { get; set; }
        public decimal T_I_star_avg { get; set; }
        
        public decimal T_II_avg { get; set; }
        public decimal T_II_star_avg { get; set; }
    }
}