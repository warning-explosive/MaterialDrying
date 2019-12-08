// ReSharper disable InconsistentNaming
namespace MaterialDrying
{
    using System;
    using System.Linq;
    using JetBrains.Annotations;

    public class Iteration
    {
        public Iteration([NotNull] Constants c,
                         decimal t_I_avg,
                         decimal t_II_avg,
                         decimal x_p_star,
                         decimal w_p_star,
                         decimal previous_x_p,
                         Layer[] layers = null)
        {
            // Температура под фронтом сублимации
            T_I_avg = t_I_avg;
            
            // Температура слоя над фронтом сублимации
            T_II_avg = t_II_avg;
            
            // Положение фронта сублимации
            X_p = x_p_star * c.L;
            X_p_star = x_p_star;
            previous_X_p = previous_x_p;

            W_p_star = w_p_star;
            
            // Положение фронта сублимации в слоях
            var j_p_notRounded = c.N - (((c.L - X_p) * c.N) / c.L);
            j_p = (int)decimal.Round(j_p_notRounded, 0, MidpointRounding.AwayFromZero);

            Layers = layers ?? InitLayers(j_p, c);
        }

        public decimal T_I_avg { get; }

        public decimal T_II_avg { get; }
        
        public decimal X_p { get; }
        public decimal X_p_star { get; }
        public decimal previous_X_p { get; }

        public decimal W_p_star { get; }

        public int j_p { get; }

        public Layer[] Layers { get; }
        
        public static Layer[] InitLayers(int j_front, Constants c)
        {
            return Enumerable.Range(0, c.N + 1)
                             .Select(layerNumber => new Layer(layerNumber, j_front, c))
                             .ToArray();
        }
    }
}