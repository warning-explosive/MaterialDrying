namespace MaterialDrying
{
    public class Frame
    {
        public Frame(ulong index,
                     decimal tm,
                     decimal tI_predictor,
                     decimal tII_predictor,
                     decimal tI_corrector,
                     decimal tII_corrector,
                     decimal tpred,
                     decimal tnext,
                     decimal w_bezr,
                     decimal T_bezrazm,
                     decimal wp_new)
        {
            Index = index;
            Tm = tm;
            TI_predictor = tI_predictor;
            TII_predictor = tII_predictor;
            TI_corrector = tI_corrector;
            TII_corrector = tII_corrector;
            Tpred = tpred;
            Tnext = tnext;
            W_bezr = w_bezr;
            t_bezrazm = T_bezrazm;
            Wp_new = wp_new;
        }

        public ulong Index { get; }

        public decimal Tm { get; }
        
        public decimal TI_predictor { get; }
        
        public decimal TII_predictor { get; }
        
        public decimal TI_corrector { get; }
        
        public decimal TII_corrector { get; }
        
        public decimal Tpred { get; }
        
        public decimal Tnext { get; }
        
        public decimal W_bezr { get; }
        
        public decimal t_bezrazm { get; }

        public decimal Wp_new { get; }
    }
}