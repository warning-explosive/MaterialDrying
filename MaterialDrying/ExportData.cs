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
        
        public decimal T_I { get; set; }
        public decimal T_I_predictor { get; set; }
        public decimal T_I_corrector { get; set; }
        
        public decimal T_II { get; set; }
        public decimal T_II_predictor { get; set; }
        public decimal T_II_corrector { get; set; }
        
        public decimal T_L { get; set; }
        public decimal T_L_star { get; set; }
        public decimal T_L_predictor { get; set; }
                
        public decimal q_I_predictor { get; set; }
        public decimal q_II_predictor { get; set; }
        
        public decimal q_I_corrector { get; set; }
        public decimal q_II_correcor { get; set; }
        
        public decimal q_L_star { get; set; }
        
        public decimal delta_X_I { get; set; }
        public decimal delta_X_II { get; set; }
    }
}