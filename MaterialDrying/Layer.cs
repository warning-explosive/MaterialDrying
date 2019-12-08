// ReSharper disable InconsistentNaming
namespace MaterialDrying
{
    using System;

    public class Layer
    {
        public Layer(int j, int j_p, Constants c)
        {
            this.j = j;
            this.j_p = j_p;
            this.c = c;
        }

        private int j { get; }
        private int j_p { get; }
        private Constants c { get; }
        
        public decimal? q_I_predictor { private get; set; }
        public decimal? T_I_predictor { private get; set; }
        public decimal? q_I_corrector { private get; set; }
        public decimal? T_I_corrector { get; set; }
        
        public decimal? q_II_predictor { private get; set; }
        public decimal? T_II_predictor { private get; set; }
        public decimal? q_II_corrector { private get; set; }
        public decimal? T_II_corrector { get; set; }
        
        // область определения T_0_corrector
        private bool is_T_0_corrector() => j.BetweenInclude(0, 1);
        // область определения T_I_corrector
        public bool is_T_I_corrector() => j.BetweenInclude(2, j_p);
        // область определения T_II_corrector
        public bool is_T_II_corrector() => j.BetweenInclude(j_p + 1, c.N - 1);
        // область определения T_L_corrector
        private bool is_T_L_corrector() => j.BetweenInclude(c.N, c.N);

        public decimal T_corrector()
        {
            if (is_T_0_corrector())
            {
                return 1m;
            }

            if (is_T_I_corrector())
            {
                return T_I_corrector ?? throw new ArgumentOutOfRangeException(nameof(T_I_corrector));
            }
            
            if (is_T_II_corrector())
            {
                return T_II_corrector ?? throw new ArgumentOutOfRangeException(nameof(T_II_corrector));
            }

            if (is_T_L_corrector())
            {
                return 1m;
            }

            throw new ArgumentOutOfRangeException(nameof(j), j, nameof(T_corrector));
        }
        
        // область определения q_I_predictor
        public bool is_q_I_predictor() => j.BetweenInclude(0, j_p);
        // область определения q_II_predictor
        public bool is_q_II_predictor() => j.BetweenInclude(j_p + 1, c.N - 1);
        // область определения q_L_predictor
        private bool is_q_L_predictor() => j.BetweenInclude(c.N, c.N);

        public decimal q_predictor()
        {
            if (is_q_I_predictor())
            {
                return q_I_predictor ?? throw new ArgumentOutOfRangeException(nameof(q_I_predictor));
            }

            if (is_q_II_predictor())
            {
                return q_II_predictor ?? throw new ArgumentOutOfRangeException(nameof(q_II_predictor));
            }

            if (is_q_L_predictor())
            {
                return 0m;
            }
            
            throw new ArgumentOutOfRangeException(nameof(j), j, nameof(q_predictor));
        }

        // область определения T_I_predictor
        public bool is_T_I_predictor() => j.BetweenInclude(0, j_p - 1);
        // область определения T_II_predictor
        public bool is_T_II_predictor() => j.BetweenInclude(j_p, c.N - 1);
        // область определения T_L_predictor
        private bool is_T_L_predictor() => j.BetweenInclude(c.N, c.N);

        public decimal T_predictor()
        {
            if (is_T_I_predictor())
            {
                return T_I_predictor ?? throw new ArgumentOutOfRangeException(nameof(T_I_predictor));
            }
            
            if (is_T_II_predictor())
            {
                return T_II_predictor ?? throw new ArgumentOutOfRangeException(nameof(T_II_predictor));
            }

            if (is_T_L_predictor())
            {
                return 1m;
            }
            
            throw new ArgumentOutOfRangeException(nameof(j), j, nameof(T_predictor));
        }
        
        // область определения q_I_corrector
        public bool is_q_I_corrector() => j.BetweenInclude(1, j_p);
        // область определения q_II_corrector
        public bool is_q_II_corrector() => j.BetweenInclude(j_p + 1, c.N - 1);
        // область определения q_L_corrector
        private bool is_q_L_corrector() => j.BetweenInclude(c.N, c.N);

        public decimal q_corrector()
        {
            if (is_q_I_corrector())
            {
                return q_I_corrector ?? throw new ArgumentOutOfRangeException(nameof(q_I_corrector));
            }
            
            if (is_q_II_corrector())
            {
                return q_II_corrector ?? throw new ArgumentOutOfRangeException(nameof(q_II_corrector));
            }
            
            if (is_q_L_corrector())
            {
                return 0m;
            }
            
            throw new ArgumentOutOfRangeException(nameof(j), j, nameof(q_corrector));
        }
    }
}