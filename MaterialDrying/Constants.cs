// ReSharper disable InconsistentNaming
namespace MaterialDrying
{
    public class Constants
    {
        /// <summary>
        /// T общ (в секундах) - ограничение расчета по времени
        /// </summary>
        public int DryingTime { get; set; }

        /// <summary>
        /// Высота стакана (толщина материала)
        /// </summary>
        public decimal L { get; set; }
        
        /// <summary>
        /// Кол-во слоев (целое)
        /// </summary>
        public decimal N { get; set; }
        
        /// <summary>
        /// Стартовое положение рабочей точки
        /// </summary>
        public decimal X_p_start { get; set; }

        /// <summary>
        /// Размерная ширина слоя материала
        /// </summary>
        public decimal delta_x => L / N;

        /// <summary>
        /// Безразмерная ширина слоя материала
        /// </summary>
        public decimal delta_x_star => delta_x / L;

        /// <summary>
        /// Размерный шаг по времени
        /// </summary>
        public decimal delta_t => (2m * (delta_t_star * L * L)) / (A_e_I + A_e_II);

        /// <summary>
        /// Безразмерный шаг по времени
        /// </summary>
        public decimal delta_t_star => (delta_x_star * delta_x_star) / 2m;

        #region physical measures
        
        public decimal E { get; set; }

        public decimal A_e_I { get; set; }
        public decimal A_e_II { get; set; }
        
        public decimal K_e_I { get; set; }
        public decimal K_e_II { get; set; }
        
        public decimal Mu_1_I { get; set; }
        public decimal Mu_1_II { get; set; }
        
        public decimal Mu_2_I { get; set; }
        public decimal Mu_2_II { get; set; }
        
        public decimal C_p_w { get; set; }
        
        public decimal Ro_vi_II { get; set; }
        public decimal Ro_e_I { get; set; }
        
        public decimal C_p_e_I { get; set; }
        public decimal C_p_e_II { get; set; }
        
        public decimal T_infinitive { get; set; }
        public decimal T_s_3 { get; set; }
        public decimal T_s_eq { get; set; }
        public decimal T_II_ref { get; set; }
        
        public decimal S_te_nu_I => (Ro_e_I * C_p_e_I * (T_s_eq - T_s_3)) / (Ro_vi_II * delta_h_s * (W_p - W_eq));
        public decimal S_te_nu_II => (C_p_e_II * (T_s_eq - T_II_ref)) / (delta_h_s * (W_p - W_eq));
        
        public decimal W_eq { get; set; }
        public decimal W_p { get; set; }
        public decimal W_p_star { get; set; }
        
        public decimal delta_h_s { get; set; }

        #endregion
    }
}