// ReSharper disable InconsistentNaming
namespace MaterialDrying
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DryingCalculator
    {
        private readonly Constants _c;

        public DryingCalculator(Constants constants)
        {
            _c = constants;
        }

        public IEnumerable<ExportData> Calculate()
        {
            var initialFrontPosition = Init();

            var position = initialFrontPosition;
            uint currentStep = 0;

            /*
             * Пересчет временного ограничения в кол-во итераций
             */
            var cutOffTime = (uint)decimal.Round(_c.DryingTime / _c.delta_t, MidpointRounding.AwayFromZero);
            Console.WriteLine($"\nCutOff time: {cutOffTime} iterations");
            
            var result = new List<ExportData>((int)cutOffTime);
            
            /*
             * Продолжаем считать если не дошли до дна стакана
             *     исходя из области определения T-корректора стакана считаем 1й слой материала
             * И
             * не превышено ограничение по времени
             */
            while (position.j_p > 1
                   && position.j_p < 49
                   && currentStep < cutOffTime)
            {
                /*
                 * Расчет следущего состояния
                 */
                Console.WriteLine($"Time step: {currentStep}");
                position = Calculate(currentStep, position, out var exportData);
                result.Add(exportData);

                ++currentStep;
            }

            return result;
        }

        private Iteration Init()
        {
            /*
             * Задаем начальные температуры и положение рабочей точки
             */
            var initialFrontPosition = new Iteration(_c,
                                                     _c.T_s_eq,
                                                     _c.T_s_eq,
                                                     _c.X_p_start / _c.L,
                                                     _c.W_p_star,
                                                     _c.X_p_start);

            foreach (var layer in initialFrontPosition.Layers)
            {
                layer.q_I_predictor = 0;
                layer.T_I_corrector = Conversions.T_I_to_star(_c, _c.T_s_eq);
                
                layer.q_II_predictor = 0;
                layer.T_II_corrector = Conversions.T_II_to_star(_c, _c.T_s_eq);
            }

            return initialFrontPosition;
        }

        private Iteration Calculate(uint step, Iteration previous, out ExportData exportData)
        {
            // 1 - Расчет Q
            var Q_nu_I = ((_c.Mu_1_I * previous.T_I_avg) + _c.Mu_2_I) * _c.E * _c.E; // (7)
            var Q_nu_I_star = ((Q_nu_I * _c.L * _c.L) / _c.K_e_I) / (_c.T_s_eq - _c.T_s_3); // (8)
            var Q_nu_II = ((_c.Mu_1_II * previous.T_II_avg) + _c.Mu_2_II) * _c.E * _c.E; // (9)
            var Q_nu_II_star = ((Q_nu_II * _c.L * _c.L) / _c.K_e_II) / (_c.T_s_eq - _c.T_II_ref); // (10)
            
            // 2 - Расчет N_w
            var delta_X_p = previous.previous_X_p - previous.X_p;
            var N_w = ((_c.W_p - _c.W_eq) * _c.Ro_vi_II * delta_X_p) / _c.delta_t; // (12)
            var N_w_star = (_c.L * _c.C_p_w * N_w) / _c.K_e_II; // (13)

            var layers = Iteration.InitLayers(previous.j_p, _c);

            for (var j = 0; j < _c.N; ++j)
            {
                var layer = layers[j];

                decimal q_predictor() => (previous.Layers[j].T_corrector() - previous.Layers[j + 1].T_corrector()) / _c.delta_x_star; // (14), (16)
                decimal q_corrector() => (layers[j - 1].T_predictor() - layer.T_predictor()) / _c.delta_x_star; // (18), (20)
                
                // 3 - Шаг предиктора I
                if (layer.is_q_I_predictor())
                {
                    layer.q_I_predictor = q_predictor();

                    if (layer.is_T_I_predictor())
                    {
                        var T_I_predictor = previous.Layers[j].T_corrector()
                                            + (_c.delta_t_star * Q_nu_I_star)
                                            - ((_c.delta_t_star * (previous.Layers[j + 1].q_predictor() - previous.Layers[j].q_predictor())) / _c.delta_x_star); // (15)

                        layer.T_I_predictor = T_I_predictor;
                    }
                }
                
                // 3 - Шаг предиктора II
                if (layer.is_T_II_predictor())
                {
                    if (layer.is_q_II_predictor())
                    {
                        layer.q_II_predictor = q_predictor();
                    }
                    
                    var T_II_predictor = previous.Layers[j].T_corrector()
                                         + (_c.delta_t_star * (Q_nu_II_star + (N_w_star * previous.Layers[j].q_predictor())))
                                         - ((_c.delta_t_star * (previous.Layers[j + 1].q_predictor() - previous.Layers[j].q_predictor())) / _c.delta_x_star); // (17)

                    layer.T_II_predictor = T_II_predictor;
                }
                
                // 3 - Шаг корректора I
                if (layer.is_q_I_corrector())
                {
                    layer.q_I_corrector = q_corrector(); // (18)

                    if (layer.is_T_I_corrector())
                    {
                        var T_I_corrector = (previous.Layers[j].T_corrector()
                                             + layer.T_predictor()
                                             - (_c.delta_t_star * (layer.q_corrector() - layers[j - 1].q_corrector()))
                                             + (_c.delta_t_star * Q_nu_I_star))
                                            / 2m; // (19)

                        layer.T_I_corrector = T_I_corrector;
                    }
                }

                // 3 - Шаг корректора II
                if (layer.is_q_II_corrector())
                {
                    layer.q_II_corrector = q_corrector(); // (20)
                            
                    if (layer.is_T_II_corrector())
                    {
                        var T_II_corrector = (previous.Layers[j].T_corrector()
                                              + layer.T_predictor()
                                              - (_c.delta_t_star * (layer.q_corrector() - layers[j - 1].q_corrector()))
                                              + (_c.delta_t_star * (Q_nu_II_star + (N_w_star * layer.q_corrector()))))
                                             / 2m; // (21)

                        layer.T_II_corrector = T_II_corrector;
                    }
                }
            }

            // 4 - Получаем средние температуры слоев
            var T_I_corrector_avg = layers.Where(z => z.is_T_I_corrector()).Select(z => z.T_I_corrector.Value).Average();
            var T_II_corrector_avg = layers.Where(z => z.is_T_II_corrector()).Select(z => z.T_II_corrector.Value).Average();
            
            // 5 - Переводим температуры слоев материала в размерный вид
            var T_I_avg_converted = Conversions.T_I_to_dimensional(_c, T_I_corrector_avg); // (22)
            var T_II_avg_converted = Conversions.T_II_to_dimensional(_c, T_II_corrector_avg); // (23)
            
            // 6 - Приращение
            var delta_X_I = layers[previous.j_p].q_corrector() * _c.S_te_nu_I * _c.delta_t_star; // (29)
            var delta_X_II = layers[previous.j_p].q_corrector() * _c.S_te_nu_II * _c.delta_t_star; // (30)
            var abs_delta_sum = Math.Abs(delta_X_I + delta_X_II);
            
            
            // 7 - Новое положение рабочей точки
            var new_X_p_star = previous.X_p_star - abs_delta_sum; // (31)
            var new_W_p_star = previous.W_p_star - abs_delta_sum; // (32)

            var iteration = new Iteration(_c,
                                          T_I_avg_converted,
                                          T_II_avg_converted,
                                          new_X_p_star,
                                          new_W_p_star,
                                          previous.X_p,
                                          layers);
            
            exportData = new ExportData
            {
                Step = step,
                
                j_p = iteration.j_p,
                X_p = iteration.X_p,
                X_p_star = iteration.X_p_star,
                W_p_star = iteration.W_p_star,
                
                delta_X_I = delta_X_I,
                delta_X_II = delta_X_II,
                abs_delta_sum = abs_delta_sum,
                
                q_p_corrector = layers[previous.j_p].q_corrector(),
                
                T_I_avg = T_I_avg_converted,
                T_I_star_avg = T_I_corrector_avg,
                
                T_II_avg = T_II_avg_converted,
                T_II_star_avg = T_II_corrector_avg,
            };

            return iteration;
        }
    }
}