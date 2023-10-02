using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Record_Types
{
    class e_state_types
    {
        protected enum pwm_bits
        {
            ch1 = 0, ch2 = 1, ch3 = 2, ch4 = 3,
        }
        protected enum dio_low_bits
        {
            ch1 = 0, ch2 = 1, ch3 = 2, ch4 = 3,
        }
        protected enum dio_high_bits
        {
            ch1 = 4, ch2 = 5, ch3 = 6, ch4 = 7,
        }
        protected enum e_systems_control
        {
            reporting = 0,
            timer5 = 1,
        }
    }
}
