// ReSharper disable InconsistentNaming
namespace MaterialDrying
{
    using System;
    using JetBrains.Annotations;

    public class Iteration
    {
        public Iteration([NotNull] Constants c,
                         decimal t_I,
                         decimal t_I_star,
                         decimal t_II,
                         decimal t_II_star,
                         decimal t_L,
                         decimal t_L_star,
                         decimal x_p_star,
                         decimal w_p_star,
                         decimal previous_x_p)
        {
            // Температура под фронтом сублимации
            T_I = t_I;
            T_I_star = t_I_star;
            
            // Температура слоя над фронтом сублимации
            T_II = t_II;
            T_II_star = t_II_star;
            
            // Температура верхнего слоя материала
            T_L = t_L;
            T_L_star = t_L_star;
            
            // Положение фронта сублимации
            X_p = x_p_star * c.L;
            X_p_star = x_p_star;
            previous_X_p = previous_x_p;

            W_p_star = w_p_star;
            
            // Положение фронта сублимации в слоях
            var j_p_notRounded = c.N - (((c.L - X_p) * c.N) / c.L);
            j_p = (int)decimal.Round(j_p_notRounded, 0, MidpointRounding.AwayFromZero);
        }

        public decimal T_I { get; }
        public decimal T_I_star { get; }

        public decimal T_II { get; }
        public decimal T_II_star { get; }
        
        public decimal T_L { get; }
        public decimal T_L_star { get; }
        
        public decimal X_p { get; }
        public decimal X_p_star { get; }
        public decimal previous_X_p { get; }

        public decimal W_p_star { get; }

        public int j_p { get; }

        public static decimal T_I_to_star(Constants c, decimal T_I)
        {
            return (T_I - c.T_s_3) / (c.T_s_eq - c.T_s_3);
        }

        public static decimal T_I_to_dimensional(Constants c, decimal T_I_star)
        {
            return (T_I_star * (c.T_s_eq - c.T_s_3)) + c.T_s_3;
        }

        public static decimal T_II_to_star(Constants c, decimal T_II)
        {
            return (T_II - c.T_II_ref) / (c.T_s_eq - c.T_II_ref);
        }

        public static decimal T_II_to_dimensional(Constants c, decimal T_II_star)
        {
            return (T_II_star * (c.T_s_eq - c.T_II_ref)) + c.T_II_ref;
        }

        public static decimal T_L_dimensional(Constants c, decimal T_L)
        {
            return ((c.T_infinitive * c.delta_h_s) - (c.K_e_II * T_L)) / (c.delta_h_s - c.K_e_II);
        }
    }
}