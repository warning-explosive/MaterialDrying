// ReSharper disable InconsistentNaming
namespace MaterialDrying
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using YamlDotNet.Serialization;

    public class Constants
    {
        #region constants

        /// <summary> 1 </summary>
        public uint N { get; set; } = 10;
        
        /// <summary> 2 </summary>
        public decimal Lm { get; set; } = 0.01m;

        /// <summary> 3 </summary>
        public decimal E { get; set; } = 800m;

        /// <summary> 4 </summary>
        public decimal P { get; set; } = 100m;

        /// <summary> 5 </summary>
        public decimal a_e { get; set; } = 8e-9m;

        /// <summary> 6 </summary>
        public decimal Weq { get; set; } = 0.15m;

        /// <summary> 7 </summary>
        public decimal Mu_2_I { get; set; } = 0.09m;
        
        /// <summary> 8 </summary>
        public decimal Mu_2_II { get; set; } = 0.09m;

        /// <summary> 9 </summary>
        public decimal ke_I { get; set; } = 1.383m;

        /// <summary> 10 </summary>
        public decimal ke_II { get; set; } = 0.546m;

        /// <summary> 11 </summary>
        public decimal T_inf { get; set; } = 253.15m;

        /// <summary> 12 </summary>
        public decimal Tsec { get; set; } = 253.15m;
        
        /// <summary> 13 </summary>
        public decimal Ts3 { get; set; } = 273.2m;

        /// <summary> 14 </summary>
        public decimal Tref { get; set; } = 293.15m;

        /// <summary> 15 </summary>
        public decimal Ro_e_I { get; set; } = 978.1m;

        /// <summary> 16 </summary>
        public decimal Ro_bu_II { get; set; } = 1040m;

        /// <summary> 17 </summary>
        public decimal C_p_e_I { get; set; } = 3055m;
        
        /// <summary> 18 </summary>
        public decimal C_p_e_II { get; set; } = 4060m;
        
        /// <summary> 19 </summary>
        public decimal del_h { get; set; } = 2616666.7m;

        /// <summary> 20 </summary>
        public decimal Wp_primary { get; set; } = 0.98m;

        /// <summary> 21 </summary>
        public decimal Weq_primary { get; set; } = 0.15m;

        /// <summary> 22  </summary>
        public decimal del_tr { get; set; } = 100m;

        /// <summary> 23 </summary>
        public decimal delta_t { get; set; } = 100m;

        /// <summary> 24 </summary>
        public decimal del_X { get; set; } = 0.01m;

        /// <summary> 25 </summary>
        public decimal Wp_secondary { get; set; } = 0.15m;

        /// <summary> 26 </summary>
        public decimal Weq_secondary { get; set; } = 0.04m;

        /// <summary> 27 </summary>
        public decimal rp { get; set; } = 4.21e-9m;

        /// <summary> 28 </summary>
        public decimal Rgaz { get; set; } = 8.314m;

        /// <summary> 29 </summary>
        public decimal Ro_II { get; set; } = 1040m;

        /// <summary> 30 </summary>
        public decimal M { get; set; } = 0.018m;

        /// <summary> 31 </summary>
        public decimal dz { get; set; } = 0.001m;

        /// <summary> 32 </summary>
        public decimal e_por { get; set; } = 0.667m;

        /// <summary> 33 </summary>
        public decimal Eo { get; set; } = 4390m;

        /// <summary> 34 </summary>
        public decimal Dso { get; set; } = 1e-9m;

        /// <summary> 35 </summary>
        public decimal Wo { get; set; } = 0.3703m;

        /// <summary> 36 </summary>
        public decimal const_a { get; set; } = 1387.82m;

        /// <summary> 37 </summary>
        public decimal const_b { get; set; } = 1.7e-7m;

        /// <summary> 38 </summary>
        public decimal const_c { get; set; } = 1511.02m;

        /// <summary> 39 </summary>
        public decimal eps { get; set; } = 1e-6m;
        
        #endregion
        
        #region calculated constants

        /// <summary> 1 </summary>
        [YamlIgnore]
        public decimal delta_x => Lm / N;

        /// <summary> 2 </summary>
        [YamlIgnore]
        public decimal del_x_bezr => delta_x / Lm; // TODO: -> N

        /// <summary> 3 </summary>
        [YamlIgnore]
        public decimal del_t_bezr => 0.5m * del_x_bezr * del_x_bezr;

        /// <summary> 4 </summary>
        [YamlIgnore]
        public decimal Q_I => E * E * Mu_2_I;

        /// <summary> 5 </summary>
        [YamlIgnore]
        public decimal Q_II => E * E * Mu_2_II;

        /// <summary> 6 </summary>
        [YamlIgnore]
        public decimal Q_I_bezr => (Lm * Lm * Q_I) / ((Tsec - Ts3) * ke_I);

        /// <summary> 7 </summary>
        [YamlIgnore]
        public decimal Q_II_bezr => (Lm * Lm * Q_II) / ((Tsec - Tref) * ke_II);

        /// <summary> 8 </summary>
        [YamlIgnore]
        public decimal Ste_I => (Ro_e_I * C_p_e_I * (Tsec - Ts3)) / (Ro_bu_II * del_h * (Wp_primary - Weq_primary));

        /// <summary> 9 </summary>
        [YamlIgnore]
        public decimal Ste_II => (C_p_e_II * (Tsec - Tref)) / (del_h * (Wp_primary - Weq_primary));

        #endregion
        
        #region t_shelf

        /// <summary>
        /// Границы для T_shelf
        /// </summary>
        public Dictionary<Boundary, decimal> Boundaries { get; set; }
            = new Dictionary<Boundary, decimal>
              {
                  [new Boundary(decimal.MinValue, decimal.Zero)] = decimal.Zero,
                  [new Boundary(decimal.Zero, decimal.MaxValue)] = decimal.Zero,
              };

        public decimal T_shelf(ulong iteration) => Boundaries
            .OrderBy(z => z.Key)
            .FirstOrDefault(z => z.Key.BetweenInclude(iteration * delta_t))
            .Value;
        
        public struct Boundary : IComparer<Boundary>,
                                 IComparable<Boundary>
        {
            public Boundary(decimal left, decimal right)
            {
                Left = left;
                Right = right;
            }

            public decimal Left { get; set; }
            public decimal Right { get; set; }

            public bool BetweenExclude(decimal x, Boundary other)
            {
                return other.Left < x && x < other.Right;
            }

            public bool BetweenInclude(decimal x, Boundary other)
            {
                return other.Left <= x && x <= other.Right;
            }

            public bool BetweenInclude(decimal x)
            {
                return BetweenInclude(x, this);
            }

            public int Compare(Boundary x, Boundary y)
            {
                if (BetweenExclude(x.Left, y)
                    || BetweenExclude(x.Right, y))
                {
                    throw new InvalidOperationException($"Границы {nameof(T_shelf)} указаны неверно (пересекаются)");
                }

                var left = x.Left.CompareTo(y.Left);
                var right = x.Right.CompareTo(y.Right);

                if (left < 0 && right < 0)
                {
                    return -1;
                }

                if (left > 0 && right > 0)
                {
                    return 1;
                }

                return 0;
            }

            public int CompareTo(Boundary other)
            {
                return Compare(this, other);
            }
        }
        
        #endregion
    }
}