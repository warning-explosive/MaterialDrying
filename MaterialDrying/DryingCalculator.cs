// ReSharper disable InconsistentNaming
namespace MaterialDrying
{
    using System;
    using System.Collections.Generic;

    public class DryingCalculator
    {
        private readonly Constants _c;

        public DryingCalculator(Constants constants)
        {
            _c = constants;
        }

        public IEnumerable<ExportData> Calculate()
        {
            /*
             * Задаем начальные температуры и положение рабочей точки
             */
            var initialFrontPosition = new Iteration(_c,
                                                     _c.T_s_eq,
                                                     Iteration.T_I_to_star(_c, _c.T_s_eq),
                                                     _c.T_s_eq,
                                                     Iteration.T_II_to_star(_c, _c.T_s_eq),
                                                     _c.T_s_eq,
                                                     Iteration.T_II_to_star(_c, _c.T_s_eq),
                                                     _c.X_p_start / _c.L,
                                                     _c.W_p_star,
                                                     _c.X_p_start);

            var position = initialFrontPosition;
            uint currentTime = 0;

            /*
             * Пересчет временного ограничения в кол-во итераций
             */
            var cutOffTime = (uint)decimal.Round(_c.DryingTime / _c.delta_t, MidpointRounding.AwayFromZero);
            Console.WriteLine($"\nCutOff time: {cutOffTime} iterations");
            
            var result = new List<ExportData>((int)cutOffTime);
            
            /*
             * Продолжаем считать если не дошли до дна стакана
             * И
             * не превышено ограничение по времени
             */
            while (position.j_p > 0 && currentTime < cutOffTime)
            {
               /*
                * Расчет следующего состояния
                */
                position = Calculate(position, out var exportData);
                exportData.Step = currentTime;
                result.Add(exportData);
                
                ++currentTime;
            }

            return result;
        }
        
        private Iteration Calculate(Iteration previous, out ExportData exportData)
        {
            // 1 - Расчет Q
            var Q_nu_I = ((_c.Mu_1_I * previous.T_I) + _c.Mu_2_I) * _c.E * _c.E; // (7)
            var Q_nu_I_star = ((Q_nu_I * _c.L * _c.L) / _c.K_e_I) / (_c.T_s_eq - _c.T_s_3); // (8)
            var Q_nu_II = ((_c.Mu_1_II * previous.T_II) + _c.Mu_2_II) * _c.E * _c.E; // (9)
            var Q_nu_II_star = ((Q_nu_II * _c.L * _c.L) / _c.K_e_II) / (_c.T_s_eq - _c.T_II_ref); // (10)
            
            // 2 - Расчет N_w
            var delta_X_p = previous.previous_X_p - previous.X_p;
            var N_w = ((_c.W_p - _c.W_eq) * _c.Ro_vi_II * delta_X_p) / _c.delta_t; // (12)
            var N_w_star = (_c.L * _c.C_p_w * N_w) / _c.K_e_II; // (13)
            
            // 3 - Шаг предиктора
            var q_I_predictor = (previous.T_I_star - previous.T_II_star) / _c.delta_x_star; // (14)
            var q_II_predictor = (previous.T_II_star - previous.T_L_star) / _c.delta_x_star; // (16)

            var T_I_predictor = previous.T_I_star
                                + (_c.delta_t_star * Q_nu_I_star)
                                - ((_c.delta_t_star * (q_II_predictor - q_I_predictor)) / _c.delta_x_star); // (15)
            var T_II_predictor = previous.T_II_star
                                 + (_c.delta_t_star * (Q_nu_II_star + (N_w_star * q_II_predictor)))
                                 - ((_c.delta_t_star * (q_II_predictor - q_I_predictor)) / _c.delta_x_star); // (17)

            var T_II_predictor_dimensional = Iteration.T_II_to_dimensional(_c, T_II_predictor); // -> 23
            var T_L_predictor_dimensional = Iteration.T_L_dimensional(_c, T_II_predictor_dimensional); // -> 24
            var T_L_predictor = Iteration.T_II_to_star(_c, T_L_predictor_dimensional); // -> 25
            
            // 4 - Шаг корректора
            var q_I_corrector = (T_I_predictor - T_II_predictor) / _c.delta_x_star; // (18)
            var q_II_corrector = (T_II_predictor - T_L_predictor) / _c.delta_x_star; // (20)

            var T_I_corrector = (previous.T_I_star
                                 + T_I_predictor
                                 - (_c.delta_t_star * (q_II_corrector - q_I_corrector))
                                 + (_c.delta_t_star * Q_nu_I_star)) / 2m; // (19)
            var T_II_corrector = (previous.T_II_star
                                 + T_II_predictor
                                 - (_c.delta_t_star * (q_II_corrector - q_I_corrector))
                                 + (_c.delta_t_star * (Q_nu_II_star + (N_w_star * q_II_corrector)))) / 2m; // (21)
            
            // 5 - Переводим температуры слоев материала в размерный вид
            var T_I_converted = Iteration.T_I_to_dimensional(_c, T_I_corrector); // (22)
            var T_II_converted = Iteration.T_II_to_dimensional(_c, T_II_corrector); // (23)
            
            // 6 - Температура на поверхности материала
            var T_L = Iteration.T_L_dimensional(_c, T_II_converted); // (24)
            var T_L_star = Iteration.T_II_to_star(_c, T_L); // (25)
            
            // 7 - q_L - на поверхности материала
            var q_L_star = (T_II_corrector - T_L_star) / _c.delta_x_star; // (26)
            
            // 8 - Приращение
            var delta_X_I = q_I_corrector * Math.Abs(_c.S_te_nu_I) * _c.delta_t_star; // (29) TODO q_I_corrector -> q_II_corrector
            var delta_X_II = q_L_star * Math.Abs(_c.S_te_nu_II) * _c.delta_t_star; // (30)

            // 9 - Новое положение рабочей точки
            var new_X_p_star = previous.X_p_star + delta_X_I + delta_X_II; // (31)
            var new_W_p_star = previous.W_p_star + delta_X_I + delta_X_II; // (32)

            var iteration = new Iteration(_c,
                                          T_I_converted,
                                          T_I_corrector,
                                          T_II_converted,
                                          T_II_corrector,
                                          T_L,
                                          T_L_star,
                                          new_X_p_star,
                                          new_W_p_star,
                                          previous.X_p);
            
            exportData = new ExportData
            {
                j_p = iteration.j_p,
                X_p = iteration.X_p,
                X_p_star = iteration.X_p_star,
                
                W_p_star = iteration.W_p_star,
                
                T_I = iteration.T_I,
                T_I_predictor = T_I_predictor,
                T_I_corrector = T_I_corrector,
                
                T_II = iteration.T_II,
                T_II_predictor = T_II_predictor,
                T_II_corrector = T_II_corrector,
                
                T_L = iteration.T_L,
                T_L_star = iteration.T_L_star,
                T_L_predictor = T_L_predictor,

                q_I_predictor = q_I_predictor,
                q_II_predictor = q_II_predictor,
                
                q_I_corrector = q_I_corrector,
                q_II_correcor = q_II_corrector,
                
                q_L_star = q_L_star,
                
                delta_X_I = delta_X_I,
                delta_X_II = delta_X_II
            };

            return iteration;
        }
    }
}