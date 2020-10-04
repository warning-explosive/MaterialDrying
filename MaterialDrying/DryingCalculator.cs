// ReSharper disable InconsistentNaming
namespace MaterialDrying
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class DryingCalculator
    {
        private readonly Constants _c;

        public DryingCalculator(Constants constants)
        {
            _c = constants;
        }

        public ICollection<Frame> Calculate()
        {
            var collection = new List<Frame>();

            CalculateFirstPeriod(collection, Report);
            Console.WriteLine("First period was calculated.");
            
            CalculateSecondPeriod(collection, Report);
            Console.WriteLine("Second period was calculated.");
            
            return collection;
        }

        void Report(Frame frame) => Console.WriteLine($"{nameof(Frame.Index)} = {frame.Index}; {nameof(Frame.Wp_new)} = {frame.Wp_new}; {nameof(Constants.T_shelf)} = {_c.T_shelf(frame.Index)}; {nameof(Frame.Tm)} = {frame.Tm}");
        
        private void CalculateFirstPeriod(ICollection<Frame> frames, Action<Frame> report)
        {
            ulong i = 1;
            var prev = new Frame(0,
                                 253.15m,
                                 1m,
                                 1m,
                                 1m,
                                 1m,
                                 253.15m,
                                 253.15m,
                                 0.99m,
                                 0.0131m,
                                 _c.Wp_primary);
            frames.Add(prev);
            report(prev);
            
            for (;
                FirstPeriodRunCondition(prev) && i < ulong.MaxValue;
                ++i)
            {
                // 1
                var TI_predictor = prev.TI_predictor + _c.del_t_bezr * _c.Q_I_bezr;
                
                // 2
                var TII_predictor = prev.TII_predictor + _c.del_t_bezr * _c.Q_II_bezr;
                
                // 3
                var qv_I_corrector = (TI_predictor - TII_predictor) / _c.del_x_bezr;
                
                // 4
                var del_W_1 = _c.del_t_bezr * (qv_I_corrector) * _c.Ste_I;
                
                // 5
                var del_W_2 = _c.del_t_bezr * (qv_I_corrector) * _c.Ste_II;
                
                // 6
                var W_bezr = del_W_1 + del_W_2 + prev.W_bezr;
                
                // 7
                var Wp_new = W_bezr * (_c.Wp_primary - _c.Weq_primary) + _c.Weq_primary;
                
                // 8
                var Tnext = _c.a_e * _c.del_tr * (_c.T_shelf(i) - 2m * prev.Tnext + _c.T_shelf(i)) / (_c.del_X * _c.del_X) + prev.Tnext;
                
                // 9
                var Tpred = _c.a_e * _c.del_tr * (_c.T_shelf(i) - 2m * prev.Tpred + Tnext) / (_c.del_X * _c.del_X) + prev.Tpred;
                
                // 10
                var Tm = _c.a_e * _c.del_tr * (Tnext - 2m * prev.Tm + Tpred) / (_c.del_X * _c.del_X) + prev.Tm;
                
                var curr = new Frame(i,
                                     Tm,
                                     TI_predictor,
                                     TII_predictor,
                                     prev.TI_corrector,
                                     prev.TII_corrector,
                                     Tpred,
                                     Tnext,
                                     W_bezr,
                                     prev.t_bezrazm,
                                     Wp_new);
                prev = curr;
                frames.Add(curr);
                report(curr);
            }
        }

        private bool FirstPeriodRunCondition(Frame prev)
        {
            Debug.Assert(_c.eps > 0m, "eps must be positive decimal number!");
            return prev.Wp_new > _c.Weq + _c.eps;
        }

        private void CalculateSecondPeriod(ICollection<Frame> frames, Action<Frame> report)
        {
            var prev = frames.Last();
            var i = prev.Index;

            for (;
                SecondPeriodRunCondition(prev) && i < ulong.MaxValue;
                ++i)
            {
                // 1
                var Tnext = _c.a_e * _c.delta_t * (_c.T_shelf(i) - 2m * prev.Tnext + _c.T_shelf(i)) / (_c.del_X * _c.del_X) + prev.Tnext;
                
                // 2 
                var Tpred = _c.a_e * _c.delta_t * (_c.T_shelf(i) - 2m * prev.Tpred + Tnext) / (_c.del_X * _c.del_X) + prev.Tpred;
                
                // 3
                var Tm = _c.a_e * _c.delta_t * (Tnext - 2m * prev.Tm + Tpred) / (_c.del_X * _c.del_X) + prev.Tm;
                
                // 4
                var Dk = 1.0638m * _c.rp * Sqrt(_c.Rgaz * Tm / _c.M);
                
                // 5
                var Ds = _c.Dso * Exp(- _c.Eo / (_c.Rgaz * Tm));
                
                // 6
                var nominator = _c.Wo * Exp(_c.const_a / Tm);
                var dn_part = prev.Wp_new - _c.Wo * Exp(_c.const_a / Tm);
                var denominator = _c.const_b * Exp(_c.const_c / Tm) * _c.P * dn_part * dn_part;
                var Y_w = nominator / denominator;
                
                // 7
                var Deff = Ds + Dk * _c.e_por * _c.M * Y_w / _c.Ro_II;
                
                // 8
                var Ki = 60m * Deff / (_c.dz * _c.dz);
                
                // 9
                var t_bezrazm = prev.t_bezrazm + 0.0131m;
                
                // 10
                var W_bezr = Exp(- _c.Lm * _c.Lm * Ki * t_bezrazm / _c.a_e);
                
                // 11
                var Wp_new = W_bezr * (_c.Wp_secondary - _c.Weq_secondary) +_c.Weq_secondary;
                
                var curr = new Frame(i,
                                     Tm,
                                     prev.TI_predictor,
                                     prev.TII_predictor,
                                     prev.TI_corrector,
                                     prev.TII_corrector,
                                     Tpred,
                                     Tnext,
                                     W_bezr,
                                     t_bezrazm,
                                     Wp_new);
                prev = curr;
                frames.Add(curr);
                report(curr);
            }
        }

        private bool SecondPeriodRunCondition(Frame prev)
        {
            Debug.Assert(_c.eps > 0m, "eps must be positive decimal number!");
            return prev.Wp_new > _c.Weq_secondary + _c.eps;
        }

        private static decimal Exp(decimal power)
        {
            return (decimal) Math.Exp((double) power);
        }

        private static decimal Sqrt(decimal number)
        {
            return (decimal) Math.Sqrt((double) number);
        }
    }
}